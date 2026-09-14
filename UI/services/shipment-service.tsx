import { ShipmentHeaderDto, ShipmentHeaderListDto, ShipmentHeaderEditCommand, vw_ReadyToShip } from "@/models/shipments-models";
import { ShipmentHeaderCreateCommand, ShipmentHeaderDeleteCommand, ShipmentHeaderFindCommand } from "@/models/shipments-models";
import { ShipmentLineDto, ShipmentLineCreateCommand, ShipmentLineEditCommand, ShipmentLineDeleteCommand } from "@/models/shipments-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const shipmentService = {
  // Header operations
  async get(shipmentId: number, token: string): Promise<ApiResponse<ShipmentHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ShipmentHeaderDto>>(`/api/v1/Shipment/GetShipmentHeader?id=${shipmentId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<ShipmentHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ShipmentHeaderDto>>(`/api/v1/Shipment/GetShipmentHeaderByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(shipmentFindCommand: ShipmentHeaderFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<ShipmentHeaderListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<ShipmentHeaderListDto[]>>(`/api/v1/Shipment/FindShipmentHeader?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, shipmentFindCommand);
    return response.data;
  },

  async getReadyToShip(token: string): Promise<ApiResponse<vw_ReadyToShip[]>> {
    const response = await axiosInstance(token).get<ApiResponse<vw_ReadyToShip[]>>(`/api/v1/Shipment/GetReadyToShip`);
    return response.data;
  },
  
  async create(shipmentCreateCommand: ShipmentHeaderCreateCommand, token: string): Promise<ApiResponse<ShipmentHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ShipmentHeaderDto>>(`/api/v1/Shipment/CreateShipmentHeader`, shipmentCreateCommand);
    return response.data;
  },

  async update(shipmentEditCommand: ShipmentHeaderEditCommand, token: string): Promise<ApiResponse<ShipmentHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ShipmentHeaderDto>>(`/api/v1/Shipment/UpdateShipmentHeader`, shipmentEditCommand);
    return response.data;
  },

  async delete(shipmentDeleteCommand: ShipmentHeaderDeleteCommand, token: string): Promise<ApiResponse<ShipmentHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ShipmentHeaderDto>>(`/api/v1/Shipment/DeleteShipmentHeader`, shipmentDeleteCommand);
    return response.data;
  },

  // Line operations
  async createLine(shipmentLineCreateCommand: ShipmentLineCreateCommand, token: string): Promise<ApiResponse<ShipmentLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ShipmentLineDto>>(`/api/v1/ShipmentLine/CreateShipmentLine`, shipmentLineCreateCommand);
    return response.data;
  },

  async updateLine(shipmentLineEditCommand: ShipmentLineEditCommand, token: string): Promise<ApiResponse<ShipmentLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ShipmentLineDto>>(`/api/v1/ShipmentLine/UpdateShipmentLine`, shipmentLineEditCommand);
    return response.data;
  },

  async deleteLine(shipmentLineDeleteCommand: ShipmentLineDeleteCommand, token: string): Promise<ApiResponse<ShipmentLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ShipmentLineDto>>(`/api/v1/ShipmentLine/DeleteShipmentLine`, shipmentLineDeleteCommand);
    return response.data;
  },
}; 