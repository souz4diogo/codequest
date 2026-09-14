using System.Text.Json;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services.Ai;
using CodeQuest.Services.Regras;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>Desfecho de um boss fight corrigido: nota, aprovação (RN08) e módulos que essa
/// conclusão acabou de liberar (RF06).</summary>
public sealed record ResultadoBoss(Teste Teste, CorrecaoTeste Correcao, bool Aprovado, IReadOnlyList<string> ModulosLiberados);

/// <summary>
/// Boss fight (RF12/RN08): uma questão por tópico ativo do módulo, gerada pela IA em nível
/// Expert. Aprovar (nota ≥ 70) conclui o módulo e libera os que dependiam dele.
/// </summary>
public interface IBossService
{
    Task<Resultado<Teste>> IniciarAsync(int moduloId, CancellationToken ct = default);

    Task<Resultado<ResultadoBoss>> ResponderAsync(int testeId, IReadOnlyList<int> respostas,
        CancellationToken ct = default);
}

public sealed class BossService : IBossService
{

    private readonly AppDbContext _db;
    private readonly IGeminiClient _gemini;
    private readonly IPoliticaArvore _politica;
    private readonly IRelogio _relogio;

    public BossService(AppDbContext db, IGeminiClient gemini, IPoliticaArvore politica, IRelogio relogio)
    {
        _db = db;
        _gemini = gemini;
        _politica = politica;
        _relogio = relogio;
    }

    public async Task<Resultado<Teste>> IniciarAsync(int moduloId, CancellationToken ct = default)
    {
        var modulo = await _db.Modulos.Include(m => m.Topicos)
            .FirstOrDefaultAsync(m => m.Id == moduloId, ct);
        if (modulo is null)
            return Resultado.Falha<Teste>("Módulo não encontrado.");
        if (modulo.Status == StatusModulo.Bloqueado)
            return Resultado.Falha<Teste>("Módulo ainda bloqueado.");
        if (modulo.Status == StatusModulo.Concluido)
            return Resultado.Falha<Teste>("Módulo já concluído.");

        var topicosAtivos = modulo.Topicos.Where(t => !t.Arquivado).ToList();
        if (!_politica.BossLiberado(topicosAtivos.Select(t => t.NivelEstimado).ToList()))
            return Resultado.Falha<Teste>(
                $"Boss ainda não liberado: todos os tópicos precisam de nível ≥ {PoliticaArvore.NivelMinimoTopico}.");

        TesteIa gerado;
        try
        {
            gerado = await _gemini.GerarAsync<TesteIa>(
                Prompts.SystemGeracaoTeste,
                Prompts.UserGeracaoBoss(modulo.Nome, topicosAtivos.Select(t => t.Nome).ToList()),
                Prompts.TesteSchema, Prompts.TemperaturaGeracao, ct);
        }
        catch (IaIndisponivelException ex)
        {
            return Resultado.Falha<Teste>($"{ex.Message} Enquanto isso, missões manuais seguem disponíveis.");
        }

        var teste = new Teste
        {
            ModuloId = modulo.Id,
            Dificuldade = Dificuldade.Expert,
            QuestoesJson = JsonSerializer.Serialize(gerado, JsonPadrao.Opcoes),
            Data = _relogio.Agora,
        };
        _db.Testes.Add(teste);
        await _db.SaveChangesAsync(ct);

        return Resultado.Ok(teste);
    }

    public async Task<Resultado<ResultadoBoss>> ResponderAsync(int testeId, IReadOnlyList<int> respostas,
        CancellationToken ct = default)
    {
        var teste = await _db.Testes.Include(t => t.Modulo)
            .FirstOrDefaultAsync(t => t.Id == testeId && t.ModuloId != null, ct);
        if (teste is null)
            return Resultado.Falha<ResultadoBoss>("Boss fight não encontrado.");
        if (teste.Nota is not null)
            return Resultado.Falha<ResultadoBoss>("Este boss fight já foi corrigido.");

        var gabarito = JsonSerializer.Deserialize<TesteIa>(teste.QuestoesJson, JsonPadrao.Opcoes)!;
        var correcao = CorretorTeste.Corrigir(gabarito, respostas);

        teste.RespostasJson = JsonSerializer.Serialize(respostas, JsonPadrao.Opcoes);
        teste.Nota = correcao.Nota;
        teste.GapsJson = JsonSerializer.Serialize(correcao.Gaps, JsonPadrao.Opcoes);

        var modulo = teste.Modulo!;
        modulo.NotaBoss = correcao.Nota;
        var aprovado = _politica.BossAprovado(correcao.Nota);

        var liberados = new List<string>();
        if (aprovado)
        {
            modulo.Status = StatusModulo.Concluido;

            // RF06 — libera módulos cujos pré-requisitos estão TODOS concluídos agora.
            var candidatos = await _db.Modulos
                .Include(m => m.PreRequisitos)
                .Where(m => m.Status == StatusModulo.Bloqueado)
                .ToListAsync(ct);
            foreach (var candidato in candidatos)
            {
                var idsRequeridos = candidato.PreRequisitos.Select(p => p.RequerModuloId).ToList();
                if (idsRequeridos.Count == 0) continue;

                if (await TodosConcluidosAsync(idsRequeridos, modulo.Id, ct))
                {
                    candidato.Status = StatusModulo.Liberado;
                    liberados.Add(candidato.Nome);
                }
            }
        }

        await _db.SaveChangesAsync(ct);

        return Resultado.Ok(new ResultadoBoss(teste, correcao, aprovado, liberados));
    }

    private async Task<bool> TodosConcluidosAsync(IReadOnlyList<int> moduloIds, int moduloRecemConcluidoId, CancellationToken ct)
    {
        var concluidos = await _db.Modulos
            .Where(m => moduloIds.Contains(m.Id) && (m.Status == StatusModulo.Concluido || m.Id == moduloRecemConcluidoId))
            .CountAsync(ct);
        return concluidos == moduloIds.Count;
    }
}
