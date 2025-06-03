using NATS.Client.Core;
using NATS.Client.JetStream.Models;
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
			.ConfigJetStream(new StreamConfig("jet", ["nats.jet"]))
			.AddHandler<PublishHandler>("nats.publish", NatsDefaultSerializerRegistry.Default)
			.AddReplyHandler<SendHandler>("nats.send", NatsDefaultSerializerRegistry.Default)
			.AddProcessor<RequestProcessor>("nats.process", NatsDefaultSerializerRegistry.Default)
			.AddJetStreamHandler<JetHandler>("nats.jet", "jet", new ConsumerConfig("aaa"), NatsDefaultSerializerRegistry.Default));
}

var app = builder.Build();
{
	app.MapDefaultEndpoints();
}

app.Run();
