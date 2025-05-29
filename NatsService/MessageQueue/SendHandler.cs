using Adaptare;

namespace NatsService.MessageQueue;

public class SendHandler(ILogger<SendHandler> logger) : IMessageHandler<string>
{
	public async ValueTask HandleAsync(
		string subject,
		string data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		await Task.Delay(2000, cancellationToken).ConfigureAwait(false);

		logger.LogInformation("Received message on subject: {Subject} with data: {Data}", subject, data);
	}
}