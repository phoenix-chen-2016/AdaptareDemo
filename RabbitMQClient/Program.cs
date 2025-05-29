using Adaptare;
using Adaptare.RabbitMQ;
using Adaptare.RabbitMQ.Configuration;
using RabbitMQ.Client;
using RabbitMQClient.Apis;

var builder = WebApplication.CreateBuilder(args);
{
	builder.AddServiceDefaults();

	builder.Services
		.AddOpenApi("v1");

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
				ConnectionPromise = sp.GetRequiredService<TaskResource<IConnection>>()
			}))
		.AddRabbitGlobPatternExchange("*", "amq.direct");
}

var app = builder.Build();
{
	app.MapMessageApis()
		.WithOpenApi();

	app.MapOpenApi();
	app.MapDefaultEndpoints();
	app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "API v1"));
}

app.Run();
