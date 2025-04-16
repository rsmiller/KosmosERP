using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Customer.Command.Edit;

public class CustomerEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public string? customer_name { get; set; }
    public string? customer_description { get; set; }
    public string? phone { get; set; }
    public string? fax { get; set; }
    public string? general_email { get; set; }
    public string? website { get; set; }
    public string? category { get; set; }
    public bool? is_taxable { get; set; }
    public decimal? tax_rate { get; set; }
}
