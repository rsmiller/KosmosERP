import { CountryDto, CountryListDto, CountryEditCommand } from "@/models/country-models";
import { CountryCreateCommand, CountryDeleteCommand, CountryFindCommand } from "@/models/country-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const countryService = {
  async get(countryId: number, token: string): Promise<ApiResponse<CountryDto>> {
      const response = await axiosInstance(token).get<ApiResponse<CountryDto>>(`/api/v1/Country/GetCountry/?id=${countryId}`);
      return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<CountryDto>> {
    const response = await axiosInstance(token).get<ApiResponse<CountryDto>>(`/api/v1/Country/GetCountryByGuid/?guid=${guid}`);
    return response.data;
  },

  async getByISOAsync(guid: string, token: string): Promise<ApiResponse<CountryDto>> {
    const response = await axiosInstance(token).get<ApiResponse<CountryDto>>(`/api/v1/Country/GetCountryByISOAsync?iso3=${guid}`);
    return response.data;
  },

    async find(countryFindCommand: CountryFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<CountryListDto[]>> {
      const response = await axiosInstance(token).post<PagedApiResponse<CountryListDto[]>>(`/api/v1/Country/FindCountry?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, countryFindCommand);
      return response.data;
  },

    async create(countryCreateCommand: CountryCreateCommand, token: string): Promise<ApiResponse<CountryDto>> {
      const response = await axiosInstance(token).post<ApiResponse<CountryDto>>(`/api/v1/Country/CreateCountry`, countryCreateCommand);
      return response.data;
  },

    async update(countryEditCommand: CountryEditCommand, token: string): Promise<ApiResponse<CountryDto>> {
      const response = await axiosInstance(token).put<ApiResponse<CountryDto>>(`/api/v1/Country/UpdateCountry`, countryEditCommand);
      return response.data;
  },

    async delete(countryDeleteCommand: CountryDeleteCommand, token: string): Promise<any> {
      await axiosInstance(token).post<ApiResponse<CountryDto>>(`/api/v1/Country/DeleteCountry`, countryDeleteCommand);
    },
}; 