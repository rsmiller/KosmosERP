import { 
    FinancialTransactionDto, 
    FinancialTransactionListDto, 
    AccountBalanceDto,
    FinancialTransactionFindCommand,
    AccountBalanceFindCommand
} from "@/models/financial-transaction-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const financialTransactionService = {
  async get(id: number, token: string): Promise<ApiResponse<FinancialTransactionDto>> {
    const response = await axiosInstance(token).get<ApiResponse<FinancialTransactionDto>>(`/api/v1/FinancialTransaction/GetFinancialTransaction/?id=${id}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<FinancialTransactionDto>> {
    const response = await axiosInstance(token).get<ApiResponse<FinancialTransactionDto>>(`/api/v1/FinancialTransaction/GetFinancialTransactionByGuid/?guid=${guid}`);
    return response.data;
  },

  async getAccountBalance(command: AccountBalanceFindCommand, token: string): Promise<ApiResponse<AccountBalanceDto>> {
    const response = await axiosInstance(token).post<ApiResponse<AccountBalanceDto>>(`/api/v1/FinancialTransaction/GetAccountBalance`, command);
    return response.data;
  },

  async getAccountLedger(
    chartOfAccountId: number, 
    token: string, 
    start: number = 1, 
    resultCount: number = 20, 
    sortOrder: string = "transaction_date-desc",
    fromDate?: string,
    toDate?: string
  ): Promise<PagedApiResponse<FinancialTransactionListDto[]>> {
    let url = `/api/v1/FinancialTransaction/GetAccountLedger?chartOfAccountId=${chartOfAccountId}&Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`;
    if (fromDate) url += `&fromDate=${fromDate}`;
    if (toDate) url += `&toDate=${toDate}`;
    const response = await axiosInstance(token).get<PagedApiResponse<FinancialTransactionListDto[]>>(url);
    return response.data;
  },

  async find(findCommand: FinancialTransactionFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "transaction_date-desc"): Promise<PagedApiResponse<FinancialTransactionListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<FinancialTransactionListDto[]>>(`/api/v1/FinancialTransaction/FindFinancialTransaction?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, findCommand);
    return response.data;
  },
};
