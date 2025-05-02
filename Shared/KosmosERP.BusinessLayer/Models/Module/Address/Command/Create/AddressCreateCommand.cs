using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Address.Command.Create;

public class AddressCreateCommand : DataCommand
{
    [Required]
    public required string street_address1 { get; set; }

    public string? street_address2 { get; set; }

    [Required]
    public required string city { get; set; }

    [Required]
    public required string state { get; set; }

    [Required]
    public required string postal_code { get; set; }

    [Required]
    public required string country { get; set; }
}
