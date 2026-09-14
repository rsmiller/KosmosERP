import { ContactDto, ContactListDto, ContactEditCommand } from "@/models/contact-models";
import { ContactCreateCommand, ContactDeleteCommand, ContactFindCommand } from "@/models/contact-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const contactService = {
  async get(contactId: number, token: string): Promise<ApiResponse<ContactDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ContactDto>>(`/api/v1/Contact/GetContact/?id=${contactId}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<ContactDto>> {
    const response = await axiosInstance(token).get<ApiResponse<ContactDto>>(`/api/v1/Contact/GetContactByGuid/?guid=${guid}`);
    return response.data;
  },

  async find(contactFindCommand: ContactFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<ContactListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<ContactListDto[]>>(`/api/v1/Contact/FindContact?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, contactFindCommand);
    return response.data;
  },

  async create(contactCreateCommand: ContactCreateCommand, token: string): Promise<ApiResponse<ContactDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ContactDto>>(`/api/v1/Contact/CreateContact`, contactCreateCommand);
    return response.data;
  },

  async update(contactEditCommand: ContactEditCommand, token: string): Promise<ApiResponse<ContactDto>> {
    const response = await axiosInstance(token).put<ApiResponse<ContactDto>>(`/api/v1/Contact/UpdateContact`, contactEditCommand);
    return response.data;
  },

  async delete(contactDeleteCommand: ContactDeleteCommand, token: string): Promise<ApiResponse<ContactDto>> {
    const response = await axiosInstance(token).post<ApiResponse<ContactDto>>(`/api/v1/Contact/DeleteContact`, contactDeleteCommand);
    return response.data;
  },
}; 