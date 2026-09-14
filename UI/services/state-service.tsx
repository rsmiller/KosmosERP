import { StateDto, StateListDto, StateEditCommand } from "@/models/country-models";
import { StateCreateCommand, StateDeleteCommand, StateFindCommand } from "@/models/country-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const stateService = {
  async get(stateId: number, token: string): Promise<ApiResponse<StateDto>> {
  const response = await axiosInstance(token).get<ApiResponse<StateDto>>(`/api/v1/State/GetState/?id=${stateId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<StateDto>> {
    const response = await axiosInstance(token).get<ApiResponse<StateDto>>(`/api/v1/State/GetStateByGuid/?guid=${guid}`);
    return response.data;
  },

  async getByISOAsync(country_id: number, iso: string, token: string): Promise<ApiResponse<StateDto>> {
    const response = await axiosInstance(token).get<ApiResponse<StateDto>>(`/api/v1/State/GetStateByISOAsync?country_id=${country_id}&iso2=${iso}`);
    return response.data;
  },

  async find(stateFindCommand: StateFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<StateListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<StateListDto[]>>(`/api/v1/State/FindState?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, stateFindCommand);
    return response.data;
  },

  async create(stateCreateCommand: StateCreateCommand, token: string): Promise<ApiResponse<StateDto>> {
    const response = await axiosInstance(token).post<ApiResponse<StateDto>>(`/api/v1/State/CreateState`, stateCreateCommand);
    return response.data;
  },

  async update(stateEditCommand: StateEditCommand, token: string): Promise<ApiResponse<StateDto>> {
    const response = await axiosInstance(token).put<ApiResponse<StateDto>>(`/api/v1/State/UpdateState`, stateEditCommand);
    return response.data;
  },

  async delete(stateDeleteCommand: StateDeleteCommand, token: string): Promise<any> {
    await axiosInstance(token).post<ApiResponse<StateDto>>(`/api/v1/State/DeleteState`, stateDeleteCommand);
  },
}; 