import { KeyValueCreateCommand, KeyValueDeleteCommand, KeyValueDto, KeyValueEditCommand, KeyValueFindCommand, ModuleObjectDto } from "@/models/key-value-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const keyValueService = {
  async GetDtoByModule(module_id: string, token: string): Promise<ApiResponse<KeyValueDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<KeyValueDto[]>>(`/api/v1/KeyValue/GetKeyValuesByModule?module_id=${module_id}`);
    return response.data;
  },

  async getModuleInfo(token: string): Promise<ApiResponse<ModuleObjectDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<ModuleObjectDto[]>>(`/api/v1/KeyValue/GetModuleInfo`);

    return response.data;
  },

  async getModuleAndKeyValueTypes(token: string): Promise<ApiResponse<ModuleObjectDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<ModuleObjectDto[]>>(`/api/v1/KeyValue/GetModuleInfo`);

    if(response.data != undefined && response.data.data)
    {
      response.data.data.push(
        {
          module_id: "83156a35-d140-4442-8fbf-699658bf65e9",
          module_name: "Payment Method"
        },
        {
          module_id: "9da95117-2792-44e5-996a-e91a244b0384",
          module_name: "Shipping Method"
        },
        {
          module_id: "93bf02ec-5578-4aa4-a45b-f82962adf4bd",
          module_name: "Payment Term"
        },
        {
          module_id: "f6e28b05-265d-4416-b5fd-48399036493a",
          module_name: "Product Category"
        },
        {
          module_id: "f157469e-5e5c-4a5b-b071-89a28b2a0310",
          module_name: "Production Status"
        },
        {
          module_id: "0c3959c3-15dc-44ab-8e2c-9b9e2773e65f",
          module_name: "Opportunity Stage"
        },
        {
          module_id: "2a2d1004-5283-40ef-96fd-8cc30c65cefa",
          module_name: "Frieght Company"
        },
        {
          module_id: "eea9df53-1b36-41ea-94fa-31420315ff60",
          module_name: "GL Account"
        },
        {
          module_id: "97dd4b13-ff15-47ff-955d-5e957644cffd",
          module_name: "Production Order Status"
        },
        {
          module_id: "78c4861d-1252-4cac-9461-0e1e0399cd83",
          module_name: "PO Category"
        },
        {
          module_id: "416786e0-47b3-440a-90da-b7036d72b1f7",
          module_name: "Transaction Type"
        },
        {
          module_id: "dae2593c-678b-4f6d-9c84-f4f74e066428",
          module_name: "Vendor Category"
        }
      );
    }

    return response.data;
  },

  async find(leadFindCommand: KeyValueFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<KeyValueDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<KeyValueDto[]>>(`/api/v1/KeyValue/FindKeyValue?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, leadFindCommand);
    return response.data;
  },

  async create(leadCreateCommand: KeyValueCreateCommand, token: string): Promise<ApiResponse<KeyValueDto>> {
    const response = await axiosInstance(token).post<ApiResponse<KeyValueDto>>(`/api/v1/KeyValue/CreateKeyValue`, leadCreateCommand);
    return response.data;
  },

  async update(leadEditCommand: KeyValueEditCommand, token: string): Promise<ApiResponse<KeyValueDto>> {
    const response = await axiosInstance(token).put<ApiResponse<KeyValueDto>>(`/api/v1/KeyValue/UpdateKeyValue`, leadEditCommand);
    return response.data;
  },

  async delete(leadDeleteCommand: KeyValueDeleteCommand, token: string): Promise<ApiResponse<KeyValueDto>> {
    const response = await axiosInstance(token).post<ApiResponse<KeyValueDto>>(`/api/v1/KeyValue/DeleteKeyValue`, leadDeleteCommand);
    return response.data;
  },
}; 