using System.Text.Json;
using CodeQuest.Auth;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services.Ai;
using CodeQuest.Services.Regras;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>Dados para criar uma missão manual (RF09).</summary>
/// <param name="Titulo">Título da missão.</param>
/// <param name="Esforco">Esforço estimado, define a faixa de XP (RN04).</param>
/// <param name="Xp">XP escolhido pelo usuário dentro da faixa.</param>
/// <param name="Descricao">Descrição opcional.</param>
/// <param name="TopicoId">Tópico foco opcional.</param>
public readonly record struct NovaMissaoManual(
    string Titulo,
    EsforcoMissao Esforco,
    int Xp,
    string? Descricao = null,
    int? TopicoId = null);

/// <summary>
/// Missões (RF09–RF13). Nesta fase cobre o fluxo manual: criar validando a faixa de XP
/// (RN04), derivar o gold (RN02) e, ao concluir, creditar as recompensas via GameService.
/// </summary>
public interface IMissaoService
{
    Task<Resultado<Missao>> CriarManualAsync(NovaMissaoManual dados, CancellationToken ct = default);

    /// <summary>Estrutura uma missão a partir de texto livre via IA (RF10).</summary>
    Task<Resultado<Missao>> SugerirAsync(string textoLivre, CancellationToken ct = default);

    /// <summary>Conclui uma missão pendente/em andamento e credita XP+gold ao jogador.</summary>
    Task<Resultado<ResultadoXp>> ConcluirAsync(int missaoId, CancellationToken ct = default);

    /// <summary>Missões de hoje; gera as automáticas (RF11/RF16) na primeira consulta do dia.</summary>
    Task<IReadOnlyList<Missao>> ListarDoDiaAsync(CancellationToken ct = default);
}

public sealed class MissaoService : IMissaoService
{
    /// <summary>Quantas missões automáticas (revisão + gaps) o dia deve ter (RF11).</summary>
    private const int MetaMissoesAutomaticas = 3;

    private readonly AppDbContext _db;
    private readonly IPoliticaRecompensaMissao _politica;
    private readonly ICalculadoraRecompensa _recompensa;
    private readonly IGameService _game;
    private readonly IGeminiClient _gemini;
    private readonly IRelogio _relogio;
    private readonly IUsuarioAtual _usuario;

    public MissaoService(
        AppDbContext db,
        IPoliticaRecompensaMissao politica,
        ICalculadoraRecompensa recompensa,
        IGameService game,
        IGeminiClient gemini,
        IRelogio relogio,
        IUsuarioAtual usuario)
    {
        _db = db;
        _politica = politica;
        _recompensa = recompensa;
        _game = game;
        _gemini = gemini;
        _relogio = relogio;
        _usuario = usuario;
    }

