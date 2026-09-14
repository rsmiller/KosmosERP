import { BaseDto, DataCommand } from "./base-models";

export class DocumentUploadRevisionTagDto extends BaseDto 
{
    document_upload_revision_id?: number;
    document_upload_object_tag_id?: number;
    tag_name?: string;
    tag_value?: string;
    is_required?: boolean;
}

export class DocumentUploadRevisionDto extends BaseDto 
{
    document_upload_id?: number;
    document_name?: string;
    document_path?: string;
    rev_num?: number;
    rejected_reason?: string;
    approved_on?: Date;
    approved_by?: number;
    rejected_on?: Date;
    rejected_by?: number;
    revision_tags?: DocumentUploadRevisionTagDto[];
}

export class DocumentUploadDto extends BaseDto {
  rev_num?: number;
  document_object_id?: number;
  document_revisions?: DocumentUploadRevisionDto[];
  tag_templates?: DocumentUploadObjectTagTemplate[];
}

export class DocumentUploadListDto extends BaseDto {
  rev_num?: number;
  document_object_id?: number;
  document_name?: string;
  revision_tags?: DocumentUploadRevisionTagDto[];
}

export class DocumentUploadCreateCommand extends DataCommand {
  document_name?: string;
  document_object_id?: number;
  revision_tags?: DocumentUploadRevisionTagCreateCommand[];
}

export class DocumentUploadEditCommand extends DataCommand {
  id?: number;
  rev_num?: number;
  document_object_id?: number;
  document_revisions?: DocumentUploadRevisionEditCommand[];
}

export class DocumentUploadRevisionCreateCommand extends DataCommand {
  document_upload_id?: number;
  document_name?: string;
  document_path?: string;
  rev_num?: number;
  rejected_reason?: string;
  approved_on?: Date;
  approved_by?: number;
  rejected_on?: Date;
  rejected_by?: number;
}

export class DocumentUploadRevisionEditCommand extends DataCommand {
  id?: number;
  document_upload_id?: number;
  document_name?: string;
  document_path?: string;
  rev_num?: number;
  rejected_reason?: string;
  approved_on?: Date;
  approved_by?: number;
  rejected_on?: Date;
  rejected_by?: number;
}

export class DocumentUploadDeleteCommand extends DataCommand {
  id?: number;
}

export class DocumentUploadFindCommand extends DataCommand {
  wildcard?: string;
  category_id?: number;
  object_id?: number;
}

export class DocumentUploadRevisionTagCreateCommand  extends DataCommand {
  document_upload_revision_id?: number;
  document_upload_object_tag_id?: number;
  tag_name?: string;
  tag_value?: string;
}

export class DocumentUploadObjectCategoryDto extends BaseDto {
  parent_category_id?: number;
  category_name?: string;
  internal_category_name?: string;

  document_objects: DocumentUploadObjectDto[] = new Array<DocumentUploadObjectDto>();
}

export class DocumentUploadObjectCategoryCreateCommand extends DataCommand {
  parent_category_id?: number;
  category_name?: string;
  internal_category_name?: string;
}

export class DocumentUploadObjectCategoryEditCommand extends DataCommand {
  id?: number;
  parent_category_id?: number;
  category_name?: string;
  internal_category_name?: string;
}

export class DocumentUploadObjectCategoryDeleteCommand extends DataCommand {
  id?: number;
}

export class DocumentUploadObjectDto extends BaseDto {
  friendly_name?: string;
  internal_name?: string;
  tag_templates?: DocumentUploadObjectTagTemplate[];
}

export class DocumentUploadObjectTagTemplate
{
  id: number = 0;
  document_object_id?: number;
  name?: string;
  is_required?: boolean;

  // for ui
  value?: string;
}

export class DocumentUploadObjectEditCommand extends DataCommand
{
  id?: number;
  friendly_name?: string;
  internal_name?: string;
}

export class DocumentUploadObjectCreateCommand extends DataCommand
{
  friendly_name?: string;
  internal_name?: string;
}

export class DocumentUploadObjectDeleteCommand extends DataCommand {
  id?: number;
}

export class DocumentUploadObjectTagDto
{
  id?: number;
  document_object_id?: number;
  name?: string;
  is_required?: boolean;
}

export class DocumentUploadObjectTagEditCommand extends DataCommand
{
  id?: number;
  name?: string;
  is_required?: boolean;
}

export class DocumentUploadObjectTagCreateCommand extends DataCommand
{
  document_object_id?: number;
  name?: string;
  is_required?: boolean;
}

export class DocumentUploadObjectTagDeleteCommand extends DataCommand {
  id?: number;
}