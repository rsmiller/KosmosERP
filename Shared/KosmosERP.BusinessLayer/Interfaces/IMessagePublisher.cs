using KosmosERP.BusinessLayer.Models;

namespace KosmosERP.BusinessLayer.Interfaces;

public interface IMessagePublisher
{
    Task<string?> GetNextMessage(string topic_or_queue);
    Task<bool> PublishAsync(MessageObject message, string topic_or_queue);
    Task CloseConnection();
}
