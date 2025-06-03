using Adaptare;

namespace NatsService.MessageQueue;

public class JetHandler(ILogger<JetHandler> logger) : IAcknowledgeMessageHandler<string>
{
	public ValueTask HandleAsync(IAcknowledgeMessage<string> msg, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Received message on subject: {Subject} with data: {Data}", msg.Subject, msg.Data);

		return msg.AckAsync(cancellationToken);
	}
}