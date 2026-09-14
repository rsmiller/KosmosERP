using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Models;

public class GlobalSearchFindCommand : DataCommand
{
    [Required]
    public PagingSortingParameters parameters { get; set; }
    [Required]
    public string wildcard { get; set; }
}