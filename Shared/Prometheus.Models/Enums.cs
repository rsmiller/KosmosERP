namespace Prometheus.Models;

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

public class TransactionType
{
    public static int Input { get { return 1; } }
    public static int Output { get { return 2; } }
    public static int Move { get { return 3; } }
    public static int Adjustment { get { return 3; } }
}

public class StorageType
{
    public static string Local { get { return "local"; } }
    public static string Azure { get { return "azure"; } }
    public static string AWS { get { return "aws"; } }
    public static string Google { get { return "google"; } }
}
