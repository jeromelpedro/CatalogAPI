namespace Catalog.Domain.Interfaces
{
	public interface IServiceBus
	{
		Task PublishAsync(string topic, object message);
	}
}
