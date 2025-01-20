using Prometheus.Models;

namespace Prometheus.Api.Models.Module.Address.Dto;

public class AddressDto : BaseDto
{
    public required string street_address1 { get; set; }
    public required string? street_address2 { get; set; }
    public required string city { get; set; }
    public required string state { get; set; }
    public required string postal_code { get; set; }
    public required string country { get; set; }
    public required string guid { get; set; }
}
