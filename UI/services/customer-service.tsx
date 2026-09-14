import { CustomerDto, CustomerListDto, CustomerEditCommand } from "@/models/customer-models";
import { CustomerCreateCommand, CustomerDeleteCommand, CustomerFindCommand } from "@/models/customer-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const customerService = {
  async get(customerId: number, token: string): Promise<ApiResponse<CustomerDto>> {
    const response = await axiosInstance(token).get<ApiResponse<CustomerDto>>(`/api/v1/Customer/GetCustomer/?id=${customerId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<CustomerDto>> {
    const response = await axiosInstance(token).get<ApiResponse<CustomerDto>>(`/api/v1/Customer/GetCustomerByGuid/?guid=${guid}`);
    return response.data;
  },

  async getPaymentTerms(token: string): Promise<any[]> {
    const response = await axiosInstance(token).get<any[]>(`/api/v1/Customer/GetPaymentTerms`);
    return response.data;
  },

  async getShippingMethods(token: string): Promise<any[]> {
    const response = await axiosInstance(token).get<any[]>(`/api/v1/Customer/GetShippingMethods`);
    return response.data;
  },

  async getPayMethods(token: string): Promise<any[]> {
    const response = await axiosInstance(token).get<any[]>(`/api/v1/Customer/GetPayMethods`);
    return response.data;
  },

  async find(customerFindCommand: CustomerFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<CustomerListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<CustomerListDto[]>>(`/api/v1/Customer/FindCustomer?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, customerFindCommand);
    return response.data;
  },

  async create(customerCreateCommand: CustomerCreateCommand, token: string): Promise<ApiResponse<CustomerDto>> {
    const response = await axiosInstance(token).post<ApiResponse<CustomerDto>>(`/api/v1/Customer/CreateCustomer`, customerCreateCommand);
    return response.data;
  },

  async update(customerEditCommand: CustomerEditCommand, token: string): Promise<ApiResponse<CustomerDto>> {
    const response = await axiosInstance(token).put<ApiResponse<CustomerDto>>(`/api/v1/Customer/UpdateCustomer`, customerEditCommand);
    return response.data;
  },

  async delete(customerDeleteCommand: CustomerDeleteCommand, token: string): Promise<ApiResponse<CustomerDto>> {
    const response = await axiosInstance(token).post<ApiResponse<CustomerDto>>(`/api/v1/Customer/DeleteCustomer`, customerDeleteCommand);
    return response.data;
  },
}; 