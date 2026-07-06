using CodeQuest;
using CodeQuest.Components;
using CodeQuest.Data;
using Microsoft.AspNetCore.Authentication;

// Carrega o .env (na raiz do repo) para variáveis de ambiente antes de montar a configuração.
// Em produção/Docker as variáveis já vêm do ambiente, então a ausência do arquivo é ignorada.
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Serviços do CodeQuest (DbContext, regras, serviços de aplicação, seed).
builder.Services.AddCodeQuest(builder.Configuration);

var app = builder.Build();

// Aplica migrations e semeia dados iniciais no boot.
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
    await seeder.ExecutarAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Logout: encerra a sessão e volta para o login. POST para evitar logout por link/GET.
app.MapPost("/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).DisableAntiforgery();

app.Run();
