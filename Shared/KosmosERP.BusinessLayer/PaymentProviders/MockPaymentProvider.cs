
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.Payment.Dto;
using KosmosERP.BusinessLayer.PaymentProviders.Models;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.PaymentProviders;

public class MockPaymentProvider : IPaymentProvider
{
    private IPaymentProviderSettings _Settings;
 
    public MockPaymentProvider(IPaymentProviderSettings settings)
    {
        _Settings = settings;
    }

    public async Task<Response<PaymentCustomer>> GetCustomer(string external_id)
    {
        PaymentCustomer model = new PaymentCustomer()
        {
            identifier = "123"
        };

        return new Response<PaymentCustomer>(model);
    }

    public async Task<Response<PaymentCustomer>> CreateCustomer(Customer customer, Address billing_address)
    {
        PaymentCustomer model = new PaymentCustomer()
        {
            identifier = "123"
        };

        return new Response<PaymentCustomer>(model);
    }

    public Task<Response<PaymentCustomer>> UpdateCustomer(Customer customer, Address billing_address)
    {
        throw new NotImplementedException();
    }
    
    public async Task<Response<PaymentProviderTransactionCreateDto>> CreateTransaction(ARInvoiceHeader ar_invoice_header, Payment payment, Customer customer, OrderHeader order_header, Dictionary<string, string>? metadata = null)
    {
        PaymentProviderTransactionCreateDto model = new PaymentProviderTransactionCreateDto()
        {
            clientSecret = "123"
        };

        return new Response<PaymentProviderTransactionCreateDto>(model);
    }

    public async Task<Response<PaymentProviderTransactionGetDto>> GetTransaction(string external_id)
    {
        PaymentProviderTransactionGetDto model = new PaymentProviderTransactionGetDto()
        {
            charge_id = "123"
        };

        return new Response<PaymentProviderTransactionGetDto>(model);
    }

    public async Task<Response<PaymentTransaction>> ReverseTransaction(string external_id)
    {
        PaymentTransaction model = new PaymentTransaction()
        {
            identifier = "123"
        };

        return new Response<PaymentTransaction>(model);
    }

    public async Task<Response<SavedPaymentMethodsDto>> GetSavedPaymentMethods(string external_id)
    {
        SavedPaymentMethodsDto model = new SavedPaymentMethodsDto()
        {
            banks = new List<SavedBankDto>()
            {
                new SavedBankDto()
                {
                    id = "bank_123",
                    bank = "Mock Bank",
                    last4 = "6789"
                }
            }
        };

        return new Response<SavedPaymentMethodsDto>(model);
    }

    public async Task<Response<PaymentProviderInvoiceDto>> CreateInvoice(ARInvoiceHeader ar_invoice_header, List<ARInvoiceLine> lines, Customer customer)
    {
        return new Response<PaymentProviderInvoiceDto>()
        {
            Data = new PaymentProviderInvoiceDto()
            {
                id = "inv_123",
                invoice_url = "https://mockpaymentprovider.com/invoice/inv_123"
            }
        };
    }

    public async Task<Response<PaymentProviderInvoiceDto>> GetInvoice(string external_id)
    {
        return new Response<PaymentProviderInvoiceDto>()
        {
            Data = new PaymentProviderInvoiceDto()
            {
                id = "inv_123",
                invoice_url = "https://mockpaymentprovider.com/invoice/inv_123"
            }
        };
    }

    public async Task<Response<PaymentProviderInvoiceDto>> CompleteInvoice(string invoice_external_id, string payment_external_id)
    {
        return new Response<PaymentProviderInvoiceDto>()
        {
            Data = new PaymentProviderInvoiceDto()
            {
                id = "inv_123",
                invoice_url = "https://mockpaymentprovider.com/invoice/inv_123"
            }
        };
    }

    public async Task<Response<PaymentCardGetDto>> CreateCard(Customer customer)
    {
        PaymentCardGetDto model = new PaymentCardGetDto()
        {
            identifier = "card_123",
            success = true
        };

        return new Response<PaymentCardGetDto>(model);
    }
}