import { VendorDto, VendorListDto, VendorEditCommand } from "@/models/vendor-models";
import { VendorCreateCommand, VendorDeleteCommand, VendorFindCommand } from "@/models/vendor-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const vendorService = {
  async get(vendorId: number, token: string): Promise<ApiResponse<VendorDto>> {
    const response = await axiosInstance(token).get<ApiResponse<VendorDto>>(`/api/v1/Vendor/GetVendor?id=${vendorId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<VendorDto>> {
    const response = await axiosInstance(token).get<ApiResponse<VendorDto>>(`/api/v1/Vendor/GetVendorByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(vendorFindCommand: VendorFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<VendorListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<VendorListDto[]>>(`/api/v1/Vendor/FindVendor?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, vendorFindCommand);
    return response.data;
  },

  async create(vendorCreateCommand: VendorCreateCommand, token: string): Promise<ApiResponse<VendorDto>> {
    const response = await axiosInstance(token).post<ApiResponse<VendorDto>>(`/api/v1/Vendor/CreateVendor`, vendorCreateCommand);
    return response.data;
  },

  async update(vendorEditCommand: VendorEditCommand, token: string): Promise<ApiResponse<VendorDto>> {
    const response = await axiosInstance(token).put<ApiResponse<VendorDto>>(`/api/v1/Vendor/UpdateVendor`, vendorEditCommand);
    return response.data;
  },

  async delete(vendorDeleteCommand: VendorDeleteCommand, token: string): Promise<ApiResponse<VendorDto>> {
    const response = await axiosInstance(token).post<ApiResponse<VendorDto>>(`/api/v1/Vendor/DeleteVendor`, vendorDeleteCommand);
    return response.data;
  },
}; 