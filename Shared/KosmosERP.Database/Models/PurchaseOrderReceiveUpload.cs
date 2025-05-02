
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KosmosERP.Database.Models;

public class PurchaseOrderReceiveUpload : BaseDatabaseModel
{
    [Required]
    public int purchase_order_receive_header_id { get; set; }

    [Required]
    public int document_upload_id { get; set; }

    [MaxLength(50)]
    [Required]
    public string guid { get; set; } = Guid.NewGuid().ToString();

    [NotMapped]
    public DocumentUpload document_upload { get; set; }
}
