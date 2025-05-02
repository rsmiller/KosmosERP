using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.State.Command.Edit;

public class StateEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public int? country_id { get; set; }
    public string? state_name { get; set; }
    public string? iso2 { get; set; }
}
