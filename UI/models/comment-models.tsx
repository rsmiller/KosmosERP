import { BaseDto, DataCommand } from "./base-models";

export class CommentDto extends BaseDto {
  object_guid?: string;
  comment_text?: string;
  isDirty: boolean = false;
  comment_by_name?: string;
}

export class CommentListDto extends BaseDto {
  object_guid?: string;
  comment_text?: string;
  isDirty: boolean = false;
  comment_by_name?: string;
}

export class CommentCreateCommand extends DataCommand {
  object_guid?: string;
  comment_text?: string;
}

export class CommentEditCommand extends DataCommand {
  id?: number;
  object_guid?: string;
  comment_text?: string;
}

export class CommentDeleteCommand extends DataCommand {
  id?: number;
}

export class CommentFindCommand extends DataCommand {
  object_guid?: string;
  wildcard?: string;
} 