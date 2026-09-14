using Google.Protobuf;
using KosmosERP.BusinessLayer;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public interface ITransactionJob
{
    Task Run();
}

public class TransactionJob : ITransactionJob
{
    private IBaseERPContext _Context;
    private ITransactionModule _TransactionModule;
    private IMessagePublisher _MessagePublisher;
    private IMessagePublisherSettings _MessagePublisherSettings;
    
    private readonly ILogger<ITransactionJob> _logger;

    public TransactionJob(ILogger<ITransactionJob> logger, 
                ITransactionModule transaction_module,
                IMessageFactory message_factory,    
                IBaseERPContext context,
                IMessagePublisherSettings messageSettings)
    {
        _logger = logger;
        _MessagePublisher = message_factory.GetPublisher();
        _Context = context;
        _TransactionModule = transaction_module;
        _MessagePublisherSettings = messageSettings;
    }

    private async Task ProcessMessages()
    {
        var message = await _MessagePublisher.GetNextMessage(_MessagePublisherSettings.transaction_movement_topic);
        if(message == null)
        {
            _logger.LogInformation("No messages to process.");
            return;
        }
        else
        {
            _logger.LogInformation("Processing message: {message}", message);

            MessageObject? entry = null;

            try
            {
                entry = JsonConvert.DeserializeObject<MessageObject>(message);

                if(entry == null)
                {
                    _logger.LogError("Failed to deserialize message: {message}", message);
                    return;
                }
                
                if (entry.object_type == "TransactionCreateCommand")
                {
                    var createCommandModel = JsonConvert.DeserializeObject<TransactionCreateCommand>(entry.body);

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

                if (entry.object_type == "TransactionEditCommand")
                {
                    var createCommandModel = JsonConvert.DeserializeObject<TransactionCreateCommand>(entry.body);

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

                if (entry.object_type == "TransactionDeleteCommand")
                {
                    var deleteCommandModel = JsonConvert.DeserializeObject<TransactionDeleteCommand>(entry.body);

                    if(!deleteCommandModel.object_sub_reference_id.HasValue)
                    {
                        var transactions = await _Context.Transactions
                            .Where(t => t.object_reference_id == deleteCommandModel.object_reference_id)
                            .ToListAsync();

                        foreach (var trans in transactions)
                        {
                            trans.transaction_type = TransactionType.Adjustment;
                            trans.is_deleted = true;

                            var transaction = CommonDataHelper<Transaction>.FillDeleteFields(trans, 1);

                            _Context.Transactions.Update(transaction);
                            await _Context.SaveChangesAsync();  
                        }
                    }
                    else
                    {
                        var trans_line = await _Context.Transactions
                            .FirstOrDefaultAsync(t => t.object_reference_id == deleteCommandModel.object_reference_id
                                && t.object_sub_reference_id == deleteCommandModel.object_sub_reference_id);

                        if(trans_line != null)
                        {
                            trans_line.transaction_type = TransactionType.Adjustment;
                            trans_line.is_deleted = true;

                            var transaction = CommonDataHelper<Transaction>.FillDeleteFields(trans_line, 1);

                            _Context.Transactions.Update(transaction);
                            await _Context.SaveChangesAsync();  
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {message}", message);


                if(entry != null)
                {
                    await _MessagePublisher.PublishAsync(entry, _MessagePublisherSettings.transaction_movement_topic);
                }   
            }

            await ProcessMessages();
        }
    }

    public async Task Run()
    {
        _logger.LogInformation("Running...");

        await ProcessMessages();
    }
}