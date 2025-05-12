using Azure.Messaging.ServiceBus;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.MessagePublisher;

public class AzureMessagePublisher : IMessagePublisher
{
    private ServiceBusSender _Sender;
    private ServiceBusClient _Client;

    private string _TheQueue = "";

    public AzureMessagePublisher(IMessagePublisherSettings settings)
    {
        var clientOptions = new ServiceBusClientOptions
        {
            TransportType = ServiceBusTransportType.AmqpWebSockets
        };


        _Client = new ServiceBusClient(settings.azure_connection_string, clientOptions);
        
    }

    public async Task<bool> PublishAsync(MessageObject message, string topic_or_queue)
    {
        if(_Sender == null || _TheQueue != topic_or_queue)
        {
            _TheQueue = topic_or_queue;
            _Sender = _Client.CreateSender(topic_or_queue);
        }
        

        using (var batch = await _Sender.CreateMessageBatchAsync())
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);

            if (!batch.TryAddMessage(new ServiceBusMessage(json)))
            {
                throw new Exception($"Could not send azure service bus message with: {json}");
            }

            await _Sender.SendMessagesAsync(batch);
        }

        return true;
    }

    public async Task<string?> GetNextMessage()
    {
        throw new NotImplementedException();
    }

    public async Task CloseConnection()
    {
        if (!_Sender.IsClosed)
            await _Sender.CloseAsync();
    }

    public async Task Dispose()
    {
        if (_Sender != null && !_Sender.IsClosed)
        {
            await _Sender.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_Sender != null && !_Sender.IsClosed)
        {
            await _Sender.CloseAsync();
        }
    }
}