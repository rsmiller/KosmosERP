import { CommentDto, CommentListDto, CommentEditCommand } from "@/models/comment-models";
import { CommentCreateCommand, CommentDeleteCommand, CommentFindCommand } from "@/models/comment-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const commentService = {
  async get(commentId: number, token: string): Promise<ApiResponse<CommentDto>> {
    const response = await axiosInstance(token).get<ApiResponse<CommentDto>>(`/api/v1/Comment/GetComment/?id=${commentId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<CommentDto>> {
    const response = await axiosInstance(token).get<ApiResponse<CommentDto>>(`/api/v1/Comment/GetCommentByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(commentFindCommand: CommentFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-desc"): Promise<PagedApiResponse<CommentListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<CommentListDto[]>>(`/api/v1/Comment/FindComment?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, commentFindCommand);
    return response.data;
  },

  async create(commentCreateCommand: CommentCreateCommand, token: string): Promise<ApiResponse<CommentDto>> {
    const response = await axiosInstance(token).post<ApiResponse<CommentDto>>(`/api/v1/Comment/CreateComment`, commentCreateCommand);
    return response.data;
  },

  async update(commentEditCommand: CommentEditCommand, token: string): Promise<ApiResponse<CommentDto>> {
    const response = await axiosInstance(token).put<ApiResponse<CommentDto>>(`/api/v1/Comment/UpdateComment`, commentEditCommand);
    return response.data;
  },

  async delete(commentDeleteCommand: CommentDeleteCommand, token: string): Promise<ApiResponse<CommentDto>> {
    const response = await axiosInstance(token).post<ApiResponse<CommentDto>>(`/api/v1/Comment/DeleteComment`, commentDeleteCommand);
    return response.data;
  },
}; 