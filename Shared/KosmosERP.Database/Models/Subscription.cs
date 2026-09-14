using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KosmosERP.Database.Models;

public class Subscription : BaseDatabaseModel
{
    [Required]
    public int customer_id { get; set; }
    [Required]
    public int order_header_id { get; set; }
    [Required]
    public int subscription_number { get; set; }
    [Required]
    public int quantity { get; set; }
    [Precision(14, 3)]
    [Required]
    public decimal price { get; set; }
    [Precision(14, 3)]
    [Required]
    public decimal tax { get; set; } = 0;
    [Required]
    public int cycle_days { get; set; }
    [Required]
    public DateOnly start_date { get; set; }
    [Required]
    public DateOnly next_date { get; set; }
    public DateOnly? end_date { get; set; }
    
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public Customer customer { get; set; }

    [NotMapped]
    public OrderHeader order { get; set; }
}
