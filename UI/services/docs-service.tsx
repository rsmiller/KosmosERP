import { PurchaseOrderHeaderDto } from "@/models/purchase-order-models";
import axiosInstance from "./axios-instance";
import { ApiResponse } from "@/models/base-models";
import { OrderHeaderDto } from "@/models/sales-order-models";
import { ShipmentHeaderDto } from "@/models/shipments-models";
import { ARInvoiceHeaderDto } from "@/models/ar-models";
import { ProductionOrderHeaderDto } from "@/models/production-orders-models";

export const docsService = {

    async getPurchaseOrderByGuid(guid: string, token: string): Promise<ApiResponse<PurchaseOrderHeaderDto>> {
        const response = await axiosInstance(token).get<ApiResponse<PurchaseOrderHeaderDto>>(`/api/v1/Docs/DocsGetPurchaseOrderByGuid/?guid=${guid}`);
        return response.data;
    },

    async getSalesOrderByGuid(guid: string, token: string): Promise<ApiResponse<OrderHeaderDto>> {
        const response = await axiosInstance(token).get<ApiResponse<OrderHeaderDto>>(`/api/v1/Docs/DocsGetOrderByGuid/?guid=${guid}`);
        return response.data;
    },

    async getShipmentByGuid(guid: string, token: string): Promise<ApiResponse<ShipmentHeaderDto>> {
        const response = await axiosInstance(token).get<ApiResponse<ShipmentHeaderDto>>(`/api/v1/Docs/DocsGetShipmentByGuid/?guid=${guid}`);
        return response.data;
    },

    async getARInvoiceByGuid(guid: string, token: string): Promise<ApiResponse<ARInvoiceHeaderDto>> {
        const response = await axiosInstance(token).get<ApiResponse<ARInvoiceHeaderDto>>(`/api/v1/Docs/DocsGetARInvoiceByGuid/?guid=${guid}`);
        return response.data;
    },

    async getProductionOrderByGuid(guid: string, token: string): Promise<ApiResponse<ProductionOrderHeaderDto>> {
        const response = await axiosInstance(token).get<ApiResponse<ProductionOrderHeaderDto>>(`/api/v1/Docs/DocsGetProductionOrderByGuid/?guid=${guid}`);
        return response.data;
    },
}; 