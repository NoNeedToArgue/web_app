using System.Text;
using Prometheus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/reset", (ILogger<Program> logger) =>
{
    logger.LogInformation($"Path: /reset  Time: {DateTime.Now.ToLongTimeString()}");
    Latency.ResetLatency();
    return "Application reset";
});

app.MapGet("/producer", (ILogger<Program> logger) =>
{
    string messageInfo = Producer.Send();
    logger.LogInformation($"Path: /producer  Time: {DateTime.Now.ToLongTimeString()}\n{messageInfo}");
    return messageInfo;
});

app.MapGet("/consumer", (ILogger<Program> logger) =>
{
    string messageInfo = Consumer.Receive();
    logger.LogInformation($"Path: /consumer  Time: {DateTime.Now.ToLongTimeString()}\n{messageInfo}");
    return messageInfo;
 });

app.MapGet("/data", async (ILogger<Program> logger) =>
{
    logger.LogInformation($"Path: /data  Time: {DateTime.Now.ToLongTimeString()}");
    int latency = Latency.GetLatency();
    await Task.Delay(latency);
    return $"Application latency: {latency}";
});

app.MapGet("/", async (context) =>
{
    app.Logger.LogInformation($"Processing request {context.Request.QueryString}  Time: {DateTime.Now.ToLongTimeString()}");
    context.Response.ContentType = "text/html; charset=utf-8";
    var stringBuilder = new StringBuilder("<h3>Request</h3><table>");
    stringBuilder.Append("<tr><td>Parameter</td><td>Value</td></tr>");
    foreach (var param in context.Request.Query)
    {
        stringBuilder.Append($"<tr><td>{param.Key}</td><td>{param.Value}</td></tr>");
    }
    stringBuilder.Append("</table>");
    await context.Response.WriteAsync(stringBuilder.ToString());
});

app.UseMetricServer();

app.Run();

static class Latency
{
    static int counter = 1;
    public static int GetLatency() => counter++ * 500;
    public static void ResetLatency() => counter = 1;
}

static class Producer
{
    private static int _counter = 0;

    public static string Send()
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "test-queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            string message = $"Message N {_counter++}";
            
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: "test-queue",
                                 basicProperties: null,
                                 body: body);

            return $"{message} sent";
        }
    }
}

static class Consumer
{
    public static string Receive()
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "test-queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            var messages = new StringBuilder();

            consumer.Received += (sender, e) =>
            {
                var body = e.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                messages.Append(message + " received\n");
            };

            channel.BasicConsume(queue: "test-queue",
                                 autoAck: true,
                                 consumer: consumer);

            Thread.Sleep(100);

            return messages.ToString().Count() > 0 ? messages.ToString().Trim() : "No messages";
        }    
    }
}