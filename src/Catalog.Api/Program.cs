using Azure.Messaging.ServiceBus;
using Catalog.Api.Configurations;
using Catalog.Api.Middlewares;
using Catalog.Application.Interfaces;
using Catalog.Application.Services;
using Catalog.Domain.Interfaces;
using Catalog.Infra.Data;
using Catalog.Infra.MessageBus;
using Catalog.Infra.Repositories;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using Serilog;
using Users.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

builder.Host.UseSerilog((_, services, loggerConfiguration) => loggerConfiguration
	.MinimumLevel.Verbose()
	.Enrich.FromLogContext()
	.Enrich.With(new Catalog.Api.Serilog.ActivityEnricher())
	.WriteTo.Console()
	.WriteTo.ApplicationInsights(
		services.GetRequiredService<TelemetryConfiguration>(),
		TelemetryConverter.Traces));

builder.Services.AddAuthConfiguration(builder.Configuration);

// ServiceBus
builder.Services.AddSingleton<IServiceBus, ServiceBus>();
builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
	var connectionString = builder.Configuration["ServiceBus:ConnectionString"];
	return new ServiceBusClient(connectionString);
});
builder.Services.AddHostedService<ServiceBusConsumer>();

// EF Core
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

// Services
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Repositories
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserGameRepository, UserGameRepository>();

// Http context accessor required by middlewares
builder.Services.AddHttpContextAccessor();

// API
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    db.Database.Migrate();
//}

// DatabaseInitializer.EnsureDatabase(builder.Configuration);

using (var scope = app.Services.CreateScope())
{
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

	try
	{
		var connectionString = db.Database.GetConnectionString();
		logger.LogInformation("Tentando conectar ao banco de dados...");
		logger.LogInformation("Connection string: {ConnectionString}", connectionString);

		// db.Database.Migrate();

		logger.LogInformation("Migrate executado com sucesso.");
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Erro ao conectar ou migrar o banco de dados.");
		throw;
	}
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
