using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Catalog.Domain.Dto;
using Catalog.Domain.Interfaces;

namespace Catalog.Infra.MessageBus
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;
        }

        public Task PublishAsync<T>(T message, string topic)
        {
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
            }

            // Retornamos Task.CompletedTask para manter a assinatura async
            return Task.CompletedTask;
        }
    }
}
