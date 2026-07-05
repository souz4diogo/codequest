# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaura primeiro (camada cacheável enquanto o csproj não muda).
COPY CodeQuest.csproj .
RUN dotnet restore

# Compila e publica.
COPY . .
RUN dotnet publish CodeQuest.csproj -c Release -o /app --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# Escuta HTTP na 8080 (sem HTTPS dentro do container; TLS fica a cargo do proxy/host).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Roda como usuário não-root (imagem já traz o usuário "app").
USER app

ENTRYPOINT ["dotnet", "CodeQuest.dll"]
