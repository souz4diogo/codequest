using CodeQuest.Services.Ai;

namespace CodeQuest.Tests.Suporte;

/// <summary>Fake de IGeminiClient: devolve um valor pré-configurado, sem chamar a IA de verdade.</summary>
public sealed class GeminiClientFake : IGeminiClient
{
    private readonly object _resposta;
    private readonly Exception? _excecao;

    private GeminiClientFake(object resposta, Exception? excecao)
    {
        _resposta = resposta;
        _excecao = excecao;
    }

    public static GeminiClientFake RetornandoOk(object resposta) => new(resposta, null);

    public static GeminiClientFake Lancando(Exception excecao) => new(new object(), excecao);

    public Task<T> GerarAsync<T>(string systemPrompt, string userPrompt, object responseSchema,
        double temperature, CancellationToken ct = default)
    {
        if (_excecao is not null) throw _excecao;
        return Task.FromResult((T)_resposta);
    }
}
