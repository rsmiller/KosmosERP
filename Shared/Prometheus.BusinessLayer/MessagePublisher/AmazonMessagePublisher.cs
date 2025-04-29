using Prometheus.BusinessLayer.Interfaces;
using Prometheus.BusinessLayer.Models;
using Prometheus.Models.Interfaces;

namespace Prometheus.BusinessLayer.MessagePublisher;

public class AmazonMessagePublisher : IMessagePublisher
{
    public AmazonMessagePublisher(IMessagePublisherSettings settings)
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
