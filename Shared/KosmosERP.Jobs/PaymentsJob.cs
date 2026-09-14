using KosmosERP.BusinessLayer;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Create;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public interface IPaymentsJob
{
    Task Run();
}

public class PaymentsJob : IPaymentsJob
{
    private IBaseERPContext _Context;
    private IPaymentModule _PaymentModule;
    private readonly IPaymentProvider _PaymentProvider;
    private readonly ILogger<ITransactionJob> _logger;

    public PaymentsJob(ILogger<ITransactionJob> logger, 
                IBaseERPContext context,
                IPaymentModule paymentModule,
                IPaymentProviderFactory paymentFactory)
    {
        _logger = logger;
        _Context = context;
        _PaymentModule = paymentModule;
        _PaymentProvider = paymentFactory.GetProvider();
    }

    public async Task Run()
    {
        var invoiced_not_paid = await _Context.ARInvoiceHeaders.Where(m => !m.is_paid && m.payment_external_id != null && m.payment_external_id != "").ToListAsync();
        
        foreach(var invoice in invoiced_not_paid)
        {
            try
            {
                // Get the Stripe invoice status
                var stripeInvoiceResponse = await _PaymentProvider.GetInvoice(invoice.payment_external_id);

                if (stripeInvoiceResponse.Success && stripeInvoiceResponse.Data != null)
                {
                    // Check if the payment status is paid (succeeded, processing, or paid)
                    if (stripeInvoiceResponse.Data.status == "succeeded" || 
                        stripeInvoiceResponse.Data.status == "processing" ||
                        stripeInvoiceResponse.Data.status == "paid")
                    {
                        // Update the invoice as paid
                        invoice.is_paid = true;
                        invoice.paid_on = DateOnly.FromDateTime(DateTime.UtcNow);
                        
                        _Context.ARInvoiceHeaders.Update(invoice);
                        await _Context.SaveChangesAsync();

                        foreach(var payment in stripeInvoiceResponse.Data.transactions)
                        {
                            // Ensure there isn't already a payment recorded for this transaction
                            //  because sometimes multiple jobs could run at the same time
                            var existing_payment = await _Context.Payments
                                .Where(m => m.transaction_id == payment.payment_id).FirstOrDefaultAsync();

                            if (existing_payment == null)
                            {
                                var payment_response = await _PaymentModule.CreateInternal(new PaymentCreateCommand()
                                {
                                    ar_invoice_header_id = invoice.id,
                                    order_header_id = invoice.order_header_id,
                                    calling_user_id = "1",
                                    payment_amount = payment.amount_received,
                                    transaction_method = payment.transaction_method,
                                    transaction_status = "succeeded",
                                    transaction_id = payment.payment_id,
                                    confirmation_code = payment.confirmation_code,
                                    payment_processor = "stripe",
                                    payment_receipt = payment.payment_receipt,
                                });
                            }
                            
                        }

                        _logger.LogInformation($"Invoice {invoice.invoice_number} marked as paid. External ID: {invoice.payment_external_id}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Failed to retrieve Stripe invoice status for {invoice.invoice_number}. Error: {stripeInvoiceResponse.Exception}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing invoice {invoice.invoice_number} (ID: {invoice.payment_external_id})");
            }
        }
    }

}