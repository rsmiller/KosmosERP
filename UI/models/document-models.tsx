import { BaseDto } from "./base-models";

export interface DocumentUploadRevisionTagDto extends BaseDto 
{
    document_upload_revision_id?: number;
    document_upload_object_tag_id?: number;
    tag_name?: string;
    tag_value?: string;
    is_required?: boolean;
}

export interface DocumentUploadRevisionDto extends BaseDto 
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

export interface DocumentUploadDto extends BaseDto {
  rev_num?: number;
  document_object_id?: number;
  document_revisions?: DocumentUploadRevisionDto[];
}