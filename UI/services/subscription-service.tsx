import { SubscriptionDto, SubscriptionListDto, SubscriptionEditCommand } from "@/models/subscription-models";
import { SubscriptionCreateCommand, SubscriptionDeleteCommand, SubscriptionFindCommand } from "@/models/subscription-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const subscriptionService = {
  async get(subscriptionId: number, token: string): Promise<ApiResponse<SubscriptionDto>> {
    const response = await axiosInstance(token).get<ApiResponse<SubscriptionDto>>(`/api/v1/Subscription/GetSubscription/?id=${subscriptionId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<SubscriptionDto>> {
    const response = await axiosInstance(token).get<ApiResponse<SubscriptionDto>>(`/api/v1/Subscription/GetSubscriptionByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(subscriptionFindCommand: SubscriptionFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<SubscriptionListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<SubscriptionListDto[]>>(`/api/v1/Subscription/FindSubscription?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, subscriptionFindCommand);
    return response.data;
  },

  async create(subscriptionCreateCommand: SubscriptionCreateCommand, token: string): Promise<ApiResponse<SubscriptionDto>> {
    const response = await axiosInstance(token).post<ApiResponse<SubscriptionDto>>(`/api/v1/Subscription/CreateSubscription`, subscriptionCreateCommand);
    return response.data;
  },

  async update(subscriptionEditCommand: SubscriptionEditCommand, token: string): Promise<ApiResponse<SubscriptionDto>> {
    const response = await axiosInstance(token).put<ApiResponse<SubscriptionDto>>(`/api/v1/Subscription/UpdateSubscription`, subscriptionEditCommand);
    return response.data;
  },

  async delete(subscriptionDeleteCommand: SubscriptionDeleteCommand, token: string): Promise<ApiResponse<SubscriptionDto>> {
    const response = await axiosInstance(token).post<ApiResponse<SubscriptionDto>>(`/api/v1/Subscription/DeleteSubscription`, subscriptionDeleteCommand);
    return response.data;
  },
}; 