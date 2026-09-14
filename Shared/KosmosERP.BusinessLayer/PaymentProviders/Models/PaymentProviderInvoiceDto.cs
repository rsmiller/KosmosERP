
using KosmosERP.BusinessLayer.Models.Module.Payment.Dto;

namespace KosmosERP.BusinessLayer.PaymentProviders.Models;

public class PaymentProviderInvoiceDto
{
    public string id { get; set;}
    public string invoice_number { get; set;}
    public string invoice_url { get; set;}
    public decimal amount_due { get; set;}
    public string status { get; set;}

    public List<PaymentProviderTransactionGetDto> transactions { get; set;} = new List<PaymentProviderTransactionGetDto>();
}