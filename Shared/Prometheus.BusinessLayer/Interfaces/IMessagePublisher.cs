using Prometheus.BusinessLayer.Models;

namespace Prometheus.BusinessLayer.Interfaces;

public interface IMessagePublisher
{
    Task<bool> PublishAsync(MessageObject message, string topic_or_queue);
    Task CloseConnection();
}
