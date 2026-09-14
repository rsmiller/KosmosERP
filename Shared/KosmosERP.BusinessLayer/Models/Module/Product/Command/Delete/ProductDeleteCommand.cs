using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Product.Command.Delete;

public class ProductDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
