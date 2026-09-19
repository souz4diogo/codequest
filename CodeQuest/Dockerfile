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

# curl só pro HEALTHCHECK (a imagem base não vem com nenhum cliente HTTP instalado).
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

# Escuta HTTP na 8080 (sem HTTPS dentro do container; TLS fica a cargo do proxy/host).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=5s --timeout=3s --start-period=15s --retries=10 \
    CMD curl -f http://localhost:8080/health || exit 1

# Roda como usuário não-root (imagem já traz o usuário "app").
USER app

ENTRYPOINT ["dotnet", "CodeQuest.dll"]
