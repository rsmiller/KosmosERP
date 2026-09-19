
'use client'

import '../../../styles/page.component.css';

import { useEffect, useState, useRef } from "react";

import SessionStorage from '@/components/session-storage';
import { Button, Field, Grid, GridItem, Input, Stack, Dialog, Portal, CloseButton, DataList, Spinner, Tabs, FileUpload, Alert, Icon, Box } from "@chakra-ui/react";
import { purchaseOrderService } from "@/services/purchase-order-service";
import { AgGridReact } from "ag-grid-react";
import { PurchaseOrderHeaderDto, PurchaseOrderLineDto } from "@/models/purchase-order-models";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { POReceiveAGGridDTO, PurchaseOrderReceiveHeaderDto, PurchaseOrderReceiveLineCreateCommand, PurchaseOrderReceiveLineDto, PurchaseOrderReceiveHeaderCreateCommand, PurchaseOrderReceiveUploadDto } from "@/models/po-receive-models";
import { poReceiveService } from '@/services/po-receive-service';
import { DocumentUploadCreateCommand, DocumentUploadRevisionDto, DocumentUploadRevisionTagCreateCommand } from '@/models/document-models';
import { LuUpload } from 'react-icons/lu';
import { documentService } from '@/services/document-service';
import { DateTimeRender } from '@/components/ag-grid/date-time-renderer';
import POReceiveUnitsReceiveEditor from '@/components/ag-grid/po-receive-units-receive-editor';
import ViewDocumentDialog from '@/components/dialogs/view-document.dialog';
import { useAuth } from '@/lib/auth/auth-context';
import { useRouter } from 'next/navigation';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewPOReceivePage() {
    const auth = useAuth();
    const router = useRouter();

    const [hasAccess, setHasAccess] = useState(true);
    const [hasWritePermission, setHasWritePermission] = useState(false);
    const [poNumber, setPoNumber] = useState<string>('');
    const [isAlertOpen, setIsAlertOpen] = useState(false);

    const userId = SessionStorage.getUserId();
    const sessionId = SessionStorage.getSession();

    const [isWorking, setIsWorking] = useState(false);
    const [canSave, setCanSave] = useState(false);
    const [hasFile, setHasFile] = useState(false);
    const [hasUpdated, setHasUpdated] = useState(false);
    const [displayGoodAlert, setDisplayGoodAlert] = useState(false);
    const [displayBadAlert, setDisplayBadAlert] = useState(false);

    const [purchaseOrder, setPurchaseOrder] = useState<PurchaseOrderHeaderDto | null>(null);

    const [rowWorkingData, setRowWorkingData] = useState<POReceiveAGGridDTO[]>([]);
    const [rowPastData, setPastRowData] = useState<PurchaseOrderReceiveLineDto[]>([]);
    const [rowOldUploadsData, setOldUploadsData] = useState<PurchaseOrderReceiveUploadDto[]>([]);

    const [completedOrDisabled, setCompletedOrDisabled] = useState(false);
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [documentGuid, setDocumentGuid] = useState<string>();
    const [openDocumentDialog, setOpenDocumentDialog] = useState<boolean>(false);
    const hasInitialized = useRef(false);

    useEffect(() => {
        if(auth.authenticated == false) return;

        const realmRoles = auth.roles || [];
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
    }, [auth.authenticated, router]);

    useEffect(() => {
    if (hasInitialized.current) return;
    hasInitialized.current = true;
    const hasValidRow = rowWorkingData.some(r =>
        (r.units_to_receive ?? 0) > 0 &&
        (r.units_to_receive ?? 0) <= (r.max_to_receive ?? 0)
        );
        setCanSave(!!purchaseOrder && hasFile && hasValidRow && hasWritePermission);
        console.log(purchaseOrder && hasFile && hasValidRow)
    }, [purchaseOrder, hasFile, hasUpdated, rowWorkingData, hasWritePermission]);

    const searchForPO = async () => {
        setHasUpdated(false);
        setCanSave(false);
        setHasFile(false);
        setDisplayBadAlert(false);
        setDisplayGoodAlert(false);

        if (!poNumber) {
            return;
        }

        try {
            
            setIsWorking(true);
            const response = await purchaseOrderService.getByPONumber(parseInt(poNumber), auth.token || "");
            
            //console.log(response);
            if (!response.data || !response.success) {
                setPurchaseOrder(null);
                setIsAlertOpen(true);
            }
            else
            {
                setPurchaseOrder(response.data);

                const find_response = await poReceiveService.getDtoByPOId(response.data.id, auth.token || "");

                console.log(find_response);

                if(response.data.purchase_order_lines)
                {
                    BuildComponentData(response.data.purchase_order_lines, find_response.data)
                }
                else
                {
                    setIsWorking(false);
                }
            }
        } catch (error) {
            console.error('Error searching for PO:', error);
            setDisplayBadAlert(true);
            setIsWorking(false);
        }
    }

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

    const onCloseAlert = () => {
        setIsAlertOpen(false);
    }

    const CheckVailidity = () =>
    {
        const isValid = hasUpdated && hasFile;

        if(isValid)
        {
            setCanSave(true);
        }
        else
        {
            setCanSave(false);
        }
    }

    const onCellValueChanged = (event: any) => {
        setHasUpdated(true);

        CheckVailidity();
    };

    const onFileUploadChanged = (event: any) => {
        //console.log(event)
        if(event.acceptedFiles.length > 0)
        {
            setHasFile(true);
            setSelectedFile(event.acceptedFiles[0]);
        }
        else
        {
            setHasFile(false);
            setSelectedFile(null);
        }

        CheckVailidity();
    }

    const onSave = async () =>
    {
        if (!purchaseOrder) return;
        try {
            setIsWorking(true);
            if (!selectedFile) { setIsWorking(false); return; }

            // Upload document first
            const documentUploadRevision: DocumentUploadRevisionTagCreateCommand = 
            {
                calling_user_id: Number(userId),
                token: sessionId?.toString(),
                document_upload_object_tag_id: 10,
                tag_name: "PO Number",
                tag_value: purchaseOrder?.po_number
            } as DocumentUploadRevisionTagCreateCommand;

            const documentUploadCreate: DocumentUploadCreateCommand = {
                document_object_id: 5, // TODO: Don't hardcode this. This doc id = po_receive_upload
                calling_user_id: Number(userId),
                token: sessionId?.toString(),
                revision_tags: [documentUploadRevision],
                document_name: selectedFile.name 
                
            } as DocumentUploadCreateCommand;

            //console.log(documentUploadCreate)
            //return;

            const docResponse = await documentService.create(auth.token || "", selectedFile, documentUploadCreate);

            if (!docResponse.success || !docResponse.data) { setIsWorking(false); setDisplayBadAlert(true); return; }

            //console.log(docResponse);
            

            // Build received lines from working data
            const lines: PurchaseOrderReceiveLineCreateCommand[] = rowWorkingData
                .filter(r => (r.units_to_receive ?? 0) > 0)
                .map(r => ({
                    calling_user_id: Number(userId),
                    token: sessionId?.toString(),
                    purchase_order_line_id: r.purchase_order_line_id,
                    units_received: r.units_to_receive
                }));

            if (!lines.length) { setIsWorking(false); return; }

            const headerCreate: PurchaseOrderReceiveHeaderCreateCommand = {
                calling_user_id: Number(userId),
                token: sessionId?.toString(),
                purchase_order_id: purchaseOrder.id,
                document_upload_id: docResponse.data.id,
                received_lines: lines
            } as PurchaseOrderReceiveHeaderCreateCommand;

            //console.log(headerCreate);

            const createResponse = await poReceiveService.create(headerCreate, auth.token || "");

            //console.log(createResponse);

            if (!createResponse.success || !createResponse.data) { setIsWorking(false); setDisplayBadAlert(true); return; }

            setIsWorking(false);
            setDisplayGoodAlert(true);
        } catch (e) {
            console.error(e);
            setIsWorking(false);
            setDisplayBadAlert(true);
        }
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

    const [colDefs, setColDefs] = useState<ColDef<POReceiveAGGridDTO>[]>([
        { field: "line_number", headerName: "Line #"},
        { field: "product_name", headerName: "Product Name"},
        { field: "total_units_ordered", headerName: "Units Ordered" },
        { field: "total_units_received", headerName: "Units Received" },
        { field: "units_to_receive", headerName: "Units To Receive", editable: true, cellEditor: POReceiveUnitsReceiveEditor },
    ]);

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
                            <GridItem colSpan={5} gap={0}>
                                <Stack direction={{ base: "column", md: "row" }}>
                                    
                                    <Field.Root w="250px" h="20">
                                        <Input 
                                            value={poNumber}
                                            onChange={(e) => setPoNumber(e.target.value)}
                                            placeholder="Enter PO Number"
                                        />
                                    </Field.Root>
                                    <Button w="100px" h="10" onClick={searchForPO}><Spinner hidden={!isWorking} />Search</Button>
                                </Stack>
                            </GridItem>
                            <GridItem colSpan={5}><hr/></GridItem>
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
                                <Tabs.Root defaultValue="upload-poacking">
                                    <Tabs.List>
                                        <Tabs.Trigger value="upload-poacking">Upload Packing List</Tabs.Trigger>
                                        <Tabs.Trigger value="prev-packing">Previous Uploads</Tabs.Trigger>
                                    </Tabs.List>
                                    <Tabs.Content value="upload-poacking">
                                        <FileUpload.Root onFileChange={onFileUploadChanged} maxFiles={1} alignItems="stretch" accept={["image/png","image/jpg", "application/pdf"]}>
                                            <FileUpload.HiddenInput />
                                            <FileUpload.Dropzone>
                                                <Icon size="md" color="fg.muted">
                                                <LuUpload />
                                                </Icon>
                                                <FileUpload.DropzoneContent>
                                                <Box>Drag and drop files here</Box>
                                                <Box color="fg.muted">.pdf, .png, .jpg up to 5MB</Box>
                                                </FileUpload.DropzoneContent>
                                            </FileUpload.Dropzone>
                                            <FileUpload.List />
                                        </FileUpload.Root>
                                    </Tabs.Content>
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
                    <div style={{ width: "100%", paddingTop: "10px", paddingBottom: "10px", textAlign: "center" }}>
                        <Button type="button" colorPalette="blue" onClick={onSave} disabled={!canSave}>Receive Purchase Order</Button>
                         <Alert.Root status="success" style={{ marginTop: "10px"}} hidden={!displayGoodAlert}>
                            <Alert.Indicator />
                            <Alert.Content>
                                <Alert.Title>Record saved!</Alert.Title>
                            </Alert.Content>
                        </Alert.Root>
                        <Alert.Root status="error" style={{ marginTop: "10px"}} hidden={!displayBadAlert}>
                            <Alert.Indicator />
                            <Alert.Content>
                                <Alert.Title>Record could not be saved!</Alert.Title>
                            </Alert.Content>
                        </Alert.Root>
                    </div>
                    <div style={{ width: "100%", height: "600px" }}>
                        
                        <AgGridReact
                            rowData={rowWorkingData}
                            columnDefs={colDefs}
                            defaultColDef={defaultColDef}
                            context={{ completedOrDisabled }}
                            onCellValueChanged={onCellValueChanged}
                            />
                        </div>
                </GridItem>
                <GridItem colSpan={3}><hr/></GridItem>
                <GridItem colSpan={3}>
                    <h3>Previous Entries</h3>
                    <div style={{ width: "100%", height: "500px" }}>
                        <AgGridReact
                            rowData={rowPastData}
                            columnDefs={colPrevDefs}
                            defaultColDef={defaultColDef}
                        />
                    </div>
                </GridItem>
            </Grid>

            <Dialog.Root open={isAlertOpen} onOpenChange={(details) => setIsAlertOpen(details.open)} role="alertdialog">
                <Portal>
                    <Dialog.Backdrop />
                    <Dialog.Positioner>
                        <Dialog.Content>
                            <Dialog.Header>
                                <Dialog.Title>PO Not Found</Dialog.Title>
                            </Dialog.Header>
                            <Dialog.Body>
                                The purchase order number you entered was not found.
                            </Dialog.Body>
                            <Dialog.Footer>
                                <Dialog.ActionTrigger asChild>
                                    <Button onClick={() => {setIsAlertOpen(false)}}>Okay</Button>
                                </Dialog.ActionTrigger>
                                <Dialog.CloseTrigger asChild>
                                    <CloseButton size="sm" />
                                </Dialog.CloseTrigger>
                            </Dialog.Footer>
                        </Dialog.Content>
                    </Dialog.Positioner>
                </Portal>
            </Dialog.Root>
            <ViewDocumentDialog document_revision_guid={documentGuid} openDialog={openDocumentDialog} onClose={handleDocumentDialogClose}/>
        </div>
    );
}

export default NewPOReceivePage;

