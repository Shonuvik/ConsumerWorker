using ConsumerFiapWorker.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace ConsumerFiapWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var connection = connectionFactory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare("queue", false, false, false, null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, eventArgs) =>
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var jsonToObject = JsonSerializer.Deserialize<User>(json) ?? new User();

                    Console.WriteLine(jsonToObject.Email + "-" + jsonToObject.UserName);
                };

                channel.BasicConsume("fila", true, consumer);
            }

            await Task.Delay(10000, stoppingToken);
        }
    }
}

