using CodeQuest.Models;
using CodeQuest.Services.Loja;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Data;

/// <summary>Popula o banco com os dados globais iniciais (módulos, tópicos, loja). Item 0.3 do backlog.
/// O Player não é semeado: cada um nasce no registro do seu usuário.</summary>
public interface ISeeder
{
    Task ExecutarAsync(CancellationToken ct = default);
}

/// <summary>
/// Seed idempotente em modo SINCRONIZAÇÃO: alinha o banco ao plano a cada boot — insere o que
/// falta e corrige nomes/preços, mas nunca apaga dados do jogador. Tópicos fora do currículo
/// são arquivados e itens fora do catálogo são desativados (histórico e FKs preservados).
/// Aplica as migrations pendentes antes de semear.
/// </summary>
public sealed class DatabaseSeeder : ISeeder
{
    private readonly AppDbContext _db;

    public DatabaseSeeder(AppDbContext db) => _db = db;

    /// <summary>Currículo junior fullstack — seção 3 do plano-codequest.md (10 módulos + tópicos).</summary>
    private static readonly (int Ordem, string Nome, string[] Topicos)[] Curriculo =
    [
        (1, "Fundamentos C#",
            ["Sintaxe e tipos", "Controle de fluxo", "Métodos", "Coleções", "Strings", "Tratamento de exceções"]),
        (2, "POO",
            ["Classes e objetos", "Herança", "Interfaces", "Polimorfismo", "Encapsulamento", "Composição vs herança"]),
        (3, "C# intermediário",
            ["LINQ", "Generics", "Delegates e eventos", "Async/await", "System.Text.Json"]),
        (4, "Banco de dados",
            ["Modelagem", "SQL (SELECT, JOIN, GROUP BY)", "Índices", "Transações", "PostgreSQL (psql, tipos, sequences)"]),
        (5, "EF Core",
            ["Migrations", "Relacionamentos", "Queries", "Tracking", "Performance básica"]),
        (6, "Back-end / APIs",
            ["ASP.NET Core", "REST e verbos HTTP", "Status codes", "Validação", "Autenticação (JWT)", "Middleware"]),
        (7, "Front-end web",
            ["HTML", "CSS (flex e grid)", "JavaScript essencial", "React (componentes, hooks, estado)", "Fetch e consumo de API"]),
        (8, "Ferramentas do ofício",
            ["Git e GitHub (branch, PR, merge)", "Debugging", "Terminal", "NuGet"]),
        (9, "Qualidade",
            ["Testes unitários (xUnit)", "SOLID básico", "Code review", "Clean code"]),
        (10, "Deploy & mundo real",
            ["Publicação (Fly.io/Railway)", "Variáveis de ambiente", "Logs", "CI básico (GitHub Actions)"]),
    ];

    /// <summary>
    /// Pré-requisitos (RF06). Do plano (seção 3): 1→2→3 em sequência, 4 em paralelo a partir
    /// do 2, 7 em paralelo a partir do 3. Os demais pares seguem a progressão natural do currículo.
    /// </summary>
    private static readonly (int Modulo, int Requer)[] Prereqs =
    [
        (2, 1), (3, 2), (4, 2), (5, 3), (5, 4), (6, 5), (7, 3), (8, 1), (9, 2), (10, 6), (10, 9),
    ];

    /// <summary>Catálogo da loja — seção 2 do plano ("Gold e loja"). Descanso comprado é sem culpa.</summary>
    private static readonly (string Nome, int CustoGold)[] Catalogo =
    [
        ("30 min de jogo", 60),
        ("1 hora de jogo", 100),
        ("Noite livre", 250),
        (ItensLojaConhecidos.PocaoDeStreak, 400),
        ("Sábado livre inteiro", 600),
    ];

    public async Task ExecutarAsync(CancellationToken ct = default)
    {
        await _db.Database.MigrateAsync(ct);

        await SincronizarArvoreAsync(ct);
        await SincronizarLojaAsync(ct);

        await _db.SaveChangesAsync(ct);
    }

