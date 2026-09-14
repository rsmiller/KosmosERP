import { OpportunityDto, OpportunityListDto, OpportunityEditCommand, OpportunityLineCreateCommand, OpportunityLineEditCommand, OpportunityLineDeleteCommand, OpportunityLineDto } from "@/models/opportunity-models";
import { OpportunityCreateCommand, OpportunityDeleteCommand, OpportunityFindCommand } from "@/models/opportunity-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const opportunityService = {
  async get(opportunityId: number, token: string): Promise<ApiResponse<OpportunityDto>> {
    const response = await axiosInstance(token).get<ApiResponse<OpportunityDto>>(`/api/v1/Opportunity/GetOpportunity/?id=${opportunityId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<OpportunityDto>> {
    const response = await axiosInstance(token).get<ApiResponse<OpportunityDto>>(`/api/v1/Opportunity/GetOpportunityByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(opportunityFindCommand: OpportunityFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<OpportunityListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<OpportunityListDto[]>>(`/api/v1/Opportunity/FindOpportunity?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, opportunityFindCommand);
    return response.data;
  },

  async create(opportunityCreateCommand: OpportunityCreateCommand, token: string): Promise<ApiResponse<OpportunityDto>> {
    const response = await axiosInstance(token).post<ApiResponse<OpportunityDto>>(`/api/v1/Opportunity/CreateOpportunity`, opportunityCreateCommand);
    return response.data;
  },

  async update(opportunityEditCommand: OpportunityEditCommand, token: string): Promise<ApiResponse<OpportunityDto>> {
    const response = await axiosInstance(token).put<ApiResponse<OpportunityDto>>(`/api/v1/Opportunity/UpdateOpportunity`, opportunityEditCommand);
    return response.data;
  },

  async delete(opportunityDeleteCommand: OpportunityDeleteCommand, token: string): Promise<ApiResponse<OpportunityDto>> {
    const response = await axiosInstance(token).post<ApiResponse<OpportunityDto>>(`/api/v1/Opportunity/DeleteOpportunity`, opportunityDeleteCommand);
    return response.data;
  },

  async createLine(orderLineCreateCommand: OpportunityLineCreateCommand, token: string): Promise<ApiResponse<OpportunityLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<OpportunityLineDto>>(`/api/v1/Opportunity/CreateOpportunityLine`, orderLineCreateCommand);
    return response.data;
  },

  async updateLine(orderLineEditCommand: OpportunityLineEditCommand, token: string): Promise<ApiResponse<OpportunityLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<OpportunityLineDto>>(`/api/v1/Opportunity/UpdateOpportunityLine`, orderLineEditCommand);
    return response.data;
  },

  async deleteLine(orderLineDeleteCommand: OpportunityLineDeleteCommand, token: string): Promise<ApiResponse<OpportunityLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<OpportunityLineDto>>(`/api/v1/Opportunity/DeleteOpportunityLine`, orderLineDeleteCommand);
    return response.data;
  },
}; 