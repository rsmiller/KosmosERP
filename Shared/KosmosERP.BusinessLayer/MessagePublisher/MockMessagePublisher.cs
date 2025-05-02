using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.MessagePublisher;

public class MockMessagePublisher : IMessagePublisher
{
    public MockMessagePublisher(IMessagePublisherSettings settings)
    {

    }

    public async Task<bool> PublishAsync(MessageObject message, string topic_or_queue)
    {
        return true;
    }

    public async Task CloseConnection()
    {
        
    }
}