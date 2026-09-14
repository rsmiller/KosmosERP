using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Edit;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public interface IBillingPaymentEntryJob
{
    Task Run();
}

public class BillingPaymentEntryJob : IBillingPaymentEntryJob
{
    private IBaseERPContext _Context;
    private IOrderModule _OrderModule;
    
    
    private readonly ILogger<BillingPaymentEntryJob> _logger;

    public BillingPaymentEntryJob(ILogger<BillingPaymentEntryJob> logger, IOrderModule order_module, IBaseERPContext context)
    {
        _logger = logger;

        _Context = context;
        _OrderModule = order_module;

    }

    public async Task Run()
    {
        _logger.LogInformation("Running...");

        await WorkSubscriptions();
        await WorkEntries();
    }
    
    private async Task WorkSubscriptions()
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var subscriptions = await _Context.Subscriptions.Where(m => m.is_deleted == false && m.next_date == today).ToListAsync();

            foreach(var subscription in subscriptions)
            {
                var entry = CommonDataHelper<SubscriptionEntry>.FillCommonFields(new SubscriptionEntry()
                {
                    billing_subscription_id = subscription.id,
                }, 1);

                await _Context.SubscriptionEntries.AddAsync(entry);
                await _Context.SaveChangesAsync();



                var updated_sub = CommonDataHelper<Subscription>.FillUpdateFields(subscription, 1);
                updated_sub.next_date = today.AddDays(subscription.cycle_days);

                _Context.Subscriptions.Update(updated_sub);
                await _Context.SaveChangesAsync();
            }
        }
        catch(Exception e)
        {
           _logger.LogError("BillingPaymentEntryJob:WorkSubscriptions general error; {1}", e.Message); 
        }
    }

    private async Task WorkEntries()
    {
        try
        {
            var entries = await (from bse in _Context.SubscriptionEntries
                             join bs in _Context.Subscriptions on bse.billing_subscription_id equals bs.id
                             where bse.is_deleted == false 
                             && bs.is_deleted == false 
                             && bse.order_header_id == null
                             select new { entry = bse, sub = bs
                             }).ToListAsync();

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;

                var order_response = await _OrderModule.DuplicateOrder(entry.sub.order_header_id, entry.sub.customer_id, "1");

                if (order_response.Success)
                {
                    // Make it released
                    await _OrderModule.Edit(new OrderHeaderEditCommand()
                    {
                        id = order_response.Data.id,
                        order_type = "R",
                        calling_user_id = "1"
                    });

                    SubscriptionEntry update_entry = entry.entry;

                    update_entry.order_header_id = order_response.Data.id;

                    CommonDataHelper<SubscriptionEntry>.FillUpdateFields(update_entry, 1);

                    _Context.SubscriptionEntries.Update(update_entry);
                    await _Context.SaveChangesAsync();

                    // Attempt to automate the invoice

                }
                else
                {
                    _logger.LogError("BillingPaymentEntryJob error creating order. Subscription {1}", entry.sub.subscription_number);
                }
            }
        }
        catch(Exception e)
        {
           _logger.LogError("BillingPaymentEntryJob:WorkEntries general error; {1}", e.Message); 
        }
    }
}