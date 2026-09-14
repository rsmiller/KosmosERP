using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Settings.Command.Create;

public class SettingsCreateCommand : DataCommand
{
    [Required]
    public string company_name { get; set; }
    public string company_address1 { get; set; }
    public string company_address2 { get; set; }
    public string company_city { get; set; }
    public string company_state { get; set; }
    public string company_zip { get; set; }
    public string company_country { get; set; }
    public string company_phone { get; set; }
    public string company_ar_email { get; set; }
    public string company_ap_email { get; set; }
    public string company_general_email { get; set; }
    public string company_website { get; set; }
    public string tax_id { get; set; }
    public string fiscal_year_start { get; set; }
}