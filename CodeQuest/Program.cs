using CodeQuest;
using CodeQuest.Data;

// Carrega o .env (na raiz do repo) para variáveis de ambiente antes de montar a configuração.
// Em produção/Docker as variáveis já vêm do ambiente, então a ausência do arquivo é ignorada.
DotNetEnv.Env.TraversePath().Load();

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseCors(CorsFront);
app.UseAuthentication();
app.UseAuthorization();

// Endpoints REST da API (consumidos pelo React). O logout é client-side: a SPA descarta o JWT.
app.MapControllers();

app.Run();
