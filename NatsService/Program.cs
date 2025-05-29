using NATS.Client.Core;
using NatsService.MessageQueue;

var builder = WebApplication.CreateBuilder(args);
{
	builder.AddServiceDefaults();

	builder.Services
		.AddSingleton(sp => new NatsConnection(
			NatsOpts.Default with
			{
				Url = builder.Configuration.GetConnectionString("Nats")
						?? throw new InvalidOperationException("NATS connection string is not configured."),
			}))
		.AddMessageQueue()
		.AddNatsMessageQueue(config => config
			.ConfigureResolveConnection(sp => sp.GetRequiredService<NatsConnection>())
			.AddHandler<PublishHandler>("nats.publish", NatsDefaultSerializerRegistry.Default)
			.AddReplyHandler<SendHandler>("nats.send", NatsDefaultSerializerRegistry.Default)
			.AddProcessor<RequestProcessor>("nats.process", NatsDefaultSerializerRegistry.Default));
}

var app = builder.Build();
{
	app.MapDefaultEndpoints();
}

app.Run();
