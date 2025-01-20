using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Api.Models.Module.Transaction.Command.Delete;

public class TransactionDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
}
