using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Transaction.Command.Delete;

public class TransactionDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
