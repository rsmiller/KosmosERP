using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class CustomerAddress : BaseDatabaseModel
{
    public int customer_id { get; set; }
    public int address_id { get; set; }
    public int address_type_id { get; set; }

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<Address> addresses { get; set; } = new List<Address>();
}
