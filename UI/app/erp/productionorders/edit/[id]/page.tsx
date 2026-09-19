
"use client"

import '../../../../styles/page.component.css';

import { useEffect, useRef, useState } from "react";
import { useParams, useRouter } from 'next/navigation';
import SessionStorage from '@/components/session-storage';
import { ProductionOrderHeaderDto, ProductionOrderHeaderEditCommand, ProductionOrderLineDto } from '@/models/production-orders-models';
import { useForm } from 'react-hook-form';
import { productionOrderService } from '@/services/production-order-service';
import { PurchaseOrderLineDto } from '@/models/purchase-order-models';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Grid, GridItem } from '@chakra-ui/react';
import { AgGridReact } from 'ag-grid-react';
import PageActionsComponent from '@/components/page-actions';
import ProductionStatusCellEditor from '@/components/ag-grid/production-status-cell-editor';
import ProductionStatusCellRenderer from '@/components/ag-grid/production-status-cell-renderer';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditProductionOrderPage() {
    const auth = useAuth();
    const router = useRouter();
    const params = useParams();
    const [hasAccess, setHasAccess] = useState(true);
    const [hasEditPermission, setHasEditPermission] = useState(false);


    const [productionOrder, setProductionOrder] = useState<ProductionOrderHeaderDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [saveable, canSave] = useState(false);
    const [successSaved, setSuccessSaved] = useState(false);
    const [failedSaved, setFailedSaved] = useState(false);
    const [completedOrDisabled, setCompletedOrDisabled] = useState(false);
    
    const {
        formState: { errors, isValid },
        setValue,
    } = useForm<ProductionOrderHeaderEditCommand>();

    const [rowData, setRowData] = useState<PurchaseOrderLineDto[]>([]);
    const hasInitialized = useRef(false);

    const loadSalesOrder = async () => {
        try {
            setLoading(true);
            const salesOrderId = String(params.id);
            
            if (salesOrderId == "") {
                setError('Invalid production order ID');
                return;
            }

            const response = await productionOrderService.getByGuid(salesOrderId, auth.token || "");
            //console.log(response)
            if (response.success && response.data) {
                setProductionOrder(response.data);
                
                // Set form values with loaded data
                setValue('id', response.data.id as number);
                setValue('status', response.data.status);
                setValue('priority_id', response.data.priority_id);
                setValue('planned_start_date', response.data.planned_start_date || undefined);
                setValue('planned_complete_date', response.data.planned_complete_date || undefined);

                if(response.data.production_order_lines) {
                setRowData(response.data.production_order_lines);
                }
                
                // If this is completed we can't edit this
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

        // Check Edit permission
        const canEdit = permissionsService.HasPermission(
          ERPModules.ProductionOrderModule,
          ERPModulePermission.Edit,
          realmRoles
        );
        setHasEditPermission(canEdit);

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        loadSalesOrder();
    }, [params.id, setValue, auth.authenticated]);


    const CheckFormValidity = () => {

    }

    const [colDefs, setColDefs] = useState<ColDef<ProductionOrderLineDto>[]>([
        { field: "order_line.product_name", headerName: "Product Name"},
        { field: "quantity", headerName: "Quantity" },
        { field: "production_lead_minutes", headerName: "Production Lead" },
        { field: "started_on", headerName: "Started On", cellRenderer: DateOnlyRender },
        { field: "is_complete", headerName: "Is Complete" },
        { 
            field: "status", 
            headerName: "Status",
            editable: !completedOrDisabled,
            cellEditor: ProductionStatusCellEditor,
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

    if (!productionOrder) {
        return <div>Production order not found</div>;
    }

    const handleSaveClick = async () => {
        setSuccessSaved(false);
    }

    const onCellValueChanged = (event: any) => {
        // Update the rowData state to reflect changes
        const updatedRowData = [...rowData];
        const rowIndex = event.rowIndex;
        if (rowIndex >= 0 && rowIndex < updatedRowData.length) {
            updatedRowData[rowIndex] = { ...updatedRowData[rowIndex], [event.colDef.field]: event.newValue };
            setRowData(updatedRowData);
        }
    };


    return (
    <form onChange={CheckFormValidity}>
      <Grid
            templateColumns="repeat(5, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
          >
            <GridItem colSpan={4} >
              <h1>Production Order - {productionOrder.order_header?.order_number}</h1>
            </GridItem>


            <GridItem colSpan={5} >
              <div style={{ width: "100%", height: "500px" }}>
                <AgGridReact
                    rowData={rowData}
                    columnDefs={colDefs}
                    defaultColDef={defaultColDef}
                    context={{ completedOrDisabled }}
                    onCellValueChanged={onCellValueChanged}
                  />
                </div>
            </GridItem>

            <GridItem colSpan={5} >
              <PageActionsComponent 
                onSave={handleSaveClick} 
                canDelete={false}
                canSave={!saveable || !hasEditPermission}
                successSaved={successSaved}
                failedSaved={failedSaved}
              />
            </GridItem>
        </Grid>
      </form>
  )
}

export default EditProductionOrderPage;