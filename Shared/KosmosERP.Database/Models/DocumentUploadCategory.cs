using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class DocumentUploadCategory : BaseDatabaseModel
{
    public int? parent_category_id { get; set; }

    [Required]
    public string category_name { get; set; }

    [Required]
    public string internal_category_name { get; set; }

    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();
}