import { APInvoiceHeaderDto, APInvoiceHeaderListDto, APInvoiceHeaderEditCommand } from "@/models/ap-models";
import { APInvoiceHeaderCreateCommand, APInvoiceHeaderDeleteCommand, APInvoiceHeaderFindCommand } from "@/models/ap-models";
import { APInvoiceLineDto, APInvoiceLineCreateCommand, APInvoiceLineEditCommand, APInvoiceLineDeleteCommand } from "@/models/ap-models";
import { APInvoiceAssoicationCommand, APInvoiceAssociatePOCommand } from "@/models/ap-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const apInvoiceService = {

  // Header operations
  async get(apInvoiceId: number, token: string): Promise<ApiResponse<APInvoiceHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<APInvoiceHeaderDto>>(`/api/v1/APInvoice/GetAPInvoice/?id=${apInvoiceId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<APInvoiceHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<APInvoiceHeaderDto>>(`/api/v1/APInvoice/GetAPInvoiceByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(apInvoiceFindCommand: APInvoiceHeaderFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<APInvoiceHeaderListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<APInvoiceHeaderListDto[]>>(`/api/v1/APInvoice/FindAPInvoice?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, apInvoiceFindCommand);
    return response.data;
  },

  async create(apInvoiceCreateCommand: APInvoiceHeaderCreateCommand, token: string): Promise<ApiResponse<APInvoiceHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<APInvoiceHeaderDto>>(`/api/v1/APInvoice/CreateAPInvoice`, apInvoiceCreateCommand);
    return response.data;
  },

  async update(apInvoiceEditCommand: APInvoiceHeaderEditCommand, token: string): Promise<ApiResponse<APInvoiceHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<APInvoiceHeaderDto>>(`/api/v1/APInvoice/UpdateAPInvoice`, apInvoiceEditCommand);
    return response.data;
  },

  async delete(apInvoiceDeleteCommand: APInvoiceHeaderDeleteCommand, token: string): Promise<ApiResponse<APInvoiceHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<APInvoiceHeaderDto>>(`/api/v1/APInvoice/DeleteAPInvoice`, apInvoiceDeleteCommand);
    return response.data;
  },

  // Line operations
  async createLine(apInvoiceLineCreateCommand: APInvoiceLineCreateCommand, token: string): Promise<ApiResponse<APInvoiceLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<APInvoiceLineDto>>(`/api/v1/APInvoice/CreateAPInvoiceLine`, apInvoiceLineCreateCommand);
    return response.data;
  },

  async updateLine(apInvoiceLineEditCommand: APInvoiceLineEditCommand, token: string): Promise<ApiResponse<APInvoiceLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<APInvoiceLineDto>>(`/api/v1/APInvoice/UpdateAPInvoiceLine`, apInvoiceLineEditCommand);
    return response.data;
  },

  async deleteLine(apInvoiceLineDeleteCommand: APInvoiceLineDeleteCommand, token: string): Promise<ApiResponse<APInvoiceLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<APInvoiceLineDto>>(`/api/v1/APInvoice/DeleteAPInvoiceLine`, apInvoiceLineDeleteCommand);
    return response.data;
  },

  // Association operations
  async associateAPInvoice(command: APInvoiceAssoicationCommand, token: string): Promise<ApiResponse<APInvoiceHeaderDto>[]> {
    const response = await axiosInstance(token).post<ApiResponse<APInvoiceHeaderDto>[]>(`/api/v1/APInvoice/AssociateAPInvoice`, command);
    return response.data;
  },

  async associateAPInvoiceLine(command: APInvoiceAssoicationCommand, token: string): Promise<ApiResponse<APInvoiceLineDto>[]> {
    const response = await axiosInstance(token).post<ApiResponse<APInvoiceLineDto>[]>(`/api/v1/APInvoice/AssociateAPInvoiceLine`, command);
    return response.data;
  },

  async associateReceivedPO(command: APInvoiceAssociatePOCommand, token: string): Promise<ApiResponse<APInvoiceLineDto>[]> {
    const response = await axiosInstance(token).post<ApiResponse<APInvoiceLineDto>[]>(`/api/v1/APInvoice/AssociateReceivedPO`, command);
    return response.data;
  },
}; 