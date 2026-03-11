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
using Microsoft.Extensions.Logging;

namespace Catalog.Infra.MessageBus;

public class PaymentProcessedConsumer : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentProcessedConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public PaymentProcessedConsumer(
        IOptions<RabbitMqSettings> options,
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _settings = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Iniciando ExecuteAsync em PaymentProcessedConsumer");
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
            queue: "PaymentProcessedEvent-Catalog",
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: "PaymentProcessedEvent-Catalog",
            exchange: _settings.ExchangeName,
            routingKey: "PaymentProcessedEvent");

        _logger.LogInformation("PaymentProcessedConsumer conectado ao RabbitMQ e aguardando mensagens na fila PaymentProcessedEvent-Catalog");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            _logger.LogTrace("Mensagem recebida em PaymentProcessedConsumer");
            using var scope = _scopeFactory.CreateScope();

            var orderService = scope.ServiceProvider
                .GetRequiredService<IOrderService>();

            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogTrace("Payload recebido no consumidor com tamanho {Length}", json.Length);
            var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(json);

            if (evt != null)
            {
                _logger.LogTrace("Evento desserializado com sucesso para OrderId {OrderId}", evt.OrderId);
                await orderService.AddGameToUserLibraryAsync(evt);
                _logger.LogInformation("Evento PaymentProcessed processado para OrderId {OrderId}", evt.OrderId);
            }
            else
            {
                _logger.LogWarning("Falha ao desserializar PaymentProcessedEvent");
            }
        };

        _channel.BasicConsume(
            queue: "PaymentProcessedEvent-Catalog",
            autoAck: true,
            consumer: consumer);

        _logger.LogTrace("BasicConsume configurado em PaymentProcessedConsumer");

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _logger.LogTrace("Dispose iniciado em PaymentProcessedConsumer");
        _channel?.Close();
        _connection?.Close();
        _logger.LogTrace("Dispose finalizado em PaymentProcessedConsumer");
        base.Dispose();
    }
}
