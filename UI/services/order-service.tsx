import { OrderHeaderDto, OrderHeaderListDto, OrderHeaderEditCommand } from "@/models/sales-order-models";
import { OrderHeaderCreateCommand, OrderHeaderDeleteCommand, OrderHeaderFindCommand } from "@/models/sales-order-models";
import { OrderLineDto, OrderLineCreateCommand, OrderLineEditCommand, OrderLineDeleteCommand } from "@/models/sales-order-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const orderService = {
  // Header operations
  async get(orderId: number, token: string): Promise<ApiResponse<OrderHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<OrderHeaderDto>>(`/api/v1/Order/GetOrder/?id=${orderId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<OrderHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<OrderHeaderDto>>(`/api/v1/Order/GetOrderByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(orderFindCommand: OrderHeaderFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<OrderHeaderListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<OrderHeaderListDto[]>>(`/api/v1/Order/FindOrder?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, orderFindCommand);
    return response.data;
  },

  async create(orderCreateCommand: OrderHeaderCreateCommand, token: string): Promise<ApiResponse<OrderHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<OrderHeaderDto>>(`/api/v1/Order/CreateOrder`, orderCreateCommand);
    return response.data;
  },

  async update(orderEditCommand: OrderHeaderEditCommand, token: string): Promise<ApiResponse<OrderHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<OrderHeaderDto>>(`/api/v1/Order/UpdateOrder`, orderEditCommand);
    return response.data;
  },

  async delete(orderDeleteCommand: OrderHeaderDeleteCommand, token: string): Promise<ApiResponse<OrderHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<OrderHeaderDto>>(`/api/v1/Order/DeleteOrder`, orderDeleteCommand);
    return response.data;
  },

  // Line operations
  async createLine(orderLineCreateCommand: OrderLineCreateCommand, token: string): Promise<ApiResponse<OrderLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<OrderLineDto>>(`/api/v1/Order/CreateOrderLine`, orderLineCreateCommand);
    return response.data;
  },

  async updateLine(orderLineEditCommand: OrderLineEditCommand, token: string): Promise<ApiResponse<OrderLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<OrderLineDto>>(`/api/v1/OrderLine/UpdateOrderLine`, orderLineEditCommand);
    return response.data;
  },

  async deleteLine(orderLineDeleteCommand: OrderLineDeleteCommand, token: string): Promise<ApiResponse<OrderLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<OrderLineDto>>(`/api/v1/Order/DeleteOrderLine`, orderLineDeleteCommand);
    return response.data;
  },
}; 