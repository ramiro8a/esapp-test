FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/API/RiskEvaluationService.API.csproj", "src/API/"]
COPY ["src/Application/RiskEvaluationService.Application.csproj", "src/Application/"]
COPY ["src/Domain/RiskEvaluationService.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/RiskEvaluationService.Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/API/RiskEvaluationService.API.csproj"

COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "RiskEvaluationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RiskEvaluationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "RiskEvaluationService.API.dll"]
