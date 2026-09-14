import { ChartOfAccountDto, ChartOfAccountListDto, ChartOfAccountEditCommand } from "@/models/chart-of-account-models";
import { ChartOfAccountCreateCommand, ChartOfAccountDeleteCommand, ChartOfAccountFindCommand } from "@/models/chart-of-account-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const chartOfAccountService = {
  async get(id: number, token: string): Promise<ApiResponse<ChartOfAccountDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ChartOfAccountDto>>(`/api/v1/ChartOfAccount/GetChartOfAccount/?id=${id}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<ChartOfAccountDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ChartOfAccountDto>>(`/api/v1/ChartOfAccount/GetChartOfAccountByGuid/?guid=${guid}`);
    return response.data;
  },

  async getByAccountNumber(accountNumber: string, token: string): Promise<ApiResponse<ChartOfAccountDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ChartOfAccountDto>>(`/api/v1/ChartOfAccount/GetChartOfAccountByAccountNumber/?accountNumber=${accountNumber}`);
    return response.data;
  },

  async getChildAccounts(parentAccountId: number, token: string): Promise<ApiResponse<ChartOfAccountListDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<ChartOfAccountListDto[]>>(`/api/v1/ChartOfAccount/GetChildAccounts/?parentAccountId=${parentAccountId}`);
    return response.data;
  },

  async getAccountsByType(accountType: number, token: string): Promise<ApiResponse<ChartOfAccountListDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<ChartOfAccountListDto[]>>(`/api/v1/ChartOfAccount/GetAccountsByType/?accountType=${accountType}`);
    return response.data;
  },

  async find(findCommand: ChartOfAccountFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "account_number-asc"): Promise<PagedApiResponse<ChartOfAccountListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<ChartOfAccountListDto[]>>(`/api/v1/ChartOfAccount/FindChartOfAccount?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, findCommand);
    return response.data;
  },

  async create(createCommand: ChartOfAccountCreateCommand, token: string): Promise<ApiResponse<ChartOfAccountDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ChartOfAccountDto>>(`/api/v1/ChartOfAccount/CreateChartOfAccount`, createCommand);
    return response.data;
  },

  async update(editCommand: ChartOfAccountEditCommand, token: string): Promise<ApiResponse<ChartOfAccountDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ChartOfAccountDto>>(`/api/v1/ChartOfAccount/UpdateChartOfAccount`, editCommand);
    return response.data;
  },

  async delete(deleteCommand: ChartOfAccountDeleteCommand, token: string): Promise<ApiResponse<ChartOfAccountDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ChartOfAccountDto>>(`/api/v1/ChartOfAccount/DeleteChartOfAccount`, deleteCommand);
    return response.data;
  },
};
