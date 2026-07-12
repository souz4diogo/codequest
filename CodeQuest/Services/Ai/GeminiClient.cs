using System.Text;
using System.Text.Json;

namespace CodeQuest.Services.Ai;

/// <summary>
/// Lançada quando a IA está inacessível, mal configurada ou a resposta não parseia nem após o
/// retry (RNF04/RNF05). Os serviços convertem em <c>Resultado</c> de falha com mensagem amigável —
/// o app segue funcionando sem IA (modo degradado, RNF07).
/// </summary>
public sealed class IaIndisponivelException : Exception
{
    public IaIndisponivelException(string mensagem, Exception? causa = null) : base(mensagem, causa) { }
}

/// <summary>
/// Cliente do Gemini (REST <c>generateContent</c>) com saída estruturada via
/// <c>responseSchema</c> — nunca parseamos texto livre (RNF05).
/// </summary>
public interface IGeminiClient
{
    /// <summary>
    /// Chama o modelo e desserializa a resposta JSON em <typeparamref name="T"/>.
    /// Timeout de 30s e 1 retry (RNF04); esgotadas as tentativas, lança <see cref="IaIndisponivelException"/>.
    /// </summary>
    Task<T> GerarAsync<T>(string systemPrompt, string userPrompt, object responseSchema,
        double temperature, CancellationToken ct = default);
}

/// <summary>
/// Implementação HTTP (seção 3 do motor-ia). A chave vem de <c>Gemini:ApiKey</c> — user secrets
/// em dev, variável de ambiente em produção (RNF02) — e viaja no header <c>x-goog-api-key</c>,
/// não na URL, para não vazar em logs. Lida por chamada (e não no construtor) para o app
/// subir sem IA configurada (RNF07).
/// </summary>
public sealed class GeminiClient : IGeminiClient
{
    // O doc citava gemini-2.0-flash, mas ele saiu do free tier (429, limit 0) — verificado em 12/07/2026.
    // Sobrescreva via config "Gemini:Model" quando o modelo atual mudar de novo.
    private const string ModeloPadrao = "gemini-2.5-flash";
    private const int MaxTentativas = 2; // 1 chamada + 1 retry (RNF04)

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(HttpClient http, IConfiguration config, ILogger<GeminiClient> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<T> GerarAsync<T>(string systemPrompt, string userPrompt, object responseSchema,
        double temperature, CancellationToken ct = default)
    {
        var apiKey = _config["Gemini:ApiKey"]
            ?? throw new IaIndisponivelException(
                "IA não configurada: defina Gemini:ApiKey (dotnet user-secrets set \"Gemini:ApiKey\" \"...\").");
        var modelo = _config["Gemini:Model"] ?? ModeloPadrao;

        var body = new
        {
            system_instruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = new[] { new { role = "user", parts = new[] { new { text = userPrompt } } } },
            generationConfig = new
            {
                response_mime_type = "application/json",
                response_schema = responseSchema,
                temperature,
                // Desliga o "thinking" do 2.5-flash: sem isso, gerações grandes (expert)
                // estouram o timeout de 30s do RNF04. Latência volta ao patamar do 2.0-flash.
                thinking_config = new { thinking_budget = 0 },
            },
        };
        var json = JsonSerializer.Serialize(body);
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelo}:generateContent";

        Exception? ultimaFalha = null;
        for (var tentativa = 1; tentativa <= MaxTentativas; tentativa++)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };
                req.Headers.Add("x-goog-api-key", apiKey);

                var resp = await _http.SendAsync(req, ct);
                resp.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
                var texto = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text").GetString()
                    ?? throw new JsonException("Resposta do modelo veio sem texto.");

                return JsonSerializer.Deserialize<T>(texto, JsonOpts)
                    ?? throw new JsonException("JSON do modelo desserializou como null.");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw; // cancelamento do chamador não é falha da IA — propaga
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                or JsonException or KeyNotFoundException or IndexOutOfRangeException or InvalidOperationException)
            {
                // RNF05: falha de transporte/parse é registrada e regenerada (1 retry) — nunca quebra a tela.
                ultimaFalha = ex;
                _logger.LogWarning(ex, "Chamada ao Gemini falhou (tentativa {Tentativa}/{Max}).",
                    tentativa, MaxTentativas);
            }
        }

        throw new IaIndisponivelException("Mentor IA indisponível no momento.", ultimaFalha);
    }
}
