
namespace Catalog.Domain.Interfaces
{
    public interface IRabbitMqPublisher
    {
        //Task PublishAsync<T>(string topic, T message);

        Task PublishAsync<T>(T message, string queueName);
    }
}
