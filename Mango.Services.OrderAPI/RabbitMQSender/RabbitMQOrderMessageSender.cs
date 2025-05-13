using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public class RabbitMQOrderMessageSender : IRabbitMQOrderMessageSender
    {
        private IConnection _connection;
        private const string OrderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";
        private const string OrderCreated_EmailUpdateQueue = "EmailUpdateQueue";

        public async Task SendMessage(object message, string exchangeName)
        {
            if(await ConnectionExists())
            {
                using var channel = await _connection.CreateChannelAsync();

                // Fanout
                //await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: false);

                // Direct
                await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: false);
                await channel.QueueDeclareAsync(OrderCreated_EmailUpdateQueue, false, false, false, null);
                await channel.QueueDeclareAsync(OrderCreated_RewardsUpdateQueue, false, false, false, null);

                await channel.QueueBindAsync(OrderCreated_EmailUpdateQueue, exchangeName, "EmailUpdate");
                await channel.QueueBindAsync(OrderCreated_RewardsUpdateQueue, exchangeName, "RewardsUpdate");
                // End direct

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                // Fanout
                //await channel.BasicPublishAsync(exchange: exchangeName, routingKey: string.Empty, body: body);

                // Direct
                await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "EmailUpdate", body: body);
                await channel.BasicPublishAsync(exchange: exchangeName, routingKey: "RewardsUpdate", body: body);
            }       
        }

        private async Task CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost"
                };

                _connection = await factory.CreateConnectionAsync();
            }
            catch (Exception ex)
            {
            }
        }

        private async Task<bool> ConnectionExists()
        {
            if(_connection != null)
            {
                return true;
            }
            await CreateConnection();
            return true;
        }
    }
}
