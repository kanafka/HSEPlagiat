# Базовый рантайм
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8002

# SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем csproj и зависимые проекты
COPY FileAnalysisService.Api/FileAnalysisService.Api.csproj ./FileAnalysisService.Api/
COPY FileAnalysis.Application/FileAnalysis.Application.csproj ./FileAnalysis.Application/
COPY FileAnalysis.Domain/FileAnalysis.Domain.csproj ./FileAnalysis.Domain/
COPY FileAnalysis.Infrastructure/FileAnalysis.Infrastructure.csproj ./FileAnalysis.Infrastructure/

# Восстанавливаем зависимости
RUN dotnet restore FileAnalysisService.Api/FileAnalysisService.Api.csproj

# Копируем всё остальное
COPY . .

# Сборка и публикация
WORKDIR /src/FileAnalysisService.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FileAnalysisService.Api.dll"]
