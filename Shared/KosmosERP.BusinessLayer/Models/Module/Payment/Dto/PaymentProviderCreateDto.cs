namespace KosmosERP.BusinessLayer.Models.Module.Payment.Dto;

public class PaymentProviderTransactionCreateDto
{
    public string clientSecret {get; set;} = "";
    public string id {get; set;} = "";
    public decimal amount {get; set;} = 0.00m;
    public string ar_invoice_number {get; set;} = "";
}