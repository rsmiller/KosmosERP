using Prometheus.Models;

namespace Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Dto;

public class APInvoiceHeaderListDto : BaseDto
{
    public int vendor_id { get; set; }

    public string invoice_number { get; set; }

    public DateTime invoice_date { get; set; }
    public DateTime invoice_due_date { get; set; }

    public DateTime invoice_received_date { get; set; }

    public decimal invoice_total { get; set; }

    public string? memo { get; set; }

    public bool packing_list_is_required { get; set; } = false;

    public bool is_paid { get; set; } = false;

    public string guid { get; set; }

    public string? vendor_name { get; set; }
}
