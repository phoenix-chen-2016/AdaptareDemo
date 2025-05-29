using System.Net;
using Adaptare;
using Microsoft.AspNetCore.Mvc;

namespace NatsClient.Apis;

public static class MessageApis
{
	public static RouteGroupBuilder MapMessageApis(this WebApplication app)
	{
		var group = app.MapGroup("/messages");
		{
			group.MapPut(
				":publish",
				async (
					[FromServices] IMessageSender messageSender,
					[FromBody] string message,
					CancellationToken cancellationToken) =>
				{
					await messageSender.PublishAsync(
						"nats.publish",
						message,
						cancellationToken).ConfigureAwait(false);

					return Results.Accepted();
				})
				.WithSummary("Publish message to the nats.publish exchange")
				.WithName("PublishMessage")
				.Produces((int)HttpStatusCode.Accepted);

			group.MapPut(
				":send",
				async (
					[FromServices] IMessageSender messageSender,
					[FromBody] string message,
					CancellationToken cancellationToken) =>
				{
					await messageSender.SendAsync(
						"nats.send",
						message,
						cancellationToken).ConfigureAwait(false);

					return Results.Ok();
				})
				.WithSummary("Send message to the nats.send exchange and wait for a response")
				.WithName("SendMessage")
				.Produces<string>((int)HttpStatusCode.OK);

			group.MapPut(
				":process",
				async (
					[FromServices] IMessageSender messageSender,
					[FromBody] string message,
					CancellationToken cancellationToken) =>
				{
					var result = await messageSender.RequestAsync<string, string>(
						"nats.process",
						message,
						cancellationToken).ConfigureAwait(false);
					return Results.Ok(result); // Changed from Results.Accepted() to return the result
				})
				.WithSummary("Process message with the nats.process exchange")
				.WithName("ProcessMessage")
				.Produces<string>((int)HttpStatusCode.OK);
		}

		return group;
	}
}