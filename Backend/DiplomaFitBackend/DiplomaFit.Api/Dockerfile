# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DiplomaFitBackend.sln", "./"]
COPY ["DiplomaFit.Api/DiplomaFit.Api.csproj", "DiplomaFit.Api/"]
COPY ["DiplomaFit.Data/DiplomaFit.Data.csproj", "DiplomaFit.Data/"]
COPY ["DiplomaFit.Model/DiplomaFit.Model.csproj", "DiplomaFit.Model/"]
COPY ["DiplomaFit.Service/DiplomaFit.Service.csproj", "DiplomaFit.Service/"]

RUN dotnet restore "DiplomaFit.Api/DiplomaFit.Api.csproj"

COPY . .
RUN dotnet publish "DiplomaFit.Api/DiplomaFit.Api.csproj" -c Release -o /app/publish

# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DiplomaFit.Api.dll"]
