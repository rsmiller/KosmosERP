using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;

public class TransactionDeleteCommand : DataCommand
{
    public int? id { get; set; }

    public int? object_reference_id { get; set; }
    public int? object_sub_reference_id { get; set; }
}
