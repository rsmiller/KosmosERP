
'use client'

import '../../../../styles/page.component.css';

import { useEffect, useState, useRef } from "react";

import { Grid, GridItem, DataList, Tabs } from "@chakra-ui/react";
import { purchaseOrderService } from "@/services/purchase-order-service";
import { AgGridReact } from "ag-grid-react";
import { PurchaseOrderHeaderDto, PurchaseOrderLineDto } from "@/models/purchase-order-models";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { POReceiveAGGridDTO, PurchaseOrderReceiveHeaderDto, PurchaseOrderReceiveLineDto, PurchaseOrderReceiveUploadDto } from "@/models/po-receive-models";
import { poReceiveService } from '@/services/po-receive-service';
import { DocumentUploadRevisionDto } from '@/models/document-models';
import { DateTimeRender } from '@/components/ag-grid/date-time-renderer';
import ViewDocumentDialog from '@/components/dialogs/view-document.dialog';
import { useKeycloak } from '@react-keycloak/web';
import { useParams, useRouter } from 'next/navigation';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewPOReceivePage() {
    const { keycloak } = useKeycloak();

    const params = useParams();
    const router = useRouter();

    const [hasAccess, setHasAccess] = useState(true);
    const [hasWritePermission, setHasWritePermission] = useState(false);

    const [hasFile, setHasFile] = useState(false);
    const [hasUpdated, setHasUpdated] = useState(false);

    const [purchaseOrder, setPurchaseOrder] = useState<PurchaseOrderHeaderDto | null>(null);

    const [rowWorkingData, setRowWorkingData] = useState<POReceiveAGGridDTO[]>([]);
    const [rowPastData, setPastRowData] = useState<PurchaseOrderReceiveLineDto[]>([]);
    const [rowOldUploadsData, setOldUploadsData] = useState<PurchaseOrderReceiveUploadDto[]>([]);

    const [documentGuid, setDocumentGuid] = useState<string>();
    const [openDocumentDialog, setOpenDocumentDialog] = useState<boolean>(false);

    const [isWorking, setIsWorking] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const hasInitialized = useRef(false);

    useEffect(() => {
        if(keycloak.authenticated == false) return;

        const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
        const hasPermission = permissionsService.HasPermission(
            ERPModules.PurchaseOrderModule,
            ERPModulePermission.Write,
            realmRoles
        );
        if (!hasPermission) {
            setHasAccess(false);
            router.push('/erp');
            return;
        }
        setHasWritePermission(true);

        const poReceiveId = String(params.id);
            
        if (poReceiveId == "") {
            setError('Invalid PO Receive ID');
            return;
        }

        poReceiveService.getByGuid(poReceiveId, keycloak.token || "").then(response => {
            if (!response.success || !response.data) {
                setError('Error fetching PO Receive data');
                return;
            }

            console.log(response.data);
            setPastRowData(response.data.received_lines || []);
            setOldUploadsData(response.data.received_uploads || []);

            if(response.data.purchase_order_id)
            {
                purchaseOrderService.get(response.data.purchase_order_id, keycloak.token || "").then((pageOrderResponse) => {
                    if (!pageOrderResponse.success || !pageOrderResponse.data) {
                        setError('Error fetching Purchase Order data');
                        return;
                    }

                    setPurchaseOrder(pageOrderResponse.data);

                    if(pageOrderResponse.data.purchase_order_lines)
                    {
                        BuildComponentData(pageOrderResponse.data.purchase_order_lines, response.data)
                    }
                    
                });
            }
            

        });

    }, [keycloak.authenticated, router, params.id]);

    useEffect(() => {

        if (hasInitialized.current) return;
        hasInitialized.current = true;

        const hasValidRow = rowWorkingData.some(r =>
            (r.units_to_receive ?? 0) > 0 &&
            (r.units_to_receive ?? 0) <= (r.max_to_receive ?? 0)
        );

        //console.log(purchaseOrder && hasFile && hasValidRow)
    }, [purchaseOrder, hasFile, hasUpdated, rowWorkingData, hasWritePermission]);


    const BuildComponentData = (po_lines: PurchaseOrderLineDto[], po_data?: PurchaseOrderReceiveHeaderDto) => 
    {
        setRowWorkingData([]);
        setPastRowData([]);
        setOldUploadsData([]);

        for(let i=0;i<po_lines.length;i++)
        {
            let total_received = 0;

            if(po_data && po_data.received_lines)
            {
                total_received = po_data.received_lines
                    .filter(line => line.units_received !== undefined)
                    .reduce((sum, line) => sum + (line.units_received || 0), 0);
            }

            let dto = new POReceiveAGGridDTO();
            dto.line_number = po_lines[i].line_number;
            dto.product_name = po_lines[i].product_name;
            dto.purchase_order_id = po_lines[i].purchase_order_header_id;
            dto.purchase_order_line_id = po_lines[i].id;
            dto.total_units_ordered = po_lines[i].quantity || 0;
            dto.total_units_received = total_received;
            dto.max_to_receive = (dto.total_units_ordered - dto.total_units_received);

            setRowWorkingData(prev => [...prev, dto]);
        }

        if(po_data && po_data.received_lines)
        {
            setPastRowData(po_data.received_lines)
        }

        if(po_data && po_data.received_uploads)
        {
            setOldUploadsData(po_data.received_uploads);
        }
        

        setIsWorking(false);
    }


    const downloadDocument = (event: any) => {
        //console.log(event.data.document_upload.document_revisions[0].guid)
        const the_guid = event.data.document_upload.document_revisions[0].guid;
        setDocumentGuid(the_guid);
        setOpenDocumentDialog(true);
    }

    const handleDocumentDialogClose = () =>
    {
        setOpenDocumentDialog(false);
    }

    const [colPrevDefs, setColPrevDefs] = useState<ColDef<PurchaseOrderReceiveLineDto>[]>([
        { field: "product_name", headerName: "Product Name"},
        { field: "units_received", headerName: "Units Received" },
        { field: "created_on", headerName: "Entered On", cellRenderer: DateTimeRender },
    ]);

    const [colOldUploadsDefs, setColOldUploadsDefs] = useState<ColDef<DocumentUploadRevisionDto>[]>([
        { headerName: "Document Name", cellRenderer: (p: any) =>{
            const revisions = p.data.document_upload.document_revisions;
            if (!revisions || revisions.length === 0) return '';
            
            const most_recent = revisions
                .sort((a: any, b: any) => new Date(b.created_on).getTime() - new Date(a.created_on).getTime())[0];
            
            return most_recent?.document_name || '';
        }},
        { field: "created_on", headerName: "Uploaded On", cellRenderer: DateTimeRender  },
    ]);

    const defaultColDef: ColDef = {
        flex: 1,
        filter: false,
        sortable: false,
    };

    return (
        <div>
            <Grid
                templateColumns="repeat(3, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
                >
                <GridItem>
                    <Grid
                        templateColumns="repeat(5, 2fr)"
                        gap={6}
                        display="grid"
                        width="100%"
                        p="auto"
                        m="auto"
                        >
                            <GridItem colSpan={5}>
                                <h1>Receive Purchase Order</h1>
                            </GridItem>
                            <GridItem colSpan={2}>
                                <DataList.Root>
                                    <DataList.Item>
                                        <DataList.ItemLabel>Purchase Order</DataList.ItemLabel>
                                        <DataList.ItemValue>{purchaseOrder?.po_number}</DataList.ItemValue>
                                    </DataList.Item>
                                </DataList.Root>
                            </GridItem>
                            <GridItem colSpan={2}>
                                <DataList.Root>
                                    <DataList.Item>
                                        <DataList.ItemLabel>PO By</DataList.ItemLabel>
                                        <DataList.ItemValue>{purchaseOrder?.po_by}</DataList.ItemValue>
                                    </DataList.Item>
                                </DataList.Root>
                            </GridItem>
                            <GridItem colSpan={3}>
                                <DataList.Root>
                                    <DataList.Item>
                                        <DataList.ItemLabel>PO Vendor</DataList.ItemLabel>
                                        <DataList.ItemValue>{purchaseOrder?.vendor_name}</DataList.ItemValue>
                                    </DataList.Item>
                                </DataList.Root>
                            </GridItem>
                            <GridItem colSpan={5}><hr/></GridItem>
                            <GridItem colSpan={5}>
                                <Tabs.Root defaultValue="prev-packing">
                                    <Tabs.List>
                                        <Tabs.Trigger value="prev-packing">Previous Uploads</Tabs.Trigger>
                                    </Tabs.List>
                                    <Tabs.Content value="prev-packing">
                                        <div style={{ width: "100%", height: "328px" }}>
                                            <AgGridReact
                                                rowData={rowOldUploadsData}
                                                columnDefs={colOldUploadsDefs}
                                                defaultColDef={defaultColDef}
                                                onRowDoubleClicked={downloadDocument}
                                            />
                                        </div>
                                    </Tabs.Content>
                                </Tabs.Root>
                            </GridItem>
                        </Grid>
                </GridItem>
                <GridItem colSpan={2}>
                    <div style={{ width: "100%", height: "600px" }}>
                        <h3>Previous Entries</h3>
                        <AgGridReact
                            rowData={rowPastData}
                            columnDefs={colPrevDefs}
                            defaultColDef={defaultColDef}
                        />
                    </div>
                </GridItem>
            </Grid>

            
            <ViewDocumentDialog document_revision_guid={documentGuid} openDialog={openDocumentDialog} onClose={handleDocumentDialogClose}/>
        </div>
    );
}

export default ViewPOReceivePage;

