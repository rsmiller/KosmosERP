using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.Lead.Dto;

public class LeadListDto
{
    public int id { get; set; }

    public bool is_deleted { get; set; } = false;

    public DateTime created_on { get; set; }

    public int created_by { get; set; }

    public DateTime? updated_on { get; set; }

    public int? updated_by { get; set; }

    public DateTime? deleted_on { get; set; }

    public int? deleted_by { get; set; }

    public string first_name { get; set; }

    public string last_name { get; set; }

    public string? title { get; set; }

    public string? email { get; set; }

    public string? phone { get; set; }

    public string? cell_phone { get; set; }

    public string company_name { get; set; }

    public string lead_stage { get; set; }

    public string? time_zone { get; set; }
    public string? address_line1 { get; set; }
    public string? address_line2 { get; set; }
    public string? city { get; set; }
    public string? state { get; set; }
    public string? zip { get; set; }
    public string? country { get; set; }

    public bool is_converted { get; set; } = false;

    public int? converted_customer_id { get; set; }
    public int? converted_contact_id { get; set; }

    public int owner_id { get; set; }

    public string guid { get; set; }
}
