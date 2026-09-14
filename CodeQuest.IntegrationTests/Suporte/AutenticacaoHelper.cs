using System.Net.Http.Headers;
using System.Net.Http.Json;
using CodeQuest.Dtos;

namespace CodeQuest.IntegrationTests.Suporte;

/// <summary>Registra/loga um usuário via API real e devolve um HttpClient já autenticado (Bearer).</summary>
public static class AutenticacaoHelper
{
    public static async Task<(HttpClient Cliente, TokenResponse Token)> RegistrarELogarAsync(
        this CodeQuestApiFactory factory, string? login = null, string senha = "senha123")
    {
        login ??= $"user_{Guid.NewGuid():N}";

        var cliente = factory.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/auth/registrar", new RegistrarRequest(login, senha));
        resposta.EnsureSuccessStatusCode();

        var token = await resposta.Content.ReadFromJsonAsync<TokenResponse>()
            ?? throw new InvalidOperationException("Registro não devolveu token.");

        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return (cliente, token);
    }
}
