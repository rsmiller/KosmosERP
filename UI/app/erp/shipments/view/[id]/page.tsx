"use client";

import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';


import { ShipmentHeaderDto, ShipmentHeaderEditCommand, ShipmentLineDto } from '@/models/shipments-models';
import { shipmentService } from '@/services/shipment-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { DataList, Grid, GridItem } from '@chakra-ui/react';
import { useAuth } from '@/lib/auth/auth-context';
import { useParams, useRouter } from 'next/navigation';
import { useEffect, useRef, useState } from 'react';
import { AgGridReact } from 'ag-grid-react';
import PageActionsComponent from '@/components/page-actions';
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community";

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewShipmentsPage() {
    const auth = useAuth();
    const router = useRouter();
    const params = useParams();
    const [hasAccess, setHasAccess] = useState(true);

    const hasInitialized = useRef(false);

    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [shipment, setShipment] = useState<ShipmentHeaderDto | null>(null);
    const [rowData, setRowData] = useState<ShipmentLineDto[]>([]);

    useEffect(() => {
    
        if(auth.authenticated == false) return;
    
        const realmRoles = auth.roles || [];
        const hasPermission = permissionsService.HasPermission(
          ERPModules.ShippingModule,
          ERPModulePermission.Read,
          realmRoles
        );
        if (!hasPermission) {
          setHasAccess(false);
          router.push('/erp');
          return;
        }

        if (hasInitialized.current) return;
        
        hasInitialized.current = true;
        
    
        loadShipment();
    }, [params.id, auth.authenticated, router]);

    const loadShipment = async () => {
        try {
            setLoading(true);
            const shipmentId = String(params.id);
            
            if (shipmentId == "") {
                setError('Invalid shipment ID');
                return;
            }

            await shipmentService.getByGuid(shipmentId, auth.token || "").then((response) => {
                if (response.success && response.data) {
                    //console.log(response.data);
                    setShipment(response.data);
                    setRowData(response.data.shipment_lines || []);
                }
                setLoading(false);
            });
        } catch (err) {
            console.error('Error loading shipment:', err);
            setError('Error loading shipment');
        }
    }

    const releaseShipment = async () =>
    {
        let command = new ShipmentHeaderEditCommand();
        command.id = shipment?.id;
        command.is_released = true;

        await shipmentService.update(command, auth.token || "").then((response) => {
            if (response.success) {
                loadShipment();
            }
        });
    }

    const printInvoice = () => 
    {
        window.open(`/docs/api/?url=${encodeURIComponent('/docs/packinglist/' + shipment?.guid)}`, "_blank")
    }

    const [colDefs, setColDefs] = useState<ColDef<ShipmentLineDto>[]>([
        { field: "line_number", headerName: "Line #"},
        { field: "line_description", headerName: "Description"},
        { field: "units_shipped", headerName: "Unites Shipped"},
    ]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: true,
    };

    if (loading) {
        return <div>Loading shipment...</div>;
    }

    if (error) {
        return <div>Error: {error}</div>;
    }

    if (!hasAccess) {
        return <div>Redirecting...</div>;
    }

    return (
        <Grid
            templateColumns="repeat(6, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
        >
            <GridItem colSpan={6} >
            <h1>View Shipment - {shipment?.shipment_number}</h1>
            </GridItem>

            <GridItem colSpan={2}>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Customer</DataList.ItemLabel>
                        <DataList.ItemValue>{shipment?.customer_name}</DataList.ItemValue>
                    </DataList.Item>
                    <DataList.Item>
                        <DataList.ItemLabel>Ship To</DataList.ItemLabel>
                        <DataList.ItemValue>
                            {shipment?.address?.street_address1}<br/>
                            {shipment?.address?.city}, {shipment?.address?.state}<br/>
                            {shipment?.address?.country}
                        </DataList.ItemValue>
                    </DataList.Item>
                    <DataList.Item>
                        <DataList.ItemLabel>Freight Carrier</DataList.ItemLabel>
                        <DataList.ItemValue>{shipment?.freight_carrier}</DataList.ItemValue>
                    </DataList.Item>
                    <DataList.Item>
                        <DataList.ItemLabel>Freight Charge</DataList.ItemLabel>
                        <DataList.ItemValue>${shipment?.freight_charge_amount}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={2}>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md" maxH="md">
                    <DataList.Item>
                        <DataList.ItemLabel>Order Number</DataList.ItemLabel>
                        <DataList.ItemValue>{shipment?.order_number}</DataList.ItemValue>
                    </DataList.Item>
                    <DataList.Item>
                        <DataList.ItemLabel>Order Date</DataList.ItemLabel>
                        <DataList.ItemValue>{shipment?.order_date}</DataList.ItemValue>
                    </DataList.Item>
                </DataList.Root>
            </GridItem>

            <GridItem colSpan={6}></GridItem>
            <GridItem colSpan={6}>
                <div style={{ width: "100%", height: "500px" }}>
                    <AgGridReact
                        loading={loading}
                        rowData={rowData}
                        columnDefs={colDefs}
                        defaultColDef={defaultColDef}
                    />
                </div>
            </GridItem>

            <GridItem colSpan={6} >
                <PageActionsComponent hidden={!shipment?.is_released} canSave={false} saveText={"Print Packing List"} canDelete={false} onSave={() => printInvoice() } onDelete={() => {}}/>
                <PageActionsComponent hidden={shipment?.is_released} canSave={false} saveText={"Release Shipment"} canDelete={false} onSave={() => releaseShipment() } onDelete={() => {}}/>
            </GridItem>
        </Grid>
    );
}

export default ViewShipmentsPage;