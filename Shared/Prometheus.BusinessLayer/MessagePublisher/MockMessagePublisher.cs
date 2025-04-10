using Prometheus.BusinessLayer.Interfaces;
using Prometheus.BusinessLayer.Models;
using Prometheus.Models.Interfaces;

namespace Prometheus.BusinessLayer.MessagePublisher;

public class MockMessagePublisher : IMessagePublisher
{
    public MockMessagePublisher(IMessagePublisherSettings settings)
    {

    }

    public async Task<bool> PublishAsync(MessageObject message, string topic_or_queue)
    {
        return true;
    }
}