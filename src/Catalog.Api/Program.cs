using Catalog.Api.Configurations;
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

builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

DatabaseInitializer.EnsureDatabase(builder.Configuration);

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
