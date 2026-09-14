import { TransactionDto, TransactionListDto, TransactionEditCommand } from "@/models/transaction-models";
import { TransactionCreateCommand, TransactionDeleteCommand, TransactionFindCommand } from "@/models/transaction-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const transactionService = {
  async get(transactionId: number, token: string): Promise<ApiResponse<TransactionDto>> {
    const response = await axiosInstance(token).get<ApiResponse<TransactionDto>>(`/api/v1/Transaction/GetTransaction/?id=${transactionId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<TransactionDto>> {
    const response = await axiosInstance(token).get<ApiResponse<TransactionDto>>(`/api/v1/Transaction/GetTransactionByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(transactionFindCommand: TransactionFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<TransactionListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<TransactionListDto[]>>(`/api/v1/Transaction/FindTransaction?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, transactionFindCommand);
    return response.data;
  },

  async create(transactionCreateCommand: TransactionCreateCommand, token: string): Promise<ApiResponse<TransactionDto>> {
    const response = await axiosInstance(token).post<ApiResponse<TransactionDto>>(`/api/v1/Transaction/CreateTransaction`, transactionCreateCommand);
    return response.data;
  },

  async update(transactionEditCommand: TransactionEditCommand, token: string): Promise<ApiResponse<TransactionDto>> {
    const response = await axiosInstance(token).put<ApiResponse<TransactionDto>>(`/api/v1/Transaction/UpdateTransaction`, transactionEditCommand);
    return response.data;
  },

  async delete(transactionDeleteCommand: TransactionDeleteCommand, token: string): Promise<ApiResponse<TransactionDto>> {
    const response = await axiosInstance(token).post<ApiResponse<TransactionDto>>(`/api/v1/Transaction/DeleteTransaction`, transactionDeleteCommand);
    return response.data;
  },
}; 