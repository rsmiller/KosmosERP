using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Comment.Command.Find;

public class CommentFindCommand : DataCommand
{
    public string? wildcard { get; set; }
    public string? object_guid { get; set; }
} 