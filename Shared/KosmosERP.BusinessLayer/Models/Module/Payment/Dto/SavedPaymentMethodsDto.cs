namespace KosmosERP.BusinessLayer.Models.Module.Payment.Dto;

public class SavedPaymentMethodsDto
{
    public List<SavedBankDto> banks { get; set; } = new List<SavedBankDto>();
    public List<SavedCreditCardDto> credit_cards { get; set; } = new List<SavedCreditCardDto>();
    
}