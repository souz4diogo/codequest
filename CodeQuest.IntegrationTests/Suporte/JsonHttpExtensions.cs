using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeQuest.IntegrationTests.Suporte;

/// <summary>
/// A API serializa enums como string (JsonStringEnumConverter registrado só nas JsonOptions do
/// MVC, em Program.cs) — os helpers de JSON do HttpClient não enxergam essa configuração, então
/// os testes precisam das mesmas opções para ler/escrever DTOs com enum (Dificuldade, StatusModulo...).
/// </summary>
public static class JsonHttpExtensions
{
    public static readonly JsonSerializerOptions Opcoes = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static Task<T?> LerJsonAsync<T>(this HttpResponseMessage resposta, CancellationToken ct = default) =>
        resposta.Content.ReadFromJsonAsync<T>(Opcoes, ct);

    public static Task<T?> ObterJsonAsync<T>(this HttpClient cliente, string url, CancellationToken ct = default) =>
        cliente.GetFromJsonAsync<T>(url, Opcoes, ct);

    public static Task<HttpResponseMessage> PostarJsonAsync<T>(this HttpClient cliente, string url, T corpo, CancellationToken ct = default) =>
        cliente.PostAsJsonAsync(url, corpo, Opcoes, ct);

    public static Task<HttpResponseMessage> PutAsJsonComEnumAsync<T>(this HttpClient cliente, string url, T corpo, CancellationToken ct = default) =>
        cliente.PutAsJsonAsync(url, corpo, Opcoes, ct);
}
