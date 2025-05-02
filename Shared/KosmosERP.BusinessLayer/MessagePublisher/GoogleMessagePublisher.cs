using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.MessagePublisher;

public class GoogleMessagePublisher : IMessagePublisher
{
    public GoogleMessagePublisher(IMessagePublisherSettings settings)
    {

    }

    public Task<bool> PublishAsync(MessageObject message, string topic_or_queue)
    {
        throw new NotImplementedException();
    }

    public async Task CloseConnection()
    {
        throw new NotImplementedException();
    }
}