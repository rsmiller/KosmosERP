import { ARInvoiceHeaderDto, ARInvoiceHeaderListDto, ARInvoiceHeaderEditCommand, vm_OrdersReadyForInvoicing, vw_PartialInvoices } from "@/models/ar-models";
import { ARInvoiceHeaderCreateCommand, ARInvoiceHeaderDeleteCommand, ARInvoiceHeaderFindCommand } from "@/models/ar-models";
import { ARInvoiceLineDto, ARInvoiceLineCreateCommand, ARInvoiceLineEditCommand, ARInvoiceLineDeleteCommand } from "@/models/ar-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const arInvoiceService = {
  // Header operations
  async get(arInvoiceId: number, token: string): Promise<ApiResponse<ARInvoiceHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ARInvoiceHeaderDto>>(`/api/v1/ARInvoice/GetARInvoice/?id=${arInvoiceId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<ARInvoiceHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ARInvoiceHeaderDto>>(`/api/v1/ARInvoice/GetARInvoiceByGuid/?guid=${guid}`);
    return response.data;
  },

  async getOrdersReadyForInvoicing(token: string, customer_id?: string): Promise<ApiResponse<vm_OrdersReadyForInvoicing[]>> {
    const response = await axiosInstance(token).get<ApiResponse<vm_OrdersReadyForInvoicing[]>>(`/api/v1/ARInvoice/GetOrdersReadyForInvoicing/?customer_id=${customer_id}`);
    return response.data;
  },

  async getPartialInvoices(token: string): Promise<ApiResponse<vw_PartialInvoices[]>> {
    const response = await axiosInstance(token).get<ApiResponse<vw_PartialInvoices[]>>(`/api/v1/ARInvoice/GetPartialInvoices`);
    return response.data;
  },

  async find(arInvoiceFindCommand: ARInvoiceHeaderFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<ARInvoiceHeaderListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<ARInvoiceHeaderListDto[]>>(`/api/v1/ARInvoice/FindARInvoice?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, arInvoiceFindCommand);
    return response.data;
  },

  async create(arInvoiceCreateCommand: ARInvoiceHeaderCreateCommand, token: string): Promise<ApiResponse<ARInvoiceHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ARInvoiceHeaderDto>>(`/api/v1/ARInvoice/CreateARInvoice`, arInvoiceCreateCommand);
    return response.data;
  },

  async update(arInvoiceEditCommand: ARInvoiceHeaderEditCommand, token: string): Promise<ApiResponse<ARInvoiceHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ARInvoiceHeaderDto>>(`/api/v1/ARInvoice/UpdateARInvoice`, arInvoiceEditCommand);
    return response.data;
  },

  async delete(arInvoiceDeleteCommand: ARInvoiceHeaderDeleteCommand, token: string): Promise<ApiResponse<ARInvoiceHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ARInvoiceHeaderDto>>(`/api/v1/ARInvoice/DeleteARInvoice`, arInvoiceDeleteCommand);
    return response.data;
  },

  // Line operations
  async createLine(arInvoiceLineCreateCommand: ARInvoiceLineCreateCommand, token: string): Promise<ApiResponse<ARInvoiceLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ARInvoiceLineDto>>(`/api/v1/ARInvoice/CreateARInvoiceLine`, arInvoiceLineCreateCommand);
    return response.data;
  },

  async updateLine(arInvoiceLineEditCommand: ARInvoiceLineEditCommand, token: string): Promise<ApiResponse<ARInvoiceLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ARInvoiceLineDto>>(`/api/v1/ARInvoice/UpdateARInvoiceLine`, arInvoiceLineEditCommand);
    return response.data;
  },

  async deleteLine(arInvoiceLineDeleteCommand: ARInvoiceLineDeleteCommand, token: string): Promise<ApiResponse<ARInvoiceLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ARInvoiceLineDto>>(`/api/v1/ARInvoice/DeleteARInvoiceLine`, arInvoiceLineDeleteCommand);
    return response.data;
  },
 };