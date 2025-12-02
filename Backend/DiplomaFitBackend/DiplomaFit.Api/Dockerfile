# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Projekt fájl bemásolása és restore
COPY ["DiplomaFit.Api.csproj", "./"]
RUN dotnet restore

# Összes forrás bemásolása és publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DiplomaFit.Api.dll"]
