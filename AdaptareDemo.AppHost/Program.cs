var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("Rabbit")
	.WithManagementPlugin();
var nats = builder.AddNats("Nats")
	.WithJetStream();

builder.AddProject<Projects.RabbitMQService>("RabbitMQService")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq);

builder.AddProject<Projects.RabbitMQClient>("RabbitMQClient")
	.WithReference(rabbitmq)
	.WaitFor(rabbitmq)
	.WithUrls(ctx =>
	{
		foreach (var url in ctx.Urls)
		{
			if (string.IsNullOrEmpty(url.DisplayText))
			{
				url.DisplayText = $"Swagger ({url.Endpoint?.Scheme?.ToUpper()})";
				url.Url = "/swagger/index.html";
			}
		}
	});

builder.AddProject<Projects.NatsService>("NatsService")
	.WithReference(nats)
	.WaitFor(nats);

builder.AddProject<Projects.NatsClient>("NatsClient")
	.WithReference(nats)
	.WaitFor(nats)
	.WithUrls(ctx =>
	{
		foreach (var url in ctx.Urls)
		{
			if (string.IsNullOrEmpty(url.DisplayText))
			{
				url.DisplayText = $"Swagger ({url.Endpoint?.Scheme?.ToUpper()})";
				url.Url = "/swagger/index.html";
			}
		}
	});

builder.Build().Run();
