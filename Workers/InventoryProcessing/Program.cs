using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer;
using KosmosERP.Models;
using Newtonsoft.Json;
using KosmosERP.BusinessLayer.Models;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;


Console.WriteLine("Hello, I am a bot!");

var options = new DbContextOptionsBuilder<DbContext>()
                    .UseMySQL(Environment.GetEnvironmentVariable("DatabaseConnectionString"))
                    .Options;

var _Context = new ERPDbContext(options);

var settings = new MessagePublisherSettings()
{
    account_provider = Environment.GetEnvironmentVariable("MessagePublisherAccountProvider"),
    rabbitmq_host = Environment.GetEnvironmentVariable("RabbitMQHost"),
    rabbitmq_username = Environment.GetEnvironmentVariable("RabbitMQUsername"),
    rabbitmq_password = Environment.GetEnvironmentVariable("RabbitMQPassword"),
    rabbitmq_port = Environment.GetEnvironmentVariable("RabbitMQPort"),
    rabbitmq_virtual_host = Environment.GetEnvironmentVariable("RabbitMQVirtualHost"),
    rabbitmq_exchange = Environment.GetEnvironmentVariable("RabbitMQExchange"),
    aws_region = Environment.GetEnvironmentVariable("AWSRegion"),
    azure_connection_string = Environment.GetEnvironmentVariable("AzureConnectionString"),
    rabbitmq_routing_key = Environment.GetEnvironmentVariable("RabbitMQRoutingKey"),
    transaction_movement_topic = Environment.GetEnvironmentVariable("TransactionMovementTopic")
};

var messaging_provider = MessageFactory.Create(settings);

//////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////

// Start recursive
await RecursiveGetMessages();

async Task RecursiveGetMessages()
{
    var message = await messaging_provider.GetNextMessage();

    if (message != null)
    {
        var message_object = JsonConvert.DeserializeObject<MessageObject>(message);

        try
        {
            if (message_object.object_type == "TransactionCreateCommand")
            {
                var createCommandModel = JsonConvert.DeserializeObject<TransactionCreateCommand>(message_object.body);

                var transaction = CommonDataHelper<Transaction>.FillCommonFields(new Transaction
                {
                    product_id = createCommandModel.product_id,
                    transaction_type = createCommandModel.transaction_type,
                    transaction_date = createCommandModel.transaction_date,
                    object_reference_id = createCommandModel.object_reference_id,
                    object_sub_reference_id = createCommandModel.object_sub_reference_id,
                    units_sold = createCommandModel.units_sold,
                    units_shipped = createCommandModel.units_shipped,
                    units_purchased = createCommandModel.units_purchased,
                    units_received = createCommandModel.units_received,
                    purchased_unit_cost = createCommandModel.purchased_unit_cost,
                    sold_unit_price = createCommandModel.sold_unit_price,
                }, 1);

                await _Context.Transactions.AddAsync(transaction);
                await _Context.SaveChangesAsync();

            }
            else if (message_object.object_type == "TransactionEditCommand")
            {
                var commandModel = JsonConvert.DeserializeObject<TransactionEditCommand>(message_object.body);

                var existingEntity = await _Context.Transactions.SingleOrDefaultAsync(m => m.id == commandModel.id);

                if (existingEntity != null)
                {

                    if (commandModel.product_id.HasValue && existingEntity.product_id != commandModel.product_id)
                        existingEntity.product_id = commandModel.product_id.Value;

                    if (commandModel.transaction_type.HasValue && existingEntity.transaction_type != commandModel.transaction_type)
                        existingEntity.transaction_type = commandModel.transaction_type.Value;

                    if (commandModel.transaction_date.HasValue && existingEntity.transaction_date != commandModel.transaction_date)
                        existingEntity.transaction_date = commandModel.transaction_date.Value;

                    if (commandModel.units_sold.HasValue && existingEntity.units_sold != commandModel.units_sold)
                        existingEntity.units_sold = commandModel.units_sold.Value;

                    if (commandModel.units_shipped.HasValue && existingEntity.units_shipped != commandModel.units_shipped)
                        existingEntity.units_shipped = commandModel.units_shipped.Value;

                    if (commandModel.units_purchased.HasValue && existingEntity.units_purchased != commandModel.units_purchased)
                        existingEntity.units_purchased = commandModel.units_purchased.Value;

                    if (commandModel.units_received.HasValue && existingEntity.units_received != commandModel.units_received)
                        existingEntity.units_received = commandModel.units_received.Value;

                    if (commandModel.purchased_unit_cost.HasValue && existingEntity.purchased_unit_cost != commandModel.purchased_unit_cost)
                        existingEntity.purchased_unit_cost = commandModel.purchased_unit_cost.Value;

                    if (commandModel.sold_unit_price.HasValue && existingEntity.sold_unit_price != commandModel.sold_unit_price)
                        existingEntity.sold_unit_price = commandModel.sold_unit_price.Value;


                    existingEntity = CommonDataHelper<Transaction>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


                    _Context.Transactions.Update(existingEntity);
                    await _Context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Could not find transaction with given id");
                }
            }

            await RecursiveGetMessages();
        }
        catch (Exception)
        {
            // Need to put the message back
            await messaging_provider.PublishAsync(message_object, settings.transaction_movement_topic);
        }
    }
}