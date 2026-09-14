using System.Text.Json;

namespace CodeQuest.Common;

/// <summary>
/// Opções de serialização padrão (camelCase) para o JSON persistido no banco (EnunciadoJson,
/// QuestoesJson, RespostasJson, GapsJson, etc.) e trocado com a IA — evita reinstanciar
/// <see cref="JsonSerializerOptions"/> em cada service/DTO que lida com esses campos.
/// </summary>
public static class JsonPadrao
{
    public static readonly JsonSerializerOptions Opcoes = new(JsonSerializerDefaults.Web);
}
