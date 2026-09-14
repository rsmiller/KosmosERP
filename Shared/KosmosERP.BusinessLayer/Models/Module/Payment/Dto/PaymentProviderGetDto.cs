namespace KosmosERP.BusinessLayer.Models.Module.Payment.Dto;

public class PaymentProviderTransactionGetDto
{
    public string status { get; set; } = "";
    public string payment_id { get; set; } = "";
    public string charge_id { get; set; } = "";
    public string auth_code { get; set; } = "";
    public string receipt_number { get; set; } = "";
    public long amount_received { get; set; } = 0;
    public string transaction_method { get; set; } = "";
    public string confirmation_code { get; set; } = "";
    public string payment_receipt { get; set; } = "";
    public DateTime payment_date { get; set;  }
}