using NATS.Client.Core;
using NatsClient.Apis;

var builder = WebApplication.CreateBuilder(args);
{
	builder.AddServiceDefaults();

	builder.Services
		.AddOpenApi("v1");

	builder.Services
		.AddSingleton(sp => new NatsConnection(
			NatsOpts.Default with
			{
				Url = builder.Configuration.GetConnectionString("Nats")
						?? throw new InvalidOperationException("NATS connection string is not configured."),
			}))
		.AddMessageQueue()
		.AddNatsMessageQueue(config => config
			.ConfigureResolveConnection(sp => sp.GetRequiredService<NatsConnection>()))
		.AddNatsGlobPatternExchange("*");
}

var app = builder.Build();
{
	app.MapMessageApis()
		.WithOpenApi();

	app.MapOpenApi();
	app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "API v1"));
	app.MapDefaultEndpoints();
}

app.Run();
