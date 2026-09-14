import { PurchaseOrderReceiveHeaderDto, PurchaseOrderReceiveHeaderListDto, PurchaseOrderReceiveHeaderEditCommand } from "@/models/po-receive-models";
import { PurchaseOrderReceiveHeaderCreateCommand, PurchaseOrderReceiveHeaderDeleteCommand, PurchaseOrderReceiveHeaderFindCommand } from "@/models/po-receive-models";
import { PurchaseOrderReceiveLineDto, PurchaseOrderReceiveLineCreateCommand, PurchaseOrderReceiveLineEditCommand, PurchaseOrderReceiveLineDeleteCommand } from "@/models/po-receive-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const poReceiveService = {

  async get(poReceiveId: number, token: string): Promise<ApiResponse<PurchaseOrderReceiveHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<PurchaseOrderReceiveHeaderDto>>(`/api/v1/PurchaseOrderReceive/GetPurchaseOrderReceive/?id=${poReceiveId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<PurchaseOrderReceiveHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<PurchaseOrderReceiveHeaderDto>>(`/api/v1/PurchaseOrderReceive/GetPurchaseOrderReceiveHeaderByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(poReceiveFindCommand: PurchaseOrderReceiveHeaderFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<PurchaseOrderReceiveHeaderListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<PurchaseOrderReceiveHeaderListDto[]>>(`/api/v1/PurchaseOrderReceive/FindPurchaseOrderReceiveHeader?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, poReceiveFindCommand);
    return response.data;
  },

  async getDtoByPOId(purchase_order_id: number, token: string): Promise<ApiResponse<PurchaseOrderReceiveHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<PurchaseOrderReceiveHeaderDto>>(`/api/v1/PurchaseOrderReceive/GetDtoByPOId/?purchase_order_id=${purchase_order_id}`);
    return response.data;
  },
  
  async create(poReceiveCreateCommand: PurchaseOrderReceiveHeaderCreateCommand, token: string): Promise<ApiResponse<PurchaseOrderReceiveHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<PurchaseOrderReceiveHeaderDto>>(`/api/v1/PurchaseOrderReceive/CreatePurchaseOrderReceiveHeader`, poReceiveCreateCommand);
    return response.data;
  },

  async update(poReceiveEditCommand: PurchaseOrderReceiveHeaderEditCommand, token: string): Promise<ApiResponse<PurchaseOrderReceiveHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<PurchaseOrderReceiveHeaderDto>>(`/api/v1/PurchaseOrderReceive/UpdatePurchaseOrderReceiveHeader`, poReceiveEditCommand);
    return response.data;
  },

  async delete(poReceiveDeleteCommand: PurchaseOrderReceiveHeaderDeleteCommand, token: string): Promise<ApiResponse<PurchaseOrderReceiveHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<PurchaseOrderReceiveHeaderDto>>(`/api/v1/PurchaseOrderReceive/DeletePurchaseOrderReceive`, poReceiveDeleteCommand);
    return response.data;
  },

  // Line operations
  async createLine(poReceiveLineCreateCommand: PurchaseOrderReceiveLineCreateCommand, token: string): Promise<ApiResponse<PurchaseOrderReceiveLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<PurchaseOrderReceiveLineDto>>(`/api/v1/PurchaseOrderReceive/CreatePurchaseOrderReceiveLine`, poReceiveLineCreateCommand);
    return response.data;
  },

  async updateLine(poReceiveLineEditCommand: PurchaseOrderReceiveLineEditCommand, token: string): Promise<ApiResponse<PurchaseOrderReceiveLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<PurchaseOrderReceiveLineDto>>(`/api/v1/PurchaseOrderReceive/UpdatePurchaseOrderReceiveLine`, poReceiveLineEditCommand);
    return response.data;
  },

  async deleteLine(poReceiveLineDeleteCommand: PurchaseOrderReceiveLineDeleteCommand, token: string): Promise<ApiResponse<PurchaseOrderReceiveLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<PurchaseOrderReceiveLineDto>>(`/api/v1/PurchaseOrderReceive/DeletePurchaseOrderReceiveLine`, poReceiveLineDeleteCommand);
    return response.data;
  },
}; 