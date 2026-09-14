import { ProductDto, ProductListDto, ProductEditCommand } from "@/models/product-models";
import { ProductCreateCommand, ProductDeleteCommand, ProductFindCommand } from "@/models/product-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const productService = {
  async get(productId: number, token: string): Promise<ApiResponse<ProductDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ProductDto>>(`/api/v1/Product/GetProduct/?id=${productId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<ProductDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ProductDto>>(`/api/v1/Product/GetProductByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(productFindCommand: ProductFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<ProductListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<ProductListDto[]>>(`/api/v1/Product/FindProduct?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, productFindCommand);
    return response.data;
  },

  async create(productCreateCommand: ProductCreateCommand, token: string): Promise<ApiResponse<ProductDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ProductDto>>(`/api/v1/Product/CreateProduct`, productCreateCommand);
    return response.data;
  },

  async update(productEditCommand: ProductEditCommand, token: string): Promise<ApiResponse<ProductDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ProductDto>>(`/api/v1/Product/UpdateProduct`, productEditCommand);
    return response.data;
  },

  async delete(productDeleteCommand: ProductDeleteCommand, token: string): Promise<ApiResponse<ProductDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ProductDto>>(`/api/v1/Product/DeleteProduct`, productDeleteCommand);
    return response.data;
  },
}; 