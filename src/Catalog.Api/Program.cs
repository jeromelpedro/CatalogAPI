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

// RabbitMQ
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// EF Core
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.MapControllers();

await app.RunAsync();
