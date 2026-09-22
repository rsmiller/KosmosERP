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

  // Renders the Purchase Order document report for a single PO, selected by guid.
  async getPurchaseOrderReport(
    purchaseOrderGuid: string,
    format: ReportFormat,
    token: string
  ): Promise<Blob> {
    const query = new URLSearchParams({ purchase_order_guid: purchaseOrderGuid, format });
    const response = await axiosInstance(token).get(`/api/v1/Reports/PurchaseOrder?${query.toString()}`, {
      responseType: "blob",
    });
    return response.data as Blob;
  },

  // Renders the AR Invoice document report for a single invoice, selected by guid.
  async getArInvoiceReport(
    arInvoiceGuid: string,
    format: ReportFormat,
    token: string
  ): Promise<Blob> {
    const query = new URLSearchParams({ ar_invoice_guid: arInvoiceGuid, format });
    const response = await axiosInstance(token).get(`/api/v1/Reports/ArInvoice?${query.toString()}`, {
      responseType: "blob",
    });
    return response.data as Blob;
  },

  // Renders the Sales Order Acknowledgement document report for a single order, selected by guid.
  async getSalesOrderAcknowledgementReport(
    orderGuid: string,
    format: ReportFormat,
    token: string
  ): Promise<Blob> {
    const query = new URLSearchParams({ order_guid: orderGuid, format });
    const response = await axiosInstance(token).get(`/api/v1/Reports/SalesOrderAcknowledgement?${query.toString()}`, {
      responseType: "blob",
    });
    return response.data as Blob;
  },

  // Renders the Packing Slip document report for a single shipment, selected by id.
  async getPackingSlipReport(
    shipmentId: number,
    format: ReportFormat,
    token: string
  ): Promise<Blob> {
    const query = new URLSearchParams({ shipment_id: String(shipmentId), format });
    const response = await axiosInstance(token).get(`/api/v1/Reports/PackingSlip?${query.toString()}`, {
      responseType: "blob",
    });
    return response.data as Blob;
  },

  // Opens a rendered report in a new tab. Must be called directly from a click handler:
  // the tab is opened synchronously so popup blockers treat it as user-initiated, then
  // pointed at the report once it has been rendered.
  async openInNewTab(loadReport: () => Promise<Blob>): Promise<void> {
    const reportWindow = window.open("", "_blank");

    try {
      const blob = await loadReport();
      const url = URL.createObjectURL(blob);

      if (reportWindow) {
        reportWindow.location.href = url;
      } else {
        window.open(url, "_blank");
      }

      window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
    } catch (error) {
      console.error("Error generating report:", error);
      reportWindow?.close();
    }
  },
};
