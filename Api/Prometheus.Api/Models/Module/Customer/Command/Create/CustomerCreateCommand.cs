using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Customer.Command.Create;

public class CustomerCreateCommand : DataCommand
{
    [Required]
    public string customer_name { get; set; }
    public string? customer_description { get; set; }

    [Required]
    public string phone { get; set; }
    public string? fax { get; set; }
    public string? general_email { get; set; }
    public string? website { get; set; }
    public string category { get; set; }
}
