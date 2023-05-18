using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace Lib
{
    public class Consumer
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

                return messages.ToString().Length > 0 ? messages.ToString().Trim() : "No messages";
            }
        }
    }
}
