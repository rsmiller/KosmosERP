using KosmosERP.Models;

namespace KosmosERP.BusinessLayer.Models.Module.Comment.Dto;

public class CommentListDto : BaseDto
{
    public string object_guid { get; set; }
    public string comment_text { get; set; }
    public string guid { get; set; }
    public string comment_by_name { get; set; }
} 