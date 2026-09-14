using System.Collections.Concurrent;
using CodeQuest.Services.Ai;

namespace CodeQuest.IntegrationTests.Suporte;

/// <summary>
/// Substitui o IGeminiClient real na DI dos testes de integração: nunca chama a IA de verdade.
/// Cada teste registra a resposta que quer receber para o tipo T pedido (fila por tipo, FIFO).
/// </summary>
public sealed class GeminiClientFake : IGeminiClient
{
    private readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _respostas = new();

    public void Enfileirar<T>(T resposta) where T : notnull =>
        _respostas.GetOrAdd(typeof(T), _ => new ConcurrentQueue<object>()).Enqueue(resposta);

    public Task<T> GerarAsync<T>(string systemPrompt, string userPrompt, object responseSchema,
        double temperature, CancellationToken ct = default)
    {
        if (_respostas.TryGetValue(typeof(T), out var fila) && fila.TryDequeue(out var resposta))
            return Task.FromResult((T)resposta);

        throw new InvalidOperationException(
            $"Nenhuma resposta fake enfileirada para {typeof(T).Name}. Chame Enfileirar<{typeof(T).Name}>(...) antes do request.");
    }
}
