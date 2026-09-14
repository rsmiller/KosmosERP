
namespace KosmosERP.Database.Views;

public class vw_OrdersReadyForScheduling
{
    public int order_number { get; set; }

    public string customer_name { get; set; }

    public int peices { get; set; }

    public string category { get; set; }
}

