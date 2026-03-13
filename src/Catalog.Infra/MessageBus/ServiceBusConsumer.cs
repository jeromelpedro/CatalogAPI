using Azure.Messaging.ServiceBus;
using Catalog.Application.Interfaces;
using Catalog.Domain.Dto.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Catalog.Infra.MessageBus
{
	public class ServiceBusConsumer(ServiceBusClient _client, IServiceProvider _serviceProvider, IConfiguration _configuration, ILogger<ServiceBusConsumer> _logger) : BackgroundService
	{
		private ServiceBusProcessor _processor;
	
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var queueNamePaymentProcessed = _configuration["ServiceBus:QueueNamePaymentProcessed"];
			var subscriptionPaymentProcessed = _configuration["ServiceBus:SubscriptionPaymentProcessed"];			

			_processor = _client.CreateProcessor(queueNamePaymentProcessed, subscriptionPaymentProcessed, new ServiceBusProcessorOptions
			{
				MaxConcurrentCalls = 1,
				AutoCompleteMessages = false
			});

			_processor.ProcessMessageAsync += ProcessMessage;
			_processor.ProcessErrorAsync += ProcessError;

			await _processor.StartProcessingAsync(stoppingToken);

			_logger.LogInformation("ServiceBus Consumer iniciado");
		}

		private async Task ProcessMessage(ProcessMessageEventArgs args)
		{
			_logger.LogTrace("Mensagem recebida em ServiceBusConsumer");

			using var scope = _serviceProvider.CreateScope();

			var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

			var json = Encoding.UTF8.GetString(args.Message.Body.ToArray());

			_logger.LogTrace("Payload recebido no consumidor com tamanho {Length}", json.Length);

			var evt = JsonSerializer.Deserialize<PaymentProcessedEvent>(json);

			if (evt != null)
			{
				_logger.LogTrace("Evento desserializado com sucesso para OrderId {OrderId}", evt.OrderId);

				await orderService.AddGameToUserLibraryAsync(evt);

				_logger.LogInformation("Evento PaymentProcessed processado para OrderId {OrderId}",evt.OrderId);

				await args.CompleteMessageAsync(args.Message);
			}
			else
			{
				_logger.LogWarning("Falha ao desserializar PaymentProcessedEvent");

				await args.AbandonMessageAsync(args.Message);
			}
		}

		private Task ProcessError(ProcessErrorEventArgs args)
		{
			_logger.LogError(args.Exception, "Erro no ServiceBus");
			return Task.CompletedTask;
		}
	}
}
