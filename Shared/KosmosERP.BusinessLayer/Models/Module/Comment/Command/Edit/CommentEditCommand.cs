using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Comment.Command.Edit;

public class CommentEditCommand : DataCommand
{
    [Required]
    public int id { get; set; }

    public string? object_guid { get; set; }

    public string? comment_text { get; set; }
} 