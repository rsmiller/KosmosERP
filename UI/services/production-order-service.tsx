import { ProductionOrderHeaderDto, ProductionOrderHeaderListDto, ProductionOrderHeaderEditCommand } from "@/models/production-orders-models";
import { ProductionOrderHeaderCreateCommand, ProductionOrderHeaderDeleteCommand, ProductionOrderHeaderFindCommand } from "@/models/production-orders-models";
import { ProductionOrderLineDto, ProductionOrderLineCreateCommand, ProductionOrderLineEditCommand, ProductionOrderLineDeleteCommand } from "@/models/production-orders-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const productionOrderService = {

  async get(productionOrderId: number, token: string): Promise<ApiResponse<ProductionOrderHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ProductionOrderHeaderDto>>(`/api/v1/ProductionOrder/GetProductionOrder?id=${productionOrderId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<ProductionOrderHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ProductionOrderHeaderDto>>(`/api/v1/ProductionOrder/GetProductionOrderByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(productionOrderFindCommand: ProductionOrderHeaderFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<ProductionOrderHeaderListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<ProductionOrderHeaderListDto[]>>(`/api/v1/ProductionOrder/FindProductionOrder?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, productionOrderFindCommand);
    return response.data;
  },

  async create(productionOrderCreateCommand: ProductionOrderHeaderCreateCommand, token: string): Promise<ApiResponse<ProductionOrderHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ProductionOrderHeaderDto>>(`/api/v1/ProductionOrder/CreateProductionOrder`, productionOrderCreateCommand);
    return response.data;
  },

  async update(productionOrderEditCommand: ProductionOrderHeaderEditCommand, token: string): Promise<ApiResponse<ProductionOrderHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ProductionOrderHeaderDto>>(`/api/v1/ProductionOrder/UpdateProductionOrder`, productionOrderEditCommand);
    return response.data;
  },

  async delete(productionOrderDeleteCommand: ProductionOrderHeaderDeleteCommand, token: string): Promise<ApiResponse<ProductionOrderHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ProductionOrderHeaderDto>>(`/api/v1/ProductionOrder/DeleteProductionOrder`, productionOrderDeleteCommand);
    return response.data;
  },

  // Line operations
  async createLine(productionOrderLineCreateCommand: ProductionOrderLineCreateCommand, token: string): Promise<ApiResponse<ProductionOrderLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ProductionOrderLineDto>>(`/api/v1/ProductionOrder/CreateProductionOrderLine`, productionOrderLineCreateCommand);
    return response.data;
  },

  async updateLine(productionOrderLineEditCommand: ProductionOrderLineEditCommand, token: string): Promise<ApiResponse<ProductionOrderLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ProductionOrderLineDto>>(`/api/v1/ProductionOrder/UpdateProductionOrderLine`, productionOrderLineEditCommand);
    return response.data;
  },

  async deleteLine(productionOrderLineDeleteCommand: ProductionOrderLineDeleteCommand, token: string): Promise<ApiResponse<ProductionOrderLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ProductionOrderLineDto>>(`/api/v1/ProductionOrder/DeleteProductionOrderLine`, productionOrderLineDeleteCommand);
    return response.data;
  },
}; 