using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Edit;

public class ProductionOrderHeaderEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }
    public int? status_id { get; set; } = 0;
    public int? priority_id { get; set; } = 99;
    public DateOnly? planned_start_date { get; set; }
    public DateOnly? planned_complete_date { get; set; }
    public DateOnly? actual_completed_on { get; set; }
    public bool? is_complete { get; set; }
}
