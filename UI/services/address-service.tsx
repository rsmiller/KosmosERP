import { AddressDto, AddressListDto, AddressEditCommand } from "@/models/address-models";
import { AddressCreateCommand, AddressDeleteCommand, AddressFindCommand } from "@/models/address-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const addressService = {
  async get(addressId: number, token: string): Promise<ApiResponse<AddressDto>> {
    const response = await axiosInstance(token).get<ApiResponse<AddressDto>>(`/api/v1/Address/GetAddress/?id=${addressId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<AddressDto>> {
    const response = await axiosInstance(token).get<ApiResponse<AddressDto>>(`/api/v1/Address/GetAddressByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(addressFindCommand: AddressFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<AddressListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<AddressListDto[]>>(`/api/v1/Address/FindAddress?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, addressFindCommand);
    return response.data;
  },

  async create(addressCreateCommand: AddressCreateCommand, token: string): Promise<ApiResponse<AddressDto>> {
    const response = await axiosInstance(token).post<ApiResponse<AddressDto>>(`/api/v1/Address/CreateAddress`, addressCreateCommand);
    return response.data;
  },

  async update(addressEditCommand: AddressEditCommand, token: string): Promise<ApiResponse<AddressDto>> {
    const response = await axiosInstance(token).put<ApiResponse<AddressDto>>(`/api/v1/Address/UpdateAddress`, addressEditCommand);
    return response.data;
  },

  async delete(addressDeleteCommand: AddressDeleteCommand, token: string): Promise<ApiResponse<AddressDto>> {
    const response = await axiosInstance(token).post<ApiResponse<AddressDto>>(`/api/v1/Address/DeleteAddress`, addressDeleteCommand);
    return response.data;
  },
}; 