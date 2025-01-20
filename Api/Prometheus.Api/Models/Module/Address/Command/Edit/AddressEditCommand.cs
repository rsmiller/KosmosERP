using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Address.Command.Edit;

public class AddressEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public string? street_address1 { get; set; }
    public string? street_address2 { get; set; }
    public string? city { get; set; }
    public string? state { get; set; }
    public string? postal_code { get; set; }
    public string? country { get; set; }
}
