namespace KosmosERP.Models;

public static class ModuleIdentifiers
{
    public static Guid CustomerModule { get { return Guid.Parse("ecf469ca-0a18-459b-954d-47de0bf23cf6"); } }
}


public enum SortOrder
{
    /// <summary>
    ///   The items are not sorted.
    /// </summary>
    None = 0,

    /// <summary>
    ///   The items are sorted in ascending order.
    /// </summary>
    Ascending = 1,

    /// <summary>
    ///   The items are sorted in descending order.
    /// </summary>
    Descending = 2,

}

public enum BroadcastType
{
    Create = 0,
    Update = 1,
    Delete = 2
}

public enum ResultCode
{
    None = 0,
    Okay = 1,
    Invalid = -1,
    NotFound = -2,
    NullItemInput = -3,
    Error = -5,
    DataValidationError = -6,
    AlreadyExists = -7,
    InvalidPermission = -8
}

public class CustomerAddressType
{
    public static int Physical { get { return 1; } }
    public static int ShipTo { get { return 2; } }
}


public class TransactionType
{
    public static int Inbound { get { return 1; } }
    public static int Outbound { get { return 2; } }
    public static int Planned { get { return 3; } }
    public static int Commited { get { return 5; } }
    public static int Reserved { get { return 6; } }
    public static int Move { get { return 7; } }
    public static int Adjustment { get { return 8; } }
}

public class StorageType
{
    public static string Local { get { return "local"; } }
    public static string Azure { get { return "azure"; } }
    public static string AWS { get { return "aws"; } }
    public static string Google { get { return "google"; } }
    public static string MOCK { get { return "mock"; } }
}

public class MessagePublisherType
{
    public static string Database { get { return "database"; } }
    public static string RabbitMq { get { return "rabbit"; } }
    public static string Azure { get { return "azure"; } }
    public static string AWS { get { return "aws"; } }
    public static string Google { get { return "google"; } }
    public static string MOCK { get { return "mock"; } }
}

public class PaymentProviderType
{
    public static string Square { get { return "square"; } }
    public static string Stripe { get { return "stripe"; } }
    public static string Paypal { get { return "paypal"; } }
    public static string MOCK { get { return "mock"; } }
}

public class LogProviderType
{
    public static string Azure { get { return "azure"; } }
    public static string DataDog { get { return "datadog"; } }
    public static string Database { get { return "database"; } }
    public static string MOCK { get { return "mock"; } }
}

public class RequiredMessageTopics
{
    public static string TransactionMovementTopic { get { return "transaction_movement_topic"; } }
}

public class DatabaseStartNumbers
{
    public static int ARInvoices { get { return 12000; } }
    public static int APInvoices { get { return 21000; } }
    public static int Customers { get { return 20000; } }
    public static int Shipments { get { return 43000; } }
    public static int Orders { get { return 31000; } }
    public static int Payments { get { return 210000; } }
    public static int Subscriptions { get { return 70000; } }
    public static int CreditMemos { get { return 30000; } }
}

/// <summary>
/// Production order status keys, stored on production order headers and lines.
/// Their display names live in key_value_stores under KeyValueIds.ProductionStatuses,
/// created by ProductionOrderModule.SeedPermissions. The older production_status_*
/// keys (module f157469e-...) were retired; see migration RetireOldProductionStatuses.
/// </summary>
public class ProductionOrderStatus
{
    public static string Submitted { get { return "production_order_status_submitted"; } }
    public static string PartsPulled { get { return "production_order_status_pulled"; } }
    public static string WorkInProgress { get { return "production_order_status_wip"; } }
    public static string QualityCheck { get { return "production_order_status_qc"; } }
    public static string Complete { get { return "production_order_status_complete"; } }
    public static string ReadyToShip { get { return "production_order_status_ready_to_ship"; } }
    public static string Canceled { get { return "production_order_status_canceled"; } }
}


public class AuthenticiationProviders
{
    public static string Database { get { return "database"; } }
    public static string Keycloak { get { return "keycloak"; } }
    public static string SAML { get { return "saml"; } }
    public static string MOCK { get { return "mock"; } }
}

public class AccountType
{
    public static int Asset { get { return 1; } }
    public static int Liability { get { return 2; } }
    public static int Equity { get { return 3; } }
    public static int Revenue { get { return 4; } }
    public static int Expense { get { return 5; } }
}

public class NormalBalance
{
    public static int Debit { get { return 1; } }
    public static int Credit { get { return 2; } }
}

public class JournalReferenceType
{
    public static int Manual { get { return 1; } }
    public static int APInvoice { get { return 2; } }
    public static int ARInvoice { get { return 3; } }
    public static int CreditMemo { get { return 4; } }
    public static int Payment { get { return 5; } }
}

public class FinancialTransactionType
{
    public static int JournalEntry { get { return 1; } }
    public static int APPost { get { return 2; } }
    public static int ARPost { get { return 3; } }
    public static int PaymentReceived { get { return 4; } }
    public static int PaymentSent { get { return 5; } }
    public static int CreditMemoApplied { get { return 6; } }
}