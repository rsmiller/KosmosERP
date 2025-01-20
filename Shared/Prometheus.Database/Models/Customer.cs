using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prometheus.Database.Models;

public class Customer : BaseDatabaseModel
{
    [Required]
    public int customer_number { get; set; }

    [Required]
    [MaxLength(150)]
    public string customer_name { get; set; }

    [MaxLength(1000)]
    public string? customer_description { get; set; }

    [Required]
    [MaxLength(25)]
    public string phone { get; set; }

    [MaxLength(25)]
    public string? fax { get; set; }

    [MaxLength(75)]
    public string? general_email { get; set; }

    [MaxLength(250)]
    public string? website { get; set; }

    [MaxLength(50)]
    [Required]
    public string category { get; set; }

    [Required]
    [MaxLength(50)]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public List<Address> addresses { get; set; } = new List<Address>();
}
