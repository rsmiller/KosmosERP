using Prometheus.Models;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.BusinessLayer.Models.Module.User.ListProfiles;

public class GeneralListProfile
{

    [Range(1, int.MaxValue, ErrorMessage = "You can not Start Before 1")]
    public int Start { get; set; } = 0;

    [Range(1, 200, ErrorMessage = "You can not have a value before 1 and more than 200")]
    public int ResultCount { get; set; } = 20;

    public string? SortOrder { get; set; }

    public GeneralListProfile()
    {
        Start = 1;
        ResultCount = 20;
        SortOrder = "id-asc";
    }
}
