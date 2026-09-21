namespace KosmosERP.Reporting;

/// <summary>
/// The outcome of a <see cref="IReportService.GenerateAsync"/> call. On success,
/// <see cref="Content"/> holds the rendered bytes and <see cref="ContentType"/>/<see cref="FileName"/>
/// are set for an HTTP file response. On failure, <see cref="Error"/> explains why.
/// </summary>
public class ReportResult
{
    public bool Success { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>MIME type: application/pdf or text/html.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Suggested download file name, including extension.</summary>
    public string FileName { get; set; } = string.Empty;

    public string? Error { get; set; }

    public static ReportResult Failed(string error) => new() { Success = false, Error = error };
}
