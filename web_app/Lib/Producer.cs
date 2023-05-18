using RabbitMQ.Client;
using System.Text;

namespace Lib
{
    public class Producer
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
}
