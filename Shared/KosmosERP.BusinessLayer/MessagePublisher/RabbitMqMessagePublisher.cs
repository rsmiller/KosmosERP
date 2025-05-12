using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models;
using KosmosERP.Models.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace KosmosERP.BusinessLayer.MessagePublisher;

public class RabbitMqMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private IConnectionFactory _ConnectionFactory;
    private IConnection _Connection;
    private IChannel _Channel;
    private IMessagePublisherSettings _RabbitMqConnectionSettings;

    public RabbitMqMessagePublisher(IMessagePublisherSettings settings)
    {
        _RabbitMqConnectionSettings = settings;
        _ConnectionFactory = new ConnectionFactory() { HostName = settings.rabbitmq_host, UserName = settings.rabbitmq_username, Password = settings.rabbitmq_password, VirtualHost = settings.rabbitmq_virtual_host };

    }

    private async Task Setup()
    {
        _Connection = await _ConnectionFactory.CreateConnectionAsync();
        _Channel = await _Connection.CreateChannelAsync();
    }

    private async Task SetQueue(string topic_or_queue)
    {
        await _Channel.QueueBindAsync(topic_or_queue, _RabbitMqConnectionSettings.rabbitmq_exchange, _RabbitMqConnectionSettings.rabbitmq_routing_key);
    }

    public async Task<bool> PublishAsync(MessageObject message, string topic_or_queue)
    {
        if (_Connection == null || _Connection.IsOpen == false)
            await Setup();

        if (string.IsNullOrEmpty(_Channel.CurrentQueue) || _Channel.CurrentQueue != topic_or_queue)
            await SetQueue(topic_or_queue);


        var json = System.Text.Json.JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);

        await _Channel.BasicPublishAsync(_RabbitMqConnectionSettings.rabbitmq_exchange, _RabbitMqConnectionSettings.rabbitmq_routing_key, bytes);

        return true;
    }

    public async Task<string?> GetNextMessage()
    {
        if (_Connection == null || _Connection.IsOpen == false)
            await Setup();
        
        var result = await _Channel.BasicGetAsync(_Channel.CurrentQueue, false);

        if (result != null && result != null)
        {
            try
            {
                var body = result.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await _Channel.BasicAckAsync(result.DeliveryTag, false);

                return message;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        return null;
    }

    public async Task CloseConnection()
    {
        if (_Connection != null && _Connection.IsOpen)
        {
            await _Connection.CloseAsync();
        }
    }

    public async Task Dispose()
    {
        if (_Connection != null && _Connection.IsOpen)
        {
            await _Connection.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_Connection != null && _Connection.IsOpen)
        {
            await _Connection.CloseAsync();
        }
    }
}
