using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Country.Command.Delete;

public class CountryDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
