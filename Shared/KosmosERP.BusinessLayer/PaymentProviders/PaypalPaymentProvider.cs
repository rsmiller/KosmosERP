
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.Payment.Dto;
using KosmosERP.BusinessLayer.PaymentProviders.Models;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.PaymentProviders;

public class PaypalPaymentProvider : IPaymentProvider
{
    private IPaymentProviderSettings _Settings;
 
    public PaypalPaymentProvider(IPaymentProviderSettings settings)
    {
        _Settings = settings;
    }

    public Task<Response<PaymentCustomer>> GetCustomer(string external_id)
    {
        throw new NotImplementedException();
    }

    public Task<Response<PaymentCustomer>> CreateCustomer(Customer customer, Address billing_address)
    {
        throw new NotImplementedException();
    }

    public Task<Response<PaymentCustomer>> UpdateCustomer(Customer customer, Address billing_address)
    {
        throw new NotImplementedException();
    }
    
    public Task<Response<PaymentProviderTransactionCreateDto>> CreateTransaction(ARInvoiceHeader ar_invoice_header, Payment payment, Customer customer, OrderHeader order_header, Dictionary<string, string>? metadata = null)
    {
        throw new NotImplementedException();
    }

    public Task<Response<PaymentProviderTransactionGetDto>> GetTransaction(string external_id)
    {
        throw new NotImplementedException();
    }

    public Task<Response<PaymentTransaction>> ReverseTransaction(string external_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<SavedPaymentMethodsDto>> GetSavedPaymentMethods(string external_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<PaymentProviderInvoiceDto>> CreateInvoice(ARInvoiceHeader ar_invoice_header, List<ARInvoiceLine> lines, Customer customer)
    {
        throw new NotImplementedException();
    }

    public Task<Response<PaymentProviderInvoiceDto>> GetInvoice(string external_id)
    {
        throw new NotImplementedException();
    }
    
    public async Task<Response<PaymentProviderInvoiceDto>> CompleteInvoice(string invoice_external_id, string payment_external_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<PaymentCardGetDto>> CreateCard(Customer customer)
    {
        throw new NotImplementedException();
    }
}