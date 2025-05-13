using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private IConnection _connection;

        public async Task SendMessage(object message, string queueName)
        {
            if(await ConnectionExists())
            {
                using var channel = await _connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queueName, false, false, false, null);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(exchange: string.Empty, routingKey: queueName, body: body);
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
