namespace KosmosERP.BusinessLayer.Models.Module.Payment.Dto;

public class SavedCreditCardDto
{
    public string id { get; set; } = "";
    public string brand { get; set; } = "";
    public string last4 { get; set; } = "";
    public string exp_month { get; set; } = "";
    public string exp_year { get; set; } = "";
}