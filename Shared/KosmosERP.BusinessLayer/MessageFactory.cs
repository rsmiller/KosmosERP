using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.MessagePublisher;
using KosmosERP.Database;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer;

public interface IMessageFactory {
    IMessagePublisher GetPublisher();
}

public class MessageFactory : IMessageFactory
{
    private IBaseERPContext _Context;
    private IMessagePublisherSettings _Settings;
    private IMessagePublisher _MessagePublisher;

    public MessageFactory(IMessagePublisherSettings settings, IBaseERPContext context)
    {
        _Context = context;
        _Settings = settings;

        ValidateSettings(settings);

        if (settings.account_provider.Equals(MessagePublisherType.AWS, StringComparison.OrdinalIgnoreCase))
        {
            _MessagePublisher = new AmazonMessagePublisher(settings);
        }
        else if (settings.account_provider.Equals(MessagePublisherType.Azure, StringComparison.OrdinalIgnoreCase))
        {
            _MessagePublisher = new AzureMessagePublisher(settings);
        }
        else if (settings.account_provider.Equals(MessagePublisherType.Google, StringComparison.OrdinalIgnoreCase))
        {
            _MessagePublisher = new GoogleMessagePublisher(settings);
        }
        else if (settings.account_provider.Equals(MessagePublisherType.RabbitMq, StringComparison.OrdinalIgnoreCase))
        {
            _MessagePublisher = new RabbitMqMessagePublisher(settings);
        }
        else if (settings.account_provider.Equals(MessagePublisherType.MOCK, StringComparison.OrdinalIgnoreCase))
        {
            _MessagePublisher = new MockMessagePublisher(settings);
        }
        else if (settings.account_provider.Equals(MessagePublisherType.Database, StringComparison.OrdinalIgnoreCase))
        {

            _MessagePublisher = new DatabaseMessagePublisher(_Context);
        }
        else
        {
            throw new ArgumentNullException("Messaging account provider not supported.");
        }
    }

    public IMessagePublisher GetPublisher()
    {
        return _MessagePublisher;
    }

    private void ValidateSettings(IMessagePublisherSettings settings)
    {
        if (string.IsNullOrEmpty(settings.account_provider))
            throw new ArgumentNullException("Messaging account provider cannot be null or empty.");

        if (string.IsNullOrEmpty(settings.transaction_movement_topic))
            throw new ArgumentNullException($"Messaging topic or queue {RequiredMessageTopics.TransactionMovementTopic} must exist and cannot be null or empty.");
    }
}
