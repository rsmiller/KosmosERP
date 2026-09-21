import axiosInstance from "./axios-instance";
import { ApiResponse } from "@/models/base-models";
import { ReportCategory, ReportCatalogItem, ReportFormat, ReportOption } from "@/models/report-models";

export const reportsService = {
  // The catalog of general reports grouped by category, used to build the sidebar.
  async getCatalog(token: string): Promise<ApiResponse<ReportCategory[]>> {
    const response = await axiosInstance(token).get<ApiResponse<ReportCategory[]>>(
      `/api/v1/Reports/Catalog`
    );
    return response.data;
  },

  // Options for a dropdown parameter (e.g. product categories for Sales by Product).
  async getOptions(endpoint: string, token: string): Promise<ApiResponse<ReportOption[]>> {
    const response = await axiosInstance(token).get<ApiResponse<ReportOption[]>>(`/${endpoint}`);
    return response.data;
  },

  // Requests a rendered report and returns it as a Blob (HTML or PDF).
  // Only parameter values that are actually filled in are sent; the API supplies
  // sensible defaults for the rest.
  async generateReport(
    report: ReportCatalogItem,
    values: Record<string, string>,
    format: ReportFormat,
    token: string
  ): Promise<Blob> {
    const query = new URLSearchParams();
    for (const param of report.parameters) {
      const value = values[param.name];
      if (value !== undefined && value !== null && value !== "") {
        query.append(param.name, value);
      }
    }
    query.append("format", format);

    const response = await axiosInstance(token).get(`/${report.endpoint}?${query.toString()}`, {
      responseType: "blob",
    });
    return response.data as Blob;
  },
};
