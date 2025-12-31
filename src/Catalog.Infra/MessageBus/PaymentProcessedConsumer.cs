using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Dto.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Catalog.Infra.MessageBus;

public class PaymentProcessedConsumer : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public PaymentProcessedConsumer(
        IOptions<RabbitMqSettings> options,
        IServiceScopeFactory scopeFactory)
    {
        _settings = options.Value;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        _channel.QueueDeclare(
            queue: "PaymentProcessedEvent",
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: "PaymentProcessedEvent",
            exchange: _settings.ExchangeName,
            routingKey: "PaymentProcessedEvent");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            using var scope = _scopeFactory.CreateScope();

            var orderService = scope.ServiceProvider
                .GetRequiredService<IOrderService>();

            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(json);

            if (evt != null)
            {
                await orderService.AddGameToUserLibraryAsync(evt);
            }
        };

        _channel.BasicConsume(
            queue: "PaymentProcessedEvent",
            autoAck: true,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
