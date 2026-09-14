using Xunit;

namespace CodeQuest.IntegrationTests.Suporte;

/// <summary>
/// Uma única collection para todas as classes de teste de integração: sobem UM container de
/// Postgres compartilhado (via CodeQuestApiFactory) em vez de um por classe — o custo de subir
/// o container só é pago uma vez por execução.
/// </summary>
[CollectionDefinition(Nome)]
public sealed class CodeQuestCollection : ICollectionFixture<CodeQuestApiFactory>
{
    public const string Nome = "CodeQuest API";
}