    private async Task SincronizarArvoreAsync(CancellationToken ct)
    {
        var modulos = await _db.Modulos.Include(m => m.Topicos).ToListAsync(ct);

        // Módulos: casa por Ordem (identidade estável) e alinha o nome ao plano.
        // O Status é progresso do jogo e nunca é sobrescrito; módulo novo nasce Bloqueado, exceto o 1º.
        foreach (var (ordem, nome, _) in Curriculo)
        {
            var modulo = modulos.FirstOrDefault(m => m.Ordem == ordem);
            if (modulo is null)
            {
                modulo = new Modulo
                {
                    Ordem = ordem,
                    Nome = nome,
                    Status = ordem == 1 ? StatusModulo.Liberado : StatusModulo.Bloqueado,
                };
                _db.Modulos.Add(modulo);
                modulos.Add(modulo);
            }
            else
            {
                modulo.Nome = nome;
            }
        }
        await _db.SaveChangesAsync(ct); // garante os Ids para pré-requisitos e tópicos

        Modulo M(int ordem) => modulos.First(m => m.Ordem == ordem);

        // Pré-requisitos: sincroniza o conjunto — adiciona os que faltam, remove os que saíram do plano.
        var atuais = await _db.ModuloPrereqs.ToListAsync(ct);
        var desejados = Prereqs
            .Select(p => (ModuloId: M(p.Modulo).Id, RequerId: M(p.Requer).Id))
            .ToHashSet();
        _db.ModuloPrereqs.RemoveRange(
            atuais.Where(a => !desejados.Contains((a.ModuloId, a.RequerModuloId))));
        var existentes = atuais.Select(a => (a.ModuloId, a.RequerModuloId)).ToHashSet();
        _db.ModuloPrereqs.AddRange(desejados
            .Where(d => !existentes.Contains(d))
            .Select(d => new ModuloPrereq { ModuloId = d.ModuloId, RequerModuloId = d.RequerId }));

        // Tópicos: insere os do currículo que faltam; os fora do plano são ARQUIVADOS, não
        // removidos — podem ter exercícios/testes/dúvidas vinculados e o histórico fica intacto.
        foreach (var (ordem, _, topicos) in Curriculo)
        {
            var modulo = M(ordem);

            foreach (var nome in topicos)
            {
                var topico = modulo.Topicos.FirstOrDefault(
                    t => string.Equals(t.Nome, nome, StringComparison.OrdinalIgnoreCase));
                if (topico is null)
                    _db.Topicos.Add(new Topico { ModuloId = modulo.Id, Nome = nome });
                else
                    topico.Arquivado = false; // voltou ao plano: reativa
            }

            foreach (var orfao in modulo.Topicos.Where(
                t => !topicos.Contains(t.Nome, StringComparer.OrdinalIgnoreCase)))
            {
                orfao.Arquivado = true;
            }
        }
    }

    private async Task SincronizarLojaAsync(CancellationToken ct)
    {
        var itens = await _db.ItensLoja.ToListAsync(ct);

        // Upsert por nome; o preço vem sempre do catálogo do plano.
        foreach (var (nome, custo) in Catalogo)
        {
            var item = itens.FirstOrDefault(
                i => string.Equals(i.Nome, nome, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                _db.ItensLoja.Add(new ItemLoja { Nome = nome, CustoGold = custo });
            }
            else
            {
                item.CustoGold = custo;
                item.Ativo = true;
            }
        }

        // Itens fora do catálogo saem de linha (Ativo = false) — compras antigas continuam íntegras
        // porque CompraLoja guarda o preço pago na época e a FK permanece válida.
        foreach (var fora in itens.Where(
            i => !Catalogo.Any(c => string.Equals(c.Nome, i.Nome, StringComparison.OrdinalIgnoreCase))))
        {
            fora.Ativo = false;
        }
    }
}
