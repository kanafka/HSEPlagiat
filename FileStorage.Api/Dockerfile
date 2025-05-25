FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем .csproj файлы по папкам
COPY FileStorage.Api/FileStorage.Api.csproj ./FileStorage.Api/
COPY FileStorage.Application/FileStorage.Application.csproj ./FileStorage.Application/
COPY FileStorage.Domain/FileStorage.Domain.csproj ./FileStorage.Domain/
COPY FileStorage.Infrastructure/FileStorage.Infrastructure.csproj ./FileStorage.Infrastructure/

# Выполняем restore на главном проекте
RUN dotnet restore FileStorage.Api/FileStorage.Api.csproj

# Копируем всё остальное
COPY . .

WORKDIR /src/FileStorage.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FileStorage.Api.dll"]
