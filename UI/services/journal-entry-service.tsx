import { 
    JournalEntryHeaderDto, 
    JournalEntryHeaderListDto, 
    JournalEntryLineDto,
    JournalEntryHeaderCreateCommand, 
    JournalEntryHeaderEditCommand, 
    JournalEntryHeaderDeleteCommand,
    JournalEntryLineCreateCommand,
    JournalEntryLineEditCommand,
    JournalEntryLineDeleteCommand,
    JournalEntryHeaderFindCommand,
    JournalEntryPostCommand,
    JournalEntryReverseCommand
} from "@/models/journal-entry-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const journalEntryService = {
  async get(id: number, token: string): Promise<ApiResponse<JournalEntryHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<JournalEntryHeaderDto>>(`/api/v1/JournalEntry/GetJournalEntry/?id=${id}`);
    return response.data;
  },

  async getByGuid(guid: string, token: string): Promise<ApiResponse<JournalEntryHeaderDto>> {
    const response = await axiosInstance(token).get<ApiResponse<JournalEntryHeaderDto>>(`/api/v1/JournalEntry/GetJournalEntryByGuid/?guid=${guid}`);
    return response.data;
  },

  async getLine(id: number, token: string): Promise<ApiResponse<JournalEntryLineDto>> {
    const response = await axiosInstance(token).get<ApiResponse<JournalEntryLineDto>>(`/api/v1/JournalEntry/GetJournalEntryLine/?id=${id}`);
    return response.data;
  },

  async getUnposted(token: string): Promise<ApiResponse<JournalEntryHeaderListDto[]>> {
    const response = await axiosInstance(token).get<ApiResponse<JournalEntryHeaderListDto[]>>(`/api/v1/JournalEntry/GetUnpostedEntries`);
    return response.data;
  },

  async validateBalance(journalEntryId: number, token: string): Promise<ApiResponse<boolean>> {
    const response = await axiosInstance(token).post<ApiResponse<boolean>>(`/api/v1/JournalEntry/ValidateJournalBalance/?journalEntryId=${journalEntryId}`);
    return response.data;
  },

  async find(findCommand: JournalEntryHeaderFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "entry_date-desc"): Promise<PagedApiResponse<JournalEntryHeaderListDto[]>> {
    const response = await axiosInstance(token).post<PagedApiResponse<JournalEntryHeaderListDto[]>>(`/api/v1/JournalEntry/FindJournalEntry?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, findCommand);
    return response.data;
  },

  async create(createCommand: JournalEntryHeaderCreateCommand, token: string): Promise<ApiResponse<JournalEntryHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<JournalEntryHeaderDto>>(`/api/v1/JournalEntry/CreateJournalEntry`, createCommand);
    return response.data;
  },

  async createLine(createCommand: JournalEntryLineCreateCommand, token: string): Promise<ApiResponse<JournalEntryLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<JournalEntryLineDto>>(`/api/v1/JournalEntry/CreateJournalEntryLine`, createCommand);
    return response.data;
  },

  async update(editCommand: JournalEntryHeaderEditCommand, token: string): Promise<ApiResponse<JournalEntryHeaderDto>> {
    const response = await axiosInstance(token).put<ApiResponse<JournalEntryHeaderDto>>(`/api/v1/JournalEntry/UpdateJournalEntry`, editCommand);
    return response.data;
  },

  async updateLine(editCommand: JournalEntryLineEditCommand, token: string): Promise<ApiResponse<JournalEntryLineDto>> {
    const response = await axiosInstance(token).put<ApiResponse<JournalEntryLineDto>>(`/api/v1/JournalEntry/UpdateJournalEntryLine`, editCommand);
    return response.data;
  },

  async delete(deleteCommand: JournalEntryHeaderDeleteCommand, token: string): Promise<ApiResponse<JournalEntryHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<JournalEntryHeaderDto>>(`/api/v1/JournalEntry/DeleteJournalEntry`, deleteCommand);
    return response.data;
  },

  async deleteLine(deleteCommand: JournalEntryLineDeleteCommand, token: string): Promise<ApiResponse<JournalEntryLineDto>> {
    const response = await axiosInstance(token).post<ApiResponse<JournalEntryLineDto>>(`/api/v1/JournalEntry/DeleteJournalEntryLine`, deleteCommand);
    return response.data;
  },

  async post(postCommand: JournalEntryPostCommand, token: string): Promise<ApiResponse<JournalEntryHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<JournalEntryHeaderDto>>(`/api/v1/JournalEntry/PostJournalEntry`, postCommand);
    return response.data;
  },

  async reverse(reverseCommand: JournalEntryReverseCommand, token: string): Promise<ApiResponse<JournalEntryHeaderDto>> {
    const response = await axiosInstance(token).post<ApiResponse<JournalEntryHeaderDto>>(`/api/v1/JournalEntry/ReverseJournalEntry`, reverseCommand);
    return response.data;
  },
};
