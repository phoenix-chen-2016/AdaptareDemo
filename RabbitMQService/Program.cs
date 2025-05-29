using Adaptare;
using Adaptare.RabbitMQ;
using Adaptare.RabbitMQ.Configuration;
using RabbitMQ.Client;
using RabbitMQService.MessageQueue;

var builder = WebApplication.CreateBuilder(args);
{
	builder.AddServiceDefaults();

	builder.Services
		.AddSingleton(sp =>
		{
			var factory = new ConnectionFactory
			{
				Uri = new Uri(builder.Configuration.GetConnectionString("Rabbit")
					?? throw new InvalidOperationException("RabbitMQ connection string is not configured.")),
			};

			return factory;
		})
		.AddSingleton(sp =>
		{
			var factory = sp.GetRequiredService<ConnectionFactory>();
			var hostLifetime = sp.GetRequiredService<IHostApplicationLifetime>();

			return new TaskResource<IConnection>(factory.CreateConnectionAsync(hostLifetime.ApplicationStopping));
		})
		.AddMessageQueue()
		.AddRabbitMessageQueue(config => config
			.UseSerializerRegistry<RabbitMQSerializerRegistry>()
			.ConfigureConnection(sp => new RabbitMQConnectionOptions
			{
				ConnectionPromise = sp.GetRequiredService<TaskResource<IConnection>>(),
				SetupQueueAndExchange = async (channel, ct) =>
				{
					await channel.QueueDeclareAsync("demo", durable: true, autoDelete: false, cancellationToken: ct).ConfigureAwait(false);

					await channel.QueueBindAsync(
						"demo",
						"amq.direct",
						"test.demo",
						cancellationToken: ct).ConfigureAwait(false);
				},
			})
			.AddHandler<DemoHandler>("demo"));
}

var app = builder.Build();
{
	app.MapDefaultEndpoints();
}
app.Run();
