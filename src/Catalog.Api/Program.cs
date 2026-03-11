using Serilog;
using OpenTelemetry.Trace;
using Catalog.Api.Configurations;
using Catalog.Api.Middlewares;
using Catalog.Application.Interfaces;
using Catalog.Application.Services;
using Catalog.Domain.Dto;
using Catalog.Domain.Interfaces;
using Catalog.Infra.Data;
using Catalog.Infra.MessageBus;
using Catalog.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Users.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Verbose()
	.Enrich.FromLogContext()
	.Enrich.With(new Catalog.Api.Serilog.ActivityEnricher())
	.WriteTo.Console()
	.CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: true);

builder.Services.AddAuthConfiguration(builder.Configuration);
// RabbitMQ
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// EF Core
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SetupConnection")));

// Services
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Repositories
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserGameRepository, UserGameRepository>();

// Message Bus
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<PaymentProcessedConsumer>();

// Http context accessor required by middlewares
builder.Services.AddHttpContextAccessor();

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// ✅ APLICAR MIGRATIONS (FORMA CORRETA)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseErrorHandling();
app.UseCorrelationId();
app.UseRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
