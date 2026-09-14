using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Views;

public class vw_OrdersReadyForInvoicing
{
    [Key]
    public int order_number { get; set; }

    public DateTime order_date { get; set; }

    public string pay_method_name { get; set; }

    public string pay_method { get; set; }

    public int created_by { get; set; }

    public string customer_name { get; set; }

    public string customer_guid { get; set; }

    public string order_guid { get; set; }
}