    public async Task<Resultado<Missao>> CriarManualAsync(NovaMissaoManual dados, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dados.Titulo))
            return Resultado.Falha<Missao>("Título é obrigatório.");

        // RN04 — XP precisa cair na faixa do esforço escolhido.
        if (!_politica.XpValido(dados.Esforco, dados.Xp))
        {
            var faixa = _politica.FaixaDe(dados.Esforco);
            return Resultado.Falha<Missao>(
                $"XP {dados.Xp} fora da faixa de esforço {dados.Esforco} ({faixa.Minimo}–{faixa.Maximo}).");
        }

        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        var missao = new Missao
        {
            PlayerId = playerId,
            Tipo = TipoMissao.Manual,
            Titulo = dados.Titulo.Trim(),
            Descricao = dados.Descricao?.Trim() ?? string.Empty,
            TopicoId = dados.TopicoId,
            XpRecompensa = dados.Xp,
            GoldRecompensa = _recompensa.GoldPorXp(dados.Xp), // RN02
            Status = StatusMissao.Pendente,
            DataAlvo = _relogio.Hoje,
        };

        _db.Missoes.Add(missao);
        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(missao);
    }

    public async Task<Resultado<Missao>> SugerirAsync(string textoLivre, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(textoLivre))
            return Resultado.Falha<Missao>("Descreva o que você quer praticar.");

        MissaoSugeridaIa sugestao;
        try
        {
            sugestao = await _gemini.GerarAsync<MissaoSugeridaIa>(
                Prompts.SystemSugestaoMissao, Prompts.UserSugestaoMissao(textoLivre),
                Prompts.MissaoSugeridaSchema, Prompts.TemperaturaMissao, ct);
        }
        catch (IaIndisponivelException ex)
        {
            return Resultado.Falha<Missao>($"{ex.Message} Crie a missão manualmente enquanto isso.");
        }

        if (!Enum.TryParse<EsforcoMissao>(sugestao.Esforco, ignoreCase: true, out var esforco))
            esforco = EsforcoMissao.Media; // IA fora do enum combinado no schema — cai no meio-termo

        var faixa = _politica.FaixaDe(esforco);
        var xp = (faixa.Minimo + faixa.Maximo) / 2; // RN04/RN09 — XP sempre da tabela, nunca da IA

        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        var missao = new Missao
        {
            PlayerId = playerId,
            Tipo = TipoMissao.SugeridaIA,
            Titulo = sugestao.Titulo,
            Descricao = sugestao.Descricao,
            XpRecompensa = xp,
            GoldRecompensa = _recompensa.GoldPorXp(xp),
            Status = StatusMissao.Pendente,
            DataAlvo = _relogio.Hoje,
            OrigemJson = JsonSerializer.Serialize(new { pedido = textoLivre, sugestao }, JsonPadrao.Opcoes),
        };

        _db.Missoes.Add(missao);
        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(missao);
    }

    public async Task<Resultado<ResultadoXp>> ConcluirAsync(int missaoId, CancellationToken ct = default)
    {
        // Só o dono conclui a própria missão (isolamento entre usuários).
        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        await ExpirarVencidasAsync(playerId, ct); // RN10: missão de ontem não pode mais ser concluída

        var missao = await _db.Missoes.FirstOrDefaultAsync(m => m.Id == missaoId && m.PlayerId == playerId, ct);
        if (missao is null)
            return Resultado.Falha<ResultadoXp>("Missão não encontrada.");

        if (missao.Status is StatusMissao.Concluida)
            return Resultado.Falha<ResultadoXp>("Missão já concluída.");
        if (missao.Status is StatusMissao.Expirada)
            return Resultado.Falha<ResultadoXp>("Missão expirada.");

        missao.Status = StatusMissao.Concluida;
        missao.ConcluidaEm = _relogio.Agora;

        // GameService é a única fonte de verdade sobre XP/gold/streak (RN09).
        var recompensa = await _game.AdicionarXpAsync(missao.XpRecompensa, ct);

        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(recompensa);
    }

    public async Task<IReadOnlyList<Missao>> ListarDoDiaAsync(CancellationToken ct = default)
    {
        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        await ExpirarVencidasAsync(playerId, ct);
        await GerarMissoesAutomaticasSeNecessarioAsync(playerId, ct);

        return await _db.Missoes
            .Where(m => m.PlayerId == playerId && m.DataAlvo == _relogio.Hoje)
            .OrderBy(m => m.Status)
            .ToListAsync(ct);
    }

    /// <summary>
    /// RF11/RF16 — na primeira consulta do dia, gera até <see cref="MetaMissoesAutomaticas"/>
    /// missões: primeiro as revisões vencidas (exercícios reprovados, RN06), depois completa
    /// com os tópicos de menor nível entre os módulos já liberados (gaps + posição na árvore).
    /// Geração preguiçosa — mesmo padrão de <see cref="ExpirarVencidasAsync"/> — dispensa job à meia-noite.
    /// </summary>
    private async Task GerarMissoesAutomaticasSeNecessarioAsync(int playerId, CancellationToken ct)
    {
        var hoje = _relogio.Hoje;
        var jaGerouHoje = await _db.Missoes.AnyAsync(
            m => m.PlayerId == playerId && m.DataAlvo == hoje && m.Tipo == TipoMissao.Diaria, ct);
        if (jaGerouHoje) return;

        var geradas = new List<Missao>();

        var revisoesDevidas = await _db.Revisoes
            .Include(r => r.Tentativa).ThenInclude(t => t.Exercicio).ThenInclude(e => e.Topico)
            .Where(r => !r.Concluida && r.AgendadaPara <= hoje)
            .Take(MetaMissoesAutomaticas)
            .ToListAsync(ct);

        foreach (var revisao in revisoesDevidas)
        {
            var topico = revisao.Tentativa.Exercicio.Topico;
            geradas.Add(NovaMissaoAutomatica(playerId, $"Revisar: {topico.Nome}", topico.Id, EsforcoMissao.Media, hoje));
            revisao.Concluida = true; // RF16 — reaparece uma vez; não volta a gerar missão todo dia
        }

        if (geradas.Count < MetaMissoesAutomaticas)
        {
            var idsJaEscolhidos = geradas.Select(m => m.TopicoId!.Value).ToList();
            var topicosFracos = await _db.Topicos
                .Where(t => !t.Arquivado && t.Modulo.Status != StatusModulo.Bloqueado
                    && !idsJaEscolhidos.Contains(t.Id))
                .OrderBy(t => t.NivelEstimado)
                .Take(MetaMissoesAutomaticas - geradas.Count)
                .ToListAsync(ct);

            geradas.AddRange(topicosFracos.Select(t =>
                NovaMissaoAutomatica(playerId, $"Praticar {t.Nome}", t.Id, EsforcoMissao.Rapida, hoje)));
        }

        if (geradas.Count == 0) return;

        _db.Missoes.AddRange(geradas);
        await _db.SaveChangesAsync(ct);
    }

    private Missao NovaMissaoAutomatica(int playerId, string titulo, int topicoId, EsforcoMissao esforco, DateOnly hoje)
    {
        var faixa = _politica.FaixaDe(esforco);
        var xp = (faixa.Minimo + faixa.Maximo) / 2;
        return new Missao
        {
            PlayerId = playerId,
            Tipo = TipoMissao.Diaria,
            Titulo = titulo,
            TopicoId = topicoId,
            XpRecompensa = xp,
            GoldRecompensa = _recompensa.GoldPorXp(xp),
            Status = StatusMissao.Pendente,
            DataAlvo = hoje,
        };
    }

    /// <summary>
    /// RN10 — missões abertas de dias anteriores expiram (sem punição além da perda do streak
    /// do dia). Expiração preguiçosa: aplicada quando o jogador consulta/conclui missões, o que
    /// dispensa um job à meia-noite e dá o mesmo resultado observável.
    /// </summary>
    private async Task ExpirarVencidasAsync(int playerId, CancellationToken ct)
    {
        var hoje = _relogio.Hoje;
        var vencidas = await _db.Missoes
            .Where(m => m.PlayerId == playerId
                && (m.Status == StatusMissao.Pendente || m.Status == StatusMissao.EmAndamento)
                && m.DataAlvo < hoje)
            .ToListAsync(ct);
        if (vencidas.Count == 0) return;

        foreach (var missao in vencidas)
            missao.Status = StatusMissao.Expirada;
        await _db.SaveChangesAsync(ct);
    }
}
