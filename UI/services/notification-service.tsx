import { NotificationDto, NotificationListDto, NotificationEditCommand } from "@/models/notification-models";
import { NotificationCreateCommand, NotificationDeleteCommand, NotificationFindCommand } from "@/models/notification-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const notificationService = {
  async get(notificationId: number, token: string): Promise<ApiResponse<NotificationDto>> {
    const response = await axiosInstance(token).get<ApiResponse<NotificationDto>>(`/api/v1/Notification/GetNotification/?id=${notificationId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<NotificationDto>> {
    const response = await axiosInstance(token).get<ApiResponse<NotificationDto>>(`/api/v1/Notification/GetNotificationByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(notificationFindCommand: NotificationFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<NotificationListDto>[]> {
    const response = await axiosInstance(token).post<PagedApiResponse<NotificationListDto>[]>(`/api/v1/Notification/FindNotification?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, notificationFindCommand);
    return response.data;
  },

  async create(notificationCreateCommand: NotificationCreateCommand, token: string): Promise<ApiResponse<NotificationDto>> {
    const response = await axiosInstance(token).post<ApiResponse<NotificationDto>>(`/api/v1/Notification/CreateNotification`, notificationCreateCommand);
    return response.data;
  },

  async update(notificationEditCommand: NotificationEditCommand, token: string): Promise<ApiResponse<NotificationDto>> {
    const response = await axiosInstance(token).put<ApiResponse<NotificationDto>>(`/api/v1/Notification/UpdateNotification`, notificationEditCommand);
    return response.data;
  },

  async delete(notificationDeleteCommand: NotificationDeleteCommand, token: string): Promise<ApiResponse<NotificationDto>> {
    const response = await axiosInstance(token).post<ApiResponse<NotificationDto>>(`/api/v1/Notification/DeleteNotification`, notificationDeleteCommand);
    return response.data;
  },
}; 