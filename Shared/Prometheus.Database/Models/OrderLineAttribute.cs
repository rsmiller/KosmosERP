using System;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Database.Models;

public class OrderLineAttribute : BaseDatabaseModel
{
    [Required]
    public int order_line_id { get; set; }

    [Required]
    [MaxLength(50)]
    public string attribute_name { get; set; }

    [Required]
    [MaxLength(50)]
    public string attribute_value { get; set; }

    [MaxLength(50)]
    public string? attribute_value2 { get; set; }

    [MaxLength(50)]
    public string? attribute_value3 { get; set; }
}
