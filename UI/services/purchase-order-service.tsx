import { PurchaseOrderHeaderDto, PurchaseOrderHeaderListDto, PurchaseOrderHeaderEditCommand } from "@/models/purchase-order-models";
import { PurchaseOrderHeaderCreateCommand, PurchaseOrderHeaderDeleteCommand, PurchaseOrderHeaderFindCommand } from "@/models/purchase-order-models";
import { PurchaseOrderLineDto, PurchaseOrderLineCreateCommand, PurchaseOrderLineEditCommand, PurchaseOrderLineDeleteCommand } from "@/models/purchase-order-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const purchaseOrderService = {

  async get(purchaseOrderId: number, token: string): Promise<ApiResponse<PurchaseOrderHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<PurchaseOrderHeaderDto>>(`/api/v1/PurchaseOrder/GetPurchaseOrderHeader/?id=${purchaseOrderId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<PurchaseOrderHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<PurchaseOrderHeaderDto>>(`/api/v1/PurchaseOrder/GetPurchaseOrderHeaderByGuid/?guid=${guid}`);
    return response.data;
  },

  async getByPONumber(poNumber: number, token: string): Promise<ApiResponse<PurchaseOrderHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<PurchaseOrderHeaderDto>>(`/api/v1/PurchaseOrder/GetByPONumber/?po_number=${poNumber}`);
    return response.data;
  },

  async find(purchaseOrderFindCommand: PurchaseOrderHeaderFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<PurchaseOrderHeaderListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<PurchaseOrderHeaderListDto[]>>(`/api/v1/PurchaseOrder/FindPurchaseOrderHeader?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, purchaseOrderFindCommand);
    return response.data;
  },

  async create(purchaseOrderCreateCommand: PurchaseOrderHeaderCreateCommand, token: string): Promise<ApiResponse<PurchaseOrderHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<PurchaseOrderHeaderDto>>(`/api/v1/PurchaseOrder/CreatePurchaseOrderHeader`, purchaseOrderCreateCommand);
    return response.data;
  },

  async update(purchaseOrderEditCommand: PurchaseOrderHeaderEditCommand, token: string): Promise<ApiResponse<PurchaseOrderHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<PurchaseOrderHeaderDto>>(`/api/v1/PurchaseOrder/UpdatePurchaseOrderHeader`, purchaseOrderEditCommand);
    return response.data;
  },

  async delete(purchaseOrderDeleteCommand: PurchaseOrderHeaderDeleteCommand, token: string): Promise<ApiResponse<PurchaseOrderHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<PurchaseOrderHeaderDto>>(`/api/v1/PurchaseOrder/DeletePurchaseOrderHeader`, purchaseOrderDeleteCommand);
    return response.data;
  },

  // Line operations
  async createLine(purchaseOrderLineCreateCommand: PurchaseOrderLineCreateCommand, token: string): Promise<ApiResponse<PurchaseOrderLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<PurchaseOrderLineDto>>(`/api/v1/PurchaseOrder/CreatePurchaseOrderLine`, purchaseOrderLineCreateCommand);
    return response.data;
  },

  async updateLine(purchaseOrderLineEditCommand: PurchaseOrderLineEditCommand, token: string): Promise<ApiResponse<PurchaseOrderLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<PurchaseOrderLineDto>>(`/api/v1/PurchaseOrder/UpdatePurchaseOrderLine`, purchaseOrderLineEditCommand);
    return response.data;
  },

  async deleteLine(purchaseOrderLineDeleteCommand: PurchaseOrderLineDeleteCommand, token: string): Promise<ApiResponse<PurchaseOrderLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<PurchaseOrderLineDto>>(`/api/v1/PurchaseOrder/DeletePurchaseOrderLine`, purchaseOrderLineDeleteCommand);
    return response.data;
  },
}; 