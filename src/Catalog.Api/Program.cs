using Azure.Messaging.ServiceBus;
using Catalog.Api.Configurations;
using Catalog.Api.Middlewares;
using Catalog.Application.Interfaces;
using Catalog.Application.Services;
using Catalog.Domain.Interfaces;
using Catalog.Infra.Data;
using Catalog.Infra.MessageBus;
using Catalog.Infra.Repositories;
using Elastic.Clients.Elasticsearch;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Serilog;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

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

builder.Services.AddSingleton<IServiceBus, ServiceBus>();
builder.Services.AddSingleton<ServiceBusClient>(_ =>
{
	var connectionString = builder.Configuration["ServiceBus:ConnectionString"]
		?? throw new InvalidOperationException("ServiceBus:ConnectionString não configurado.");
	return new ServiceBusClient(connectionString);
});
builder.Services.AddHostedService<ServiceBusConsumer>();

builder.Services.AddDbContext<AppDbContext>(opt =>
	opt.UseSqlServer(
		builder.Configuration.GetConnectionString("SqlConnection")
		?? throw new InvalidOperationException("ConnectionStrings:SqlConnection não configurada.")));

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IGameReviewService, GameReviewService>();
builder.Services.AddScoped<IGameSearchService, GameSearchService>();

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserGameRepository, UserGameRepository>();
builder.Services.AddScoped<IGameSearchRepository, ElasticsearchGameRepository>();
builder.Services.AddScoped<IGameReviewRepository, MongoGameReviewRepository>();

builder.Services.AddSingleton<IMongoClient>(sp =>
{
	var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("MongoConfiguration");
	var connectionString = ResolveMongoConnectionString(builder.Configuration, logger)
		?? throw new InvalidOperationException("MongoDb:ConnectionString não configurada.");
	return new MongoClient(connectionString);
});

builder.Services.AddSingleton(sp =>
{
	var databaseName = builder.Configuration["MongoDb:Database"]
		?? throw new InvalidOperationException("MongoDb:Database não configurado.");
	var client = sp.GetRequiredService<IMongoClient>();
	return client.GetDatabase(databaseName);
});

builder.Services.AddSingleton<ElasticsearchClient>(_ =>
{
	var uri = builder.Configuration["Elasticsearch:Uri"]
		?? throw new InvalidOperationException("Elasticsearch:Uri não configurado.");
	var settings = new ElasticsearchClientSettings(new Uri(uri));
	return new ElasticsearchClient(settings);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

	try
	{
		logger.LogInformation("Aplicando migrations pendentes, se existirem.");
		await db.Database.MigrateAsync();

		logger.LogInformation("Validando conexão com o banco de dados.");

		if (!db.Database.CanConnect())
		{
			throw new InvalidOperationException("Não foi possível conectar ao banco de dados.");
		}

		logger.LogInformation("Conexão com o banco validada com sucesso.");
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "Erro ao conectar ou migrar o banco de dados.");
		throw;
	}
}

app.UseSwaggerConfiguration();

app.UseErrorHandling();
app.UseCorrelationId();
app.UseRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

static string ResolveMongoConnectionString(IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
{
	var connectionString = configuration["MongoDb:ConnectionString"]
		?? throw new InvalidOperationException("MongoDb:ConnectionString não configurada.");

	if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var mongoUri))
	{
		return connectionString;
	}

	if (!mongoUri.Host.Equals("mongo", StringComparison.OrdinalIgnoreCase))
	{
		return connectionString;
	}

	var isKubernetes = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"));
	var isContainer = string.Equals(
		Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
		"true",
		StringComparison.OrdinalIgnoreCase);

	if (isKubernetes)
	{
		var updated = ReplaceMongoHost(mongoUri, "mongodb");
		logger.LogWarning("MongoDb host 'mongo' detectado em Kubernetes. Usando host '{MongoHost}'.", "mongodb");
		return updated;
	}

	if (!isContainer)
	{
		var updated = ReplaceMongoHost(mongoUri, "localhost");
		logger.LogWarning("MongoDb host 'mongo' detectado fora de container. Usando host '{MongoHost}'.", "localhost");
		return updated;
	}

	return connectionString;
}

static string ReplaceMongoHost(Uri mongoUri, string host)
{
	var uriBuilder = new UriBuilder(mongoUri)
	{
		Host = host
	};

	return uriBuilder.Uri.ToString();
}