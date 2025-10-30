using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.DTOs;
using PaymentService.Application.Services;
using PaymentService.Domain.Repositories;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Infrastructure.Services;
using PaymentService.Infrastructure.Kafka;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var kafkaServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
var pgConnection = builder.Configuration.GetConnectionString("PostgreSQL") ?? "Host=localhost;Port=5433;Database=payment_service;Username=postgres;Password=postgres";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Payment Service API",
        Version = "v1",
        Description = "API para gestión de pagos con evaluación automática de riesgos. Los pagos se evalúan en tiempo real para detectar fraudes y transacciones sospechosas.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Repositorio GitHub",
            Url = new Uri("https://github.com/ramiro8a/esapp-test")
        }
    });
});

builder.Services.AddDbContext<PaymentDbContext>(o => o.UseNpgsql(pgConnection));
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentAppService>();
builder.Services.AddScoped(_ => new PaymentEventPublisher(kafkaServers));

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    
    x.AddConsumer<RiskEvaluationResponseConsumer>();
    
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
    
    x.AddRider(rider =>
    {
        rider.UsingKafka((context, cfg) =>
        {
            cfg.Host(kafkaServers);
        });
    });
});

builder.Services.AddCors(o => o.AddPolicy("All", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("All");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    try
    {
        Console.WriteLine("🔄 Verificando conexión a la BD...");
        var canConnect = db.Database.CanConnect();
        Console.WriteLine($"✅ Conexión a BD: {canConnect}");
        
        Console.WriteLine("🔄 Aplicando migraciones...");
        db.Database.Migrate();
        Console.WriteLine("✅ Migraciones aplicadas exitosamente");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error en migraciones: {ex.Message}");
        Console.WriteLine($"Stack: {ex.StackTrace}");
        throw;
    }
}

Console.WriteLine("\n✅ === PAYMENT SERVICE READY ===");
Console.WriteLine("URL: http://localhost:5000");
Console.WriteLine("Swagger: http://localhost:5000/swagger");
Console.WriteLine("Health: http://localhost:5000/health\n");

app.Run();
