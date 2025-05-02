using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;
namespace KosmosERP.BusinessLayer.Models.Module.Product.Command.Delete;

public class ProductAttributeDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
