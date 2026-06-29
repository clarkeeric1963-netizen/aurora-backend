# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AuroraTms.Api/*.csproj ./AuroraTms.Api/
RUN dotnet restore ./AuroraTms.Api/AuroraTms.Api.csproj

# Cache-bust: bump this value any time Railway serves a stale build.
# Changing it forces a clean re-copy of source + a fresh publish.
ARG CACHEBUST=2026-06-28-2330
COPY AuroraTms.Api/ ./AuroraTms.Api/
RUN dotnet publish ./AuroraTms.Api/AuroraTms.Api.csproj -c Release -o /app

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Railway provides PORT at runtime; Program.cs binds to it.
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuroraTms.Api.dll"]
