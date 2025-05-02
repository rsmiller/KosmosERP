using KosmosERP.BusinessLayer.Models;

namespace KosmosERP.BusinessLayer.Interfaces;

public interface IMessagePublisher
{
    Task<bool> PublishAsync(MessageObject message, string topic_or_queue);
    Task CloseConnection();
}
