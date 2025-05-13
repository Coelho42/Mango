
using System.Text;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Services;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        private IConnection _connection;
        private IChannel _channel;
        private string exchangeName = "";
        private const string orderCreated_EmailUpdateQueue = "EmailUpdateQueue";
        string queueName = string.Empty;
 
        public RabbitMQOrderConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
            exchangeName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Fanout
            // await _channel.ExchangeDeclareAsync(exchange: _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"), ExchangeType.Fanout);
            //var queueDeclare = await _channel.QueueDeclareAsync();
            //queueName = queueDeclare.QueueName;
            //await _channel.QueueBindAsync(queueName, _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic"), string.Empty);

            // Direct
            await _channel.ExchangeDeclareAsync(exchange: exchangeName, ExchangeType.Direct);
            await _channel.QueueDeclareAsync(orderCreated_EmailUpdateQueue, false, false, false, null);
            await _channel.QueueBindAsync(orderCreated_EmailUpdateQueue, exchangeName, "EmailUpdate");

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
            await _channel.BasicConsumeAsync(orderCreated_EmailUpdateQueue, false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessage(RewardsMessage rewardsMessage)
        {
            await _emailService.LogOrderPlaced(rewardsMessage);
        }
    }
}
