using System.ComponentModel.DataAnnotations;

namespace KosmosERP.Database.Models
{
    public class Module : BaseDatabaseModel
    {
        [Required]
        [MaxLength(255)]
        public string module_id { get; set; }

        [Required]
        public string module_name { get; set; }
    }
}
