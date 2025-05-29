using Adaptare;

namespace NatsService.MessageQueue;

public class PublishHandler(ILogger<PublishHandler> logger) : IMessageHandler<string>
{
	public ValueTask HandleAsync(
		string subject,
		string data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Received message on subject: {Subject} with data: {Data}", subject, data);

		return ValueTask.CompletedTask;
	}
}