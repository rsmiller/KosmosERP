
namespace KosmosERP.BusinessLayer.Models.Module.APInvoice.Dto;

public class APInvoiceAssociationDto
{
    public int id { get; set; }
    public string identifier { get; set; }
    public bool is_purchase_order { get; set; } = false;

    public bool is_sales_order { get; set; } = false;

    public bool is_ar_invoice { get; set; } = false;

    public Dictionary<string, string> additional_data { get; set; } = new Dictionary<string, string>();
}
