
using System.Text;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Services.RewardAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly RewardsService _rewardsService;
        private IConnection _connection;
        private IChannel _channel;
        private const string orderCreated_RewardsUpdateQueue = "RewardsUpdateQueue";
        private string exchangeName = "";
        string queueName = string.Empty;
 
        public RabbitMQOrderConsumer(IConfiguration configuration, RewardsService rewardsService)
        {
            _configuration = configuration;
            _rewardsService = rewardsService;
            exchangeName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Fanout
            //await _channel.ExchangeDeclareAsync(exchange: _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"), ExchangeType.Fanout);
            //var queueDeclare = await _channel.QueueDeclareAsync();
            //queueName = queueDeclare.QueueName;
            //await _channel.QueueBindAsync(queueName, _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"), string.Empty);

            // Direct
            await _channel.ExchangeDeclareAsync(exchange: exchangeName, ExchangeType.Direct);
            await _channel.QueueDeclareAsync(orderCreated_RewardsUpdateQueue, false, false, false, null);
            await _channel.QueueBindAsync(orderCreated_RewardsUpdateQueue, exchangeName, "RewardsUpdate");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                RewardsMessage rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(content);
                await HandleMessage(rewardsMessage);
                await _channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            // Fanout
            //await _channel.BasicConsumeAsync(queueName, false, consumer);

            //Direct
            await _channel.BasicConsumeAsync(orderCreated_RewardsUpdateQueue, false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessage(RewardsMessage rewardsMessage)
        {
            await _rewardsService.UpdateRewards(rewardsMessage);
        }
    }
}
