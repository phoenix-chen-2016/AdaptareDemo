using System.Diagnostics;
using Adaptare;

namespace NatsService.MessageQueue;

public class PublishHandler(ILogger<PublishHandler> logger) : IMessageHandler<string>
{
	private static ActivitySource _ActivitySource = new ActivitySource("NatsService.PublishHandler");

	public ValueTask HandleAsync(
		string subject,
		string data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		using var activity = _ActivitySource.StartActivity("PublishHandler", ActivityKind.Internal);

		logger.LogInformation("Received message on subject: {Subject} with data: {Data}", subject, data);

		return ValueTask.CompletedTask;
	}
}