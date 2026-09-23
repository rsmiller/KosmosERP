
namespace KosmosERP.BusinessLayer.Models.Module.Reports.Dto
{
    /// <summary>
    /// One selectable option for a dropdown report parameter. <see cref="Value"/> is what the
    /// report endpoint expects for the parameter; <see cref="Label"/> is shown to the user.
    /// </summary>
    public sealed class ReportOptionDto
    {
        public required string Value { get; init; }
        public required string Label { get; init; }
    }
}
