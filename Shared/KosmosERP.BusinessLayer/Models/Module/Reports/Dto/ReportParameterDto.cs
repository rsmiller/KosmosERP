
namespace KosmosERP.BusinessLayer.Models.Module.Reports.Dto
{
    /// <summary>
    /// A single tunable input for a report (mapped to a query-string parameter on the
    /// report endpoint). The UI renders an input control based on <see cref="Type"/> and
    /// only sends values the user actually fills in.
    /// </summary>
    public sealed class ReportParameterDto
    {
        /// <summary>Query-string key expected by the endpoint (e.g. "as_of_date").</summary>
        public required string Name { get; init; }

        /// <summary>Human-readable label for the input control.</summary>
        public required string Label { get; init; }

        /// <summary>Control hint: "date", "int", "string", or "select".</summary>
        public required string Type { get; init; }

        /// <summary>Whether the endpoint needs this value (most general reports default server-side).</summary>
        public bool Required { get; init; }

        /// <summary>
        /// When set, the parameter is a dropdown: the UI fetches its options from this endpoint
        /// (path relative to the API root, no leading slash) instead of showing a free-text input.
        /// Used together with <see cref="Type"/> = "select".
        /// </summary>
        public string? OptionsEndpoint { get; init; }
    }
}
