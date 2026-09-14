import { DocumentUploadDto, DocumentUploadListDto, DocumentUploadEditCommand, DocumentUploadObjectDto, DocumentUploadObjectEditCommand, DocumentUploadObjectCreateCommand, DocumentUploadObjectDeleteCommand, DocumentUploadObjectTagDto, DocumentUploadObjectTagEditCommand, DocumentUploadObjectTagDeleteCommand, DocumentUploadObjectTagCreateCommand } from "@/models/document-models";
import { DocumentUploadCreateCommand, DocumentUploadDeleteCommand, DocumentUploadFindCommand, DocumentUploadRevisionTagCreateCommand } from "@/models/document-models";
import { DocumentUploadObjectCategoryDto, DocumentUploadObjectCategoryCreateCommand, DocumentUploadObjectCategoryEditCommand, DocumentUploadObjectCategoryDeleteCommand } from "@/models/document-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const documentService = {
  async get(documentId: number, token: string): Promise<ApiResponse<DocumentUploadDto>> {
    const response = await axiosInstance(token).get<ApiResponse<DocumentUploadDto>>(`/api/v1/Document/GetDocument/?id=${documentId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<DocumentUploadDto>> {
    const response = await axiosInstance(token).get<ApiResponse<DocumentUploadDto>>(`/api/v1/Document/GetDocumentByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(documentFindCommand: DocumentUploadFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<DocumentUploadListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<DocumentUploadListDto[]>>(`/api/v1/Document/FindDocument?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, documentFindCommand);
    return response.data;
  },

  // This is basically find, but returns more data
  async searchDocuments(documentFindCommand: DocumentUploadFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<ApiResponse<DocumentUploadDto[]>> {
    const response = await axiosInstance(token).post<ApiResponse<DocumentUploadDto[]>>(`/api/v1/Document/SearchDocuments?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, documentFindCommand);
    return response.data;
  },

  async create(
    token: string, file: File,
    documentCreateCommand: DocumentUploadCreateCommand & { document_name?: string; revision_tags?: DocumentUploadRevisionTagCreateCommand[] }
  ): Promise<ApiResponse<DocumentUploadDto>> {
    const form = new FormData();
    form.append("file", file);

    // Scalars required by the API
    if (documentCreateCommand.document_name) 
    {
        form.append("document_name", documentCreateCommand.document_name);
    }

    if (documentCreateCommand.document_object_id != null)
    {
      form.append("document_object_id", String(documentCreateCommand.document_object_id));
    }


    // Flatten revision_tags array for [FromForm] model binding
    if (documentCreateCommand.revision_tags && Array.isArray(documentCreateCommand.revision_tags)) {
      documentCreateCommand.revision_tags.forEach((tag, i) => {

        if (tag.document_upload_object_tag_id != null)
          form.append(`revision_tags[${i}].document_upload_object_tag_id`, String(tag.document_upload_object_tag_id));
        if (tag.tag_name != null)
          form.append(`revision_tags[${i}].tag_name`, tag.tag_name);
        if (tag.tag_value != null)
          form.append(`revision_tags[${i}].tag_value`, tag.tag_value);
      });
    }

    const response = await axiosInstance(token).post<ApiResponse<DocumentUploadDto>>(`/api/v1/Document/CreateDocument`, form, { headers: { "Content-Type": "multipart/form-data" } });
    return response.data;
  },

  async update(documentEditCommand: DocumentUploadEditCommand, token: string): Promise<ApiResponse<DocumentUploadDto>> {
    const response = await axiosInstance(token).put<ApiResponse<DocumentUploadDto>>(`/api/v1/Document/UpdateDocument`, documentEditCommand);
    return response.data;
  },

  async delete(documentDeleteCommand: DocumentUploadDeleteCommand, token: string): Promise<ApiResponse<DocumentUploadDto>> {
    const response = await axiosInstance(token).post<ApiResponse<DocumentUploadDto>>(`/api/v1/Document/DeleteDocument`, documentDeleteCommand);
    return response.data;
  },

  async downloadFile(documentRevisionId: number, token: string): Promise<Blob | null> {
    try {
      const response = await axiosInstance(token).get(`/api/v1/Document/GetFile?document_revision_id=${documentRevisionId}`, {
        responseType: 'blob'
      });
      
      // Extract content type from response headers
      const contentType = response.headers['content-type'] || 'application/octet-stream';
      
      // Create a new blob with the correct content type
      return new Blob([response.data], { type: contentType });
    } catch (error) {
      console.error('Error opening file:', error);
      return null;
    }
  },

  async downloadFileByGuid(documentRevisionGuid: string, token: string): Promise<Blob | null> {
    try {
      const response = await axiosInstance(token).get(`/api/v1/Document/GetFileByGuid?document_revision_guid=${documentRevisionGuid}`, {
        responseType: 'blob'
      });
      
      // Extract content type from response headers
      const contentType = response.headers['content-type'] || 'application/octet-stream';
      
      // Create a new blob with the correct content type
      return new Blob([response.data], { type: contentType });
    } catch (error) {
      console.error('Error opening file:', error);
      return null;
    }
  },

  // DocumentUploadObjectCategory methods
  async getObjectCategories(token: string): Promise<ApiResponse<DocumentUploadObjectCategoryDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<DocumentUploadObjectCategoryDto[]>>(`/api/v1/Document/GetObjectCategories`);
    return response.data;
  },

  async createObjectCategory(createCommand: DocumentUploadObjectCategoryCreateCommand, token: string): Promise<ApiResponse<DocumentUploadObjectCategoryDto>> {
    const response = await axiosInstance(token).post<ApiResponse<DocumentUploadObjectCategoryDto>>(`/api/v1/Document/CreateObjectCategory`, createCommand);
    return response.data;
  },

  async editObjectCategory(editCommand: DocumentUploadObjectCategoryEditCommand, token: string): Promise<ApiResponse<DocumentUploadObjectCategoryDto>> {
    const response = await axiosInstance(token).put<ApiResponse<DocumentUploadObjectCategoryDto>>(`/api/v1/Document/EditObjectCategory`, editCommand);
    return response.data;
  },

  async deleteObjectCategory(deleteCommand: DocumentUploadObjectCategoryDeleteCommand, token: string): Promise<ApiResponse<DocumentUploadObjectCategoryDto>> {
    const response = await axiosInstance(token).delete<ApiResponse<DocumentUploadObjectCategoryDto>>(`/api/v1/Document/DeleteObjectCategory`, { data: deleteCommand });
    return response.data;
  },

  // Document Objects
  async getUploadObjects(token: string): Promise<ApiResponse<DocumentUploadObjectDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<DocumentUploadObjectDto[]>>(`/api/v1/Document/GetUploadObjects`);
    return response.data;
  },

  async editUploadObject(editCommand: DocumentUploadObjectEditCommand, token: string): Promise<ApiResponse<DocumentUploadObjectDto>> {
    const response = await axiosInstance(token).put<ApiResponse<DocumentUploadObjectDto>>(`/api/v1/Document/EditUploadObject`, editCommand);
    return response.data;
  },

  async createUploadObject(createCommand: DocumentUploadObjectCreateCommand, token: string): Promise<ApiResponse<DocumentUploadObjectDto>> {
    const response = await axiosInstance(token).post<ApiResponse<DocumentUploadObjectDto>>(`/api/v1/Document/CreateUploadObject`, createCommand);
    return response.data;
  },

  async deleteUploadObject(deleteCommand: DocumentUploadObjectDeleteCommand, token: string): Promise<ApiResponse<DocumentUploadObjectDto>> {
    const response = await axiosInstance(token).delete<ApiResponse<DocumentUploadObjectDto>>(`/api/v1/Document/DeleteUploadObject`, { data: deleteCommand });
    return response.data;
  },

  // Document Object Tags
  async getUploadObjectTags(document_object_id: number, token: string): Promise<ApiResponse<DocumentUploadObjectTagDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<DocumentUploadObjectTagDto[]>>(`/api/v1/Document/GetDocumentObjectTags?document_object_id=${document_object_id}`);
    return response.data;
  },

  async editUploadObjectTag(editCommand: DocumentUploadObjectTagEditCommand, token: string): Promise<ApiResponse<DocumentUploadObjectTagDto>> {
    const response = await axiosInstance(token).put<ApiResponse<DocumentUploadObjectTagDto>>(`/api/v1/Document/EditDocumentObjectTags`, editCommand);
    return response.data;
  },

  async createUploadObjectTag(createCommand: DocumentUploadObjectTagCreateCommand, token: string): Promise<ApiResponse<DocumentUploadObjectTagDto>> {
    const response = await axiosInstance(token).post<ApiResponse<DocumentUploadObjectTagDto>>(`/api/v1/Document/CreateDocumentObjectTags`, createCommand);
    return response.data;
  },

  async deleteUploadObjectTag(deleteCommand: DocumentUploadObjectTagDeleteCommand, token: string): Promise<ApiResponse<DocumentUploadObjectTagDto>> {
    const response = await axiosInstance(token).delete<ApiResponse<DocumentUploadObjectTagDto>>(`/api/v1/Document/DeleteDocumentObjectTags`, { data: deleteCommand });
    return response.data;
  },
}; 