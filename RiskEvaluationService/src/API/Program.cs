using Microsoft.EntityFrameworkCore;
using RiskEvaluationService.Application.Services;
using RiskEvaluationService.Infrastructure.Kafka;
using RiskEvaluationService.Infrastructure.Data;
using RiskEvaluationService.Infrastructure.Repositories;
using RiskEvaluationService.Domain.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5001");

var kafkaServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
var pgConnection = builder.Configuration.GetConnectionString("PostgreSQL") ?? "Host=localhost;Port=5433;Database=payment_service;Username=postgres;Password=postgres";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// DbContext
builder.Services.AddDbContext<PaymentDbContext>(o => o.UseNpgsql(pgConnection));

// Repositorios
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Servicios
builder.Services.AddScoped<IRiskEvaluationService, RiskEvaluationAppService>();
builder.Services.AddSingleton<RiskResponseProducer>(_ => new RiskResponseProducer(kafkaServers));

// Consumer como Background Service
builder.Services.AddHostedService(sp => 
    new RiskRequestConsumer(
        kafkaServers,
        sp.GetRequiredService<RiskResponseProducer>(),
        sp
    )
);

builder.Services.AddCors(o => o.AddPolicy("All", p => 
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors("All");
app.MapControllers();

Console.WriteLine("\n✅ === RISK EVALUATION SERVICE READY ===");
Console.WriteLine("URL: http://localhost:5001");
Console.WriteLine("Escuchando: risk-evaluation-request\n");

app.Run();
