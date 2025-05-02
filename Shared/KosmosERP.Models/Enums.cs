namespace KosmosERP.Models;

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
    public static string RabbitMq { get { return "rabbit"; } }
    public static string Azure { get { return "azure"; } }
    public static string AWS { get { return "aws"; } }
    public static string Google { get { return "google"; } }
    public static string MOCK { get { return "mock"; } }
}

public class RequiredMessageTopics
{
    public static string TransactionMovementTopic { get { return "transaction_movement_topic"; } }
}