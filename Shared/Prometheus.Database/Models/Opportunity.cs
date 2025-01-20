using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;
public class Opportunity : BaseDatabaseModel
{
    [Required]
    [MaxLength(200)]
    public string opportunity_name { get; set; }

    [Required]
    public int customer_id { get; set; }

    [Required]
    public int contact_id { get; set; }

    [Required]
    [Precision(16,2)]
    public decimal amount { get; set; }

    [Required]
    public string stage { get; set; }

    [Required]
    [Precision(3)]
    public int win_chance { get; set; }

    [Required]
    public DateOnly expected_close { get; set; }

    [Required]
    public int owner_id { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public Contact contact { get; set; }

    [NotMapped]
    public Customer customer { get; set; }

    [NotMapped]
    public List<OpportunityLine> opportunity_lines { get; set; } = new List<OpportunityLine>();
}
