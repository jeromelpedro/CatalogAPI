using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Catalog.Domain.Dto;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Catalog.Infra.MessageBus
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IOptions<RabbitMqSettings> options, ILogger<RabbitMqPublisher> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public Task PublishAsync<T>(T message, string topic)
        {
            _logger.LogTrace("Iniciando PublishAsync em RabbitMqPublisher para topic {Topic} e mensagem {MessageType}", topic, typeof(T).Name);
            // Serializa a mensagem
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            // Cria factory (sincronamente)
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                // Opcional: configurar VirtualHost, AutomaticRecoveryEnabled, etc.
            };

            // A API 6.5 usa CreateConnection / CreateModel (sincronos)
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                _logger.LogTrace("Conexao e canal RabbitMQ criados para topic {Topic}", topic);
                // Declara exchange (durable, topic)
                channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                // Garante que a fila existe
                channel.QueueDeclare(
                    queue: topic,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                // Garante o binding
                channel.QueueBind(
                    queue: topic,
                    exchange: _settings.ExchangeName,
                    routingKey: topic,
                    arguments: null);

                // Cria propriedades corretamente
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";

                // Publica (sincrono)
                channel.BasicPublish(
                    exchange: _settings.ExchangeName,
                    routingKey: topic,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Mensagem publicada no RabbitMQ. Exchange: {Exchange}, RoutingKey: {RoutingKey}", _settings.ExchangeName, topic);
            }

            // Retornamos Task.CompletedTask para manter a assinatura async
            _logger.LogTrace("Finalizando PublishAsync em RabbitMqPublisher para topic {Topic}", topic);
            return Task.CompletedTask;
        }
    }
}
