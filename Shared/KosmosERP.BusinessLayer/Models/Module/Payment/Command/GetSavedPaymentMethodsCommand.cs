using KosmosERP.Models;

public class GetSavedPaymentMethodsCommand : DataCommand
{
    public int? customer_id { get; set; }
    public string? ar_invoice_header_guid { get; set; }
}