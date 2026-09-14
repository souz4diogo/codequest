using CodeQuest.Models;

namespace CodeQuest.IntegrationTests.Suporte;

/// <summary>Cria módulo/tópico próprios (fora do currículo semeado) direto no banco, para arranjar
/// pré-condições sem depender/interferir no estado global do currículo entre testes.</summary>
public static class ArvoreTestHelper
{
    public static async Task<(Modulo Modulo, Topico Topico)> CriarModuloComTopicoAsync(
        this CodeQuestApiFactory factory, int nivelTopico = 0, StatusModulo status = StatusModulo.Liberado)
    {
        await using var db = factory.CriarDbContext();
        var modulo = new Modulo { Nome = $"Módulo Teste {Guid.NewGuid():N}", Status = status };
        db.Modulos.Add(modulo);
        await db.SaveChangesAsync();

        var topico = new Topico { ModuloId = modulo.Id, Nome = "Tópico 1", NivelEstimado = nivelTopico };
        db.Topicos.Add(topico);
        await db.SaveChangesAsync();

        return (modulo, topico);
    }
}
