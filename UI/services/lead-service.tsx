import { LeadDto, LeadListDto, LeadEditCommand } from "@/models/lead-models";
import { LeadCreateCommand, LeadDeleteCommand, LeadFindCommand } from "@/models/lead-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const leadService = {
  async get(leadId: number, token: string): Promise<ApiResponse<LeadDto>> {
    const response = await axiosInstance(token).get<ApiResponse<LeadDto>>(`/api/v1/Lead/GetLead/?id=${leadId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<LeadDto>> {
    const response = await axiosInstance(token).get<ApiResponse<LeadDto>>(`/api/v1/Lead/GetLeadByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(leadFindCommand: LeadFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<LeadListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<LeadListDto[]>>(`/api/v1/Lead/FindLead?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, leadFindCommand);
    return response.data;
  },

  async create(leadCreateCommand: LeadCreateCommand, token: string): Promise<ApiResponse<LeadDto>> {
    const response = await axiosInstance(token).post<ApiResponse<LeadDto>>(`/api/v1/Lead/CreateLead`, leadCreateCommand);
    return response.data;
  },

  async update(leadEditCommand: LeadEditCommand, token: string): Promise<ApiResponse<LeadDto>> {
    const response = await axiosInstance(token).put<ApiResponse<LeadDto>>(`/api/v1/Lead/UpdateLead`, leadEditCommand);
    return response.data;
  },

  async delete(leadDeleteCommand: LeadDeleteCommand, token: string): Promise<ApiResponse<LeadDto>> {
    const response = await axiosInstance(token).post<ApiResponse<LeadDto>>(`/api/v1/Lead/DeleteLead`, leadDeleteCommand);
    return response.data;
  },
}; 