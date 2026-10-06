namespace KosmosERP.BusinessLayer.Models;

/// <summary>
/// Valid values for a sales order's <c>order_type</c> and a purchase order's <c>po_type</c>.
/// These match the UI's type dropdown (components/header-type-selector.tsx). A released
/// header is locked for editing.
/// </summary>
public static class HeaderTypes
{
    public const string Quote = "Q";
    public const string Release = "R";

    /// <summary>Validation pattern for the type fields (null/empty is left to [Required]).</summary>
    public const string Pattern = "^[QR]$";

    public const string ErrorMessage = "Type must be Q (Quote) or R (Release).";
}
