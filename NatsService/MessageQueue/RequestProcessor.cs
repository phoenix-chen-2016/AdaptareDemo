using Adaptare;

namespace NatsService.MessageQueue;

public class RequestProcessor(ILogger<RequestProcessor> logger) : IMessageProcessor<string, string>
{
	public ValueTask<string> HandleAsync(
		string subject,
		string data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Processing request on subject: {Subject} with data: {Data}", subject, data);

		return ValueTask.FromResult(DateTime.Now.ToString("o"));
	}
}