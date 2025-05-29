using System.Net;
using Adaptare;
using Microsoft.AspNetCore.Mvc;

namespace RabbitMQClient.Apis;

public static class MessageApis
{
	public static RouteGroupBuilder MapMessageApis(this WebApplication app)
	{
		var group = app.MapGroup("/messages");
		{
			group.MapPut(
				"",
				async (
					[FromServices] IMessageSender messageSender,
					[FromBody] string message,
					CancellationToken cancellationToken) =>
				{
					await messageSender.PublishAsync(
						"test.demo",
						message,
						cancellationToken).ConfigureAwait(false);

					return Results.Accepted();
				})
				.WithSummary("Publish message to the test.demo exchange")
				.WithName("PublishMessage")
				.Produces((int)HttpStatusCode.Accepted);
		}

		return group;
	}
}