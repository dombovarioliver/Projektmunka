FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FitAppBackendRefactor/FitAppBackend.Api.csproj", "FitAppBackendRefactor/"]
COPY ["DiplomaFit.Data/DiplomaFit.Data.csproj", "DiplomaFit.Data/"]
COPY ["DiplomaFit.Model/DiplomaFit.Model.csproj", "DiplomaFit.Model/"]
COPY ["DiplomaFit.Service/DiplomaFit.Service.csproj", "DiplomaFit.Service/"]

RUN dotnet restore "FitAppBackendRefactor/FitAppBackend.Api.csproj"

COPY . .

RUN dotnet publish "FitAppBackendRefactor/FitAppBackend.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FitAppBackend.Api.dll"]