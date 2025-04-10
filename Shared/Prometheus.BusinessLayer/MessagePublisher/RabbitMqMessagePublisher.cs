using Prometheus.BusinessLayer.Interfaces;
using Prometheus.BusinessLayer.Models;
using Prometheus.Models.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Prometheus.BusinessLayer.MessagePublisher;

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


        var json = JsonSerializer.Serialize(message);
        var bytes = Encoding.UTF8.GetBytes(json);

        await _Channel.BasicPublishAsync(_RabbitMqConnectionSettings.rabbitmq_exchange, _RabbitMqConnectionSettings.rabbitmq_routing_key, bytes);

        return true;
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
