
import { SettingsDto, SettingsListDto, SettingsEditCommand } from "@/models/settings-models";
import { SettingsCreateCommand, SettingsDeleteCommand, SettingsFindCommand } from "@/models/settings-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const settingsService = {
	async getBaseSettings(token: string): Promise<ApiResponse<SettingsDto>> {
		const response = await axiosInstance(token).get<ApiResponse<SettingsDto>>(`/api/v1/Settings/GetBaseSettings`);
		return response.data;
	},

	async get(settingsId: number, token: string): Promise<ApiResponse<SettingsDto>> {
		const response = await axiosInstance(token).get<ApiResponse<SettingsDto>>(`/api/v1/Settings/GetSettings/?id=${settingsId}`);
		return response.data;
	},

	async getByGuid(guid: string, token: string): Promise<ApiResponse<SettingsDto>> {
		const response = await axiosInstance(token).get<ApiResponse<SettingsDto>>(`/api/v1/Settings/GetSettingsByGuid/?guid=${guid}`);
		return response.data;
	},

	async find(settingsFindCommand: SettingsFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<SettingsListDto[]>> {
		const response = await axiosInstance(token).post<PagedApiResponse<SettingsListDto[]>>(`/api/v1/Settings/FindSettings?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, settingsFindCommand);
		return response.data;
	},

	async create(settingsCreateCommand: SettingsCreateCommand, token: string): Promise<ApiResponse<SettingsDto>> {
		const response = await axiosInstance(token).post<ApiResponse<SettingsDto>>(`/api/v1/Settings/CreateSettings`, settingsCreateCommand);
		return response.data;
	},

	async update(settingsEditCommand: SettingsEditCommand, token: string): Promise<ApiResponse<SettingsDto>> {
		const response = await axiosInstance(token).put<ApiResponse<SettingsDto>>(`/api/v1/Settings/UpdateSettings`, settingsEditCommand);
		return response.data;
	},

	async delete(settingsDeleteCommand: SettingsDeleteCommand, token: string): Promise<ApiResponse<SettingsDto>> {
		const response = await axiosInstance(token).post<ApiResponse<SettingsDto>>(`/api/v1/Settings/DeleteSettings`, settingsDeleteCommand);
		return response.data;
	},
};
