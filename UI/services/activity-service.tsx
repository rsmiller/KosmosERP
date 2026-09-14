import { ActivityDto, ActivityListDto, ActivityEditCommand } from "@/models/activity-models";
import { ActivityCreateCommand, ActivityDeleteCommand, ActivityFindCommand } from "@/models/activity-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const activityService = {
  async get(activityId: number, token: string): Promise<ApiResponse<ActivityDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ActivityDto>>(`/api/v1/Activity/GetActivity/?id=${activityId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<ActivityDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ActivityDto>>(`/api/v1/Activity/GetActivityByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(activityFindCommand: ActivityFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<ActivityListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<ActivityListDto[]>>(`/api/v1/Activity/FindActivity?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, activityFindCommand);
    return response.data;
  },

  async create(activityCreateCommand: ActivityCreateCommand, token: string): Promise<ApiResponse<ActivityDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ActivityDto>>(`/api/v1/Activity/CreateActivity`, activityCreateCommand);
    return response.data;
  },

  async update(activityEditCommand: ActivityEditCommand, token: string): Promise<ApiResponse<ActivityDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ActivityDto>>(`/api/v1/Activity/UpdateActivity`, activityEditCommand);
    return response.data;
  },

  async delete(activityDeleteCommand: ActivityDeleteCommand, token: string): Promise<ApiResponse<ActivityDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ActivityDto>>(`/api/v1/Activity/DeleteActivity`, activityDeleteCommand);
    return response.data;
  },
}; 