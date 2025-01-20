using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Prometheus.Database.Models;

public class ProductAttribute : BaseDatabaseModel
{
    [Required]
    public int product_id { get; set; }

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

    // TODO: Default these attributes

    //public string size { get; set; }
    //public decimal decimal_size { get; set; } = 0;
    //public decimal height { get; set; } = 0;
    //public string height_metric { get; set; } = "in";
    //public decimal weight { get; set; } = 0;
    //public string weight_metric { get; set; } = "in";
    //public string unit_of_measure { get; set; }
    //public string coating { get; set; }
    //public string grade { get; set; }
    //public string color { get; set; }
    //public string brand { get; set; }
    //public string year { get; set; }
}
