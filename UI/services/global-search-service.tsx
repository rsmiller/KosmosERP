import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";
import { GlobalSearchFindCommand, GlobalSearchResultDto } from "@/models/global-search-models";

export const globalSearchService = {
  async search(command: GlobalSearchFindCommand, token: string): Promise<ApiResponse<GlobalSearchResultDto>> {
    const response = await axiosInstance(token).post<ApiResponse<GlobalSearchResultDto>>(`/api/v1/GlobalSearch/Search`, command);
    return response.data;
  },
}; 