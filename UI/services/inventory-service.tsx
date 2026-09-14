import { InventoryDto } from "@/models/inventory-models";
import axiosInstance from "./axios-instance";
import { ApiResponse } from "@/models/base-models";

export const inventoryService = {
  async getCounts(token: string): Promise<ApiResponse<InventoryDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<InventoryDto[]>>(`/api/v1/Inventory/GetCounts`);
    return response.data;
  },

  async rebuildCounts(token: string): Promise<ApiResponse<boolean>> {
    const response = await axiosInstance(token).post<ApiResponse<boolean>>(`/api/v1/Inventory/RebuildCounts`);
    return response.data;
  },

  // Additional methods that could be added when the backend supports them:
  // async get(inventoryId: number): Promise<ApiResponse<InventoryDto>> {
  //   const response = await axiosInstance.get<ApiResponse<InventoryDto>>(`/api/v1/Inventory/GetInventory/?id=${inventoryId}`);
  //   return response.data;
  // },

  // async getByGuid(guid: string): Promise<ApiResponse<InventoryDto>> {
  //   const response = await axiosInstance.get<ApiResponse<InventoryDto>>(`/api/v1/Inventory/GetInventoryByGuid/?guid=${guid}`);
  //   return response.data;
  // },

  // async find(inventoryFindCommand: InventoryFindCommand, start: number = 1, resultCount: number = 20, sortOrder: string = "id-asc"): Promise<ApiResponse<InventoryListDto[]>> {
  //   const response = await axiosInstance.post<ApiResponse<InventoryListDto[]>>(`/api/v1/Inventory/FindInventory?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, inventoryFindCommand);
  //   return response.data;
  // },

  // async create(inventoryCreateCommand: InventoryCreateCommand): Promise<ApiResponse<InventoryDto>> {
  //   const response = await axiosInstance.post<ApiResponse<InventoryDto>>(`/api/v1/Inventory/CreateInventory`, inventoryCreateCommand);
  //   return response.data;
  // },

  // async update(inventoryEditCommand: InventoryEditCommand): Promise<ApiResponse<InventoryDto>> {
  //   const response = await axiosInstance.put<ApiResponse<InventoryDto>>(`/api/v1/Inventory/UpdateInventory`, inventoryEditCommand);
  //   return response.data;
  // },

  // async delete(inventoryDeleteCommand: InventoryDeleteCommand): Promise<ApiResponse<InventoryDto>> {
  //   const response = await axiosInstance.post<ApiResponse<InventoryDto>>(`/api/v1/Inventory/DeleteInventory`, inventoryDeleteCommand);
  //   return response.data;
  // },
}; 