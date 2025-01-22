using Prometheus.Models;

namespace Prometheus.Api.Models.Module.State.Dto;

public class StateDto : BaseDto
{
    public int country_id { get; set; }
    public string state_name { get; set; }
    public string iso2 { get; set; }
}
