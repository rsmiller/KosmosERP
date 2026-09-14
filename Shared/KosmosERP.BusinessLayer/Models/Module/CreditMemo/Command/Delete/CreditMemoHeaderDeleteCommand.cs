using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.CreditMemo.Command.Delete;

public class CreditMemoHeaderDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
} 