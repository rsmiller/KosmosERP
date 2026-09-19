"use client"

import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';

import { ProductionOrderHeaderDto, ProductionOrderLineDto } from "@/models/production-orders-models";
import { OrderHeaderDto } from "@/models/sales-order-models";
import { productionOrderService } from "@/services/production-order-service";
import { DataList, For, Grid, GridItem } from "@chakra-ui/react";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { AgGridReact } from 'ag-grid-react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import ProductionStatusCellRenderer from '@/components/ag-grid/production-status-cell-renderer';
import { format } from 'date-fns';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { PurchaseOrderLineDto } from '@/models/purchase-order-models';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import PageActionsComponent from '@/components/page-actions';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewProductionOrderPage() {
    const auth = useAuth();
    const router = useRouter();
    const params = useParams();
    const [hasAccess, setHasAccess] = useState(true);

    const [loading, setLoading] = useState(true);

    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [completedOrDisabled, setCompletedOrDisabled] = useState(false);

    const [productionOrder, setProductionOrder] = useState<ProductionOrderHeaderDto | null>(null);
    const [orderModel, setOrderModel] = useState<OrderHeaderDto>();

    const [rowData, setRowData] = useState<ProductionOrderLineDto[]>([]);
    
    const hasInitialized = useRef(false);


    const loadProductionOrder = async () => {
        try {
            setLoading(true);
            const productionOrderId = String(params.id);
            
            if (productionOrderId == "") {
                setError('Invalid production order ID');
                return;
            }

            const response = await productionOrderService.getByGuid(productionOrderId, auth.token || "");
            console.log(response)

            if (response.success && response.data) {
                setProductionOrder(response.data);
                setOrderModel(response.data.order_header);

                if(response.data.production_order_lines) {
                    setRowData(response.data.production_order_lines);
                }
                

                if(response.data.is_complete) {
                    setCompletedOrDisabled(true);
                    canSave(false);
                } else {
                    CheckFormValidity();
                }

            } else {
                setError('Failed to load production order');
            }
        } catch (err) {
            console.error('Error loading production order:', err);
            setError('Error loading production order');
        } finally {
        setLoading(false);
        }
    };

    useEffect(() => {
        if(String(params.id) == undefined || String(params.id) == "")
            return;

        if(auth.authenticated == false) return;
        
        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ProductionOrderModule,
          ERPModulePermission.Read,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        if(hasInitialized.current)
            return;
        
        hasInitialized.current = true;

        loadProductionOrder();
    }, [params.id, auth.authenticated]);

    const formatDateString = (dateString: string | undefined): string => {
        if(!dateString || dateString.trim() === ""){
            return "";
        }

        return format(dateString || "", 'MM-dd-yyyy');
    }

    const CheckFormValidity = () => {
    
    }

    const Print = () => {
        window.open(`/docs/api/?url=${encodeURIComponent('/docs/productionorder/' + productionOrder?.guid)}`, "_blank")
    }

    const [colDefs, setColDefs] = useState<ColDef<ProductionOrderLineDto>[]>([
        { field: "order_line.product_name", headerName: "Product Name"},
        { field: "quantity", headerName: "Quantity" },
        { field: "production_lead_minutes", headerName: "Production Lead" },
        { field: "started_on", headerName: "Started On", cellRenderer: DateOnlyRender },
        { field: "is_complete", headerName: "Is Complete", editable: false },
        { 
            field: "status", 
            headerName: "Status",
            editable: false,
            cellRenderer: ProductionStatusCellRenderer
        },
    ]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: true,
        sortable: true,
    };

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    if (loading) {
        return <div>Loading production order...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    return (
        <div>
            <Grid
                templateColumns="repeat(8, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={2}>
                    <div><h3>Order</h3></div>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Number</DataList.ItemLabel>
                        <DataList.ItemValue>{orderModel?.order_number}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>PO Number</DataList.ItemLabel>
                        <DataList.ItemValue>{orderModel?.po_number}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Order Date</DataList.ItemLabel>
                        <DataList.ItemValue>{formatDateString(orderModel?.order_date)}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                </GridItem>
                <GridItem colSpan={2}>
                    <div><h3>&nbsp;</h3></div>
                    <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                        <DataList.Item>
                        <DataList.ItemLabel>Customer</DataList.ItemLabel>
                        <DataList.ItemValue>{orderModel?.customer_name}</DataList.ItemValue>
                        </DataList.Item>
                    </DataList.Root>
                </GridItem>
                <GridItem colSpan={4}></GridItem>
                <GridItem colSpan={8} >
                    <div style={{ width: "100%", height: "500px" }}>
                        <AgGridReact
                            rowData={rowData}
                            columnDefs={colDefs}
                            defaultColDef={defaultColDef}
                            context={{ completedOrDisabled }}
                            />
                    </div>
                </GridItem>

                <GridItem colSpan={8}>
                    <PageActionsComponent 
                        canSave={false}
                        saveText="Print Production Order"
                        canDelete={false}
                        onSave={Print} 
                        onDelete={() => {}}
                    />
                </GridItem>
            </Grid>
        </div>
    );
}

export default ViewProductionOrderPage;
