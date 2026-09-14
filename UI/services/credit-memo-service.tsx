import { CreditMemoHeaderDto, CreditMemoHeaderCreateCommand, CreditMemoLineCreateCommand, CreditMemoLineDto } from "@/models/credit-memo-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const creditMemoService = {
  async get(id: number, token: string): Promise<ApiResponse<CreditMemoHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<CreditMemoHeaderDto>>(`/api/v1/CreditMemo/GetCreditMemo/?id=${id}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<CreditMemoHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<CreditMemoHeaderDto>>(`/api/v1/CreditMemo/GetCreditMemoByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(findCommand: any, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<any>> {
    const response = await axiosInstance(token).post<PagedApiResponse<any>>(`/api/v1/CreditMemo/FindCreditMemo?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, findCommand);
    return response.data;
  },

  async create(createCommand: CreditMemoHeaderCreateCommand, token: string): Promise<ApiResponse<CreditMemoHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<CreditMemoHeaderDto>>(`/api/v1/CreditMemo/CreateCreditMemo`, createCommand);
    return response.data;
  },

  async createLine(createCommand: CreditMemoLineCreateCommand, token: string): Promise<ApiResponse<CreditMemoLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<CreditMemoLineDto>>(`/api/v1/CreditMemo/CreateCreditMemoLine`, createCommand);
    return response.data;
  },

  async update(editCommand: any, token: string): Promise<ApiResponse<CreditMemoHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<CreditMemoHeaderDto>>(`/api/v1/CreditMemo/UpdateCreditMemo`, editCommand);
    return response.data;
  },

  async updateLine(editCommand: any, token: string): Promise<ApiResponse<CreditMemoLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<CreditMemoLineDto>>(`/api/v1/CreditMemo/UpdateCreditMemoLine`, editCommand);
    return response.data;
  },

  async delete(deleteCommand: any, token: string): Promise<ApiResponse<CreditMemoHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<CreditMemoHeaderDto>>(`/api/v1/CreditMemo/DeleteCreditMemo`, deleteCommand);
    return response.data;
  },

  async deleteLine(deleteCommand: any, token: string): Promise<ApiResponse<CreditMemoLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<CreditMemoLineDto>>(`/api/v1/CreditMemo/DeleteCreditMemoLine`, deleteCommand);
    return response.data;
  },
};

export default creditMemoService;
