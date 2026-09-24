using KosmosERP.BusinessLayer.Models.Module.Payment.Dto;
using KosmosERP.BusinessLayer.PaymentProviders.Models;
using KosmosERP.Database.Models;
using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Interfaces;

public interface IPaymentProvider
{
    Task<Response<PaymentCustomer>> CreateCustomer(Customer customer, Address billing_address);
    Task<Response<PaymentCustomer>> GetCustomer(string external_id);
    Task<Response<PaymentCustomer>> UpdateCustomer(Customer customer, Address billing_address);
    Task<Response<PaymentProviderTransactionCreateDto>> CreateTransaction(ARInvoiceHeader ar_invoice_header, Payment payment, Customer customer, OrderHeader order_header, Dictionary<string, string>? metadata = null);
    Task<Response<PaymentProviderTransactionGetDto>> GetTransaction(string external_id);
    Task<Response<PaymentTransaction>> ReverseTransaction(string external_id);
    Task<Response<SavedPaymentMethodsDto>> GetSavedPaymentMethods(string external_id);
    Task<Response<PaymentProviderInvoiceDto>> GetInvoice(string external_id);
    Task<Response<PaymentProviderInvoiceDto>> CreateInvoice(ARInvoiceHeader ar_invoice_header, List<ARInvoiceLine> lines, Customer customer);
    Task<Response<PaymentProviderInvoiceDto>> CompleteInvoice(string invoice_external_id, string payment_external_id);
    Task<Response<PaymentCardGetDto>> CreateCard(Customer customer);
}
