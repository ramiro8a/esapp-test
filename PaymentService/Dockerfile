FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/API/PaymentService.API.csproj", "src/API/"]
COPY ["src/Application/PaymentService.Application.csproj", "src/Application/"]
COPY ["src/Domain/PaymentService.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/PaymentService.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/API/PaymentService.API.csproj"
COPY . .
RUN dotnet build "src/API/PaymentService.API.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "src/API/PaymentService.API.csproj" -c Release -o /app/publish
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
