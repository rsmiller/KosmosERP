using KosmosERP.Models;
using System.ComponentModel.DataAnnotations;

namespace KosmosERP.BusinessLayer.Models.Module.Comment.Command.Create;

public class CommentCreateCommand : DataCommand
{
    [Required]
    public string object_guid { get; set; }

    [Required]
    public string comment_text { get; set; }
} 