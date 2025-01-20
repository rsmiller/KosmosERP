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