var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("Rabbit")
	.WithManagementPlugin();
var nats = builder.AddNats("Nats");

builder.AddProject<Projects.RabbitMQService>("RabbitMQService")
	.WithReference(rabbitmq);

builder.AddProject<Projects.RabbitMQClient>("RabbitMQClient")
	.WithReference(rabbitmq);

builder.AddProject<Projects.NatsService>("NatsService")
	.WithReference(nats);

builder.AddProject<Projects.NatsClient>("NatsClient")
	.WithReference(nats);

builder.Build().Run();
