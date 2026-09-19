using System.Net;
using System.Threading.RateLimiting;
using CodeQuest;
using CodeQuest.Data;
using Microsoft.AspNetCore.RateLimiting;

// Carrega o .env (na raiz do repo) para variáveis de ambiente antes de montar a configuração.
// Em produção/Docker as variáveis já vêm do ambiente, então a ausência do arquivo é ignorada.
// NoClobber() — se algo (ex.: WebApplicationFactory dos testes de integração) já setou a
// variável antes deste ponto, o .env NÃO deve sobrescrever (o padrão da lib é sobrescrever,
// o que fazia os testes conectarem no Postgres de desenvolvimento real).
DotNetEnv.Env.TraversePath().NoClobber().Load();

var builder = WebApplication.CreateBuilder(args);

const string CorsFront = "front";

// API REST consumida pela SPA React. Enums viajam como string (Status/Tipo/Esforço legíveis no JSON,
// imunes a reordenação dos membros) — casa com a mesma decisão do HasConversion<string>() no banco.
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// CORS liberado só para a origem do front (Vite). JWT vai no header, então não precisa de credenciais.
builder.Services.AddCors(o => o.AddPolicy(CorsFront, p => p
    .WithOrigins(builder.Configuration["Cors:OrigemFront"] ?? "http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Serviços do CodeQuest (DbContext, regras, serviços de aplicação, seed).
builder.Services.AddCodeQuest(builder.Configuration);

// Health check simples (só confirma que o app subiu e respondeu) — o compose usa isso pra
// só liberar o frontend depois que a API estiver de pé de verdade, não só "iniciada".
builder.Services.AddHealthChecks();

// Rate limit de login/registro por IP — só existe pra dificultar força bruta de senha;
// não protege nada mais no resto da API (o JWT/refresh token já cuida disso). Limite
// configurável (não fixo em 5) porque o WebApplicationFactory dos testes de integração não
// expõe IP real — todo request de teste cai no mesmo balde "desconhecido", e testes que
// registram/logam muitas vezes em sequência estourariam um limite de produção em segundos.
var limitePorMinuto = builder.Configuration.GetValue("RateLimiting:LoginPorMinuto", 5);
builder.Services.AddRateLimiter(o =>
{
    // Particiona por IP explicitamente: AddFixedWindowLimiter sozinho compartilharia um único
    // balde entre TODOS os clientes, travando o login de todo mundo depois de só 5 tentativas.
    o.AddPolicy("auth", contexto => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = limitePorMinuto,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
    o.OnRejected = async (contexto, ct) =>
    {
        contexto.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        contexto.HttpContext.Response.ContentType = "application/json";
        await contexto.HttpContext.Response.WriteAsJsonAsync(
            new { erro = "Muitas tentativas. Aguarde um minuto e tente de novo." }, ct);
    };
});

var app = builder.Build();

// Aplica migrations e semeia dados iniciais no boot.
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
    await seeder.ExecutarAsync();
}

// Configure the HTTP request pipeline.

// Rede de segurança: qualquer exceção não tratada por um controller vira um 500 no mesmo
// formato { erro } que o resto da API usa, em vez da página de erro padrão do ASP.NET
// (que em produção não devolve nada — só um 500 vazio, pior pra depurar do lado do front).
app.UseExceptionHandler(tratador => tratador.Run(async contexto =>
{
    contexto.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    contexto.Response.ContentType = "application/json";
    await contexto.Response.WriteAsJsonAsync(new { erro = "Erro inesperado no servidor." });
}));

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseCors(CorsFront);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints REST da API (consumidos pelo React). Logout revoga o refresh token no servidor
// (AuthController); o access token em si expira sozinho — a SPA só descarta os dois do storage.
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

/// <summary>Expõe o entry point para o WebApplicationFactory dos testes de integração.</summary>
public partial class Program;
