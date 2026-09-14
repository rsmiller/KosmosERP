import { BOMDto, BOMListDto, BOMEditCommand } from "@/models/bom-models";
import { BOMCreateCommand, BOMDeleteCommand, BOMFindCommand } from "@/models/bom-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const bomService = {
  async get(bomId: number, token: string): Promise<ApiResponse<BOMDto>> {
    const response = await axiosInstance(token).get<ApiResponse<BOMDto>>(`/api/v1/BOM/GetBOM/?id=${bomId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<BOMDto>> {
    const response = await axiosInstance(token).get<ApiResponse<BOMDto>>(`/api/v1/BOM/GetBOMByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(bomFindCommand: BOMFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "order_number-asc"): Promise<PagedApiResponse<BOMListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<BOMListDto[]>>(`/api/v1/BOM/FindBOM?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, bomFindCommand);
    return response.data;
  },

  async create(bomCreateCommand: BOMCreateCommand, token: string): Promise<ApiResponse<BOMDto>> {
    const response = await axiosInstance(token).post<ApiResponse<BOMDto>>(`/api/v1/BOM/CreateBOM`, bomCreateCommand);
    return response.data;
  },

  async update(bomEditCommand: BOMEditCommand, token: string): Promise<ApiResponse<BOMDto>> {
    const response = await axiosInstance(token).put<ApiResponse<BOMDto>>(`/api/v1/BOM/UpdateBOM`, bomEditCommand);
    return response.data;
  },

  async delete(bomDeleteCommand: BOMDeleteCommand, token: string): Promise<ApiResponse<BOMDto>> {
    const response = await axiosInstance(token).post<ApiResponse<BOMDto>>(`/api/v1/BOM/DeleteBOM`, bomDeleteCommand);
    return response.data;
  },
}; 