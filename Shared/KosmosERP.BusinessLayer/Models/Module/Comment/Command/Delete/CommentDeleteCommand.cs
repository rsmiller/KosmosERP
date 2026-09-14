using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Comment.Command.Delete;

public class CommentDeleteCommand : DataCommand
{
    [Required]
    public int id { get; set; }
} 