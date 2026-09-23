"use client"
import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';

import {
  Button,
  Checkbox,
  DataList,
  Grid,
  GridItem,
  Tabs,
} from '@chakra-ui/react';
import { format, parse } from 'date-fns';
import { useCallback, useEffect, useRef, useState } from "react";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import { useParams, useRouter } from 'next/navigation';
import { APInvoiceHeaderDto, APInvoiceLineDto, UIAssociatedLines } from '@/models/ap-models';
import { apInvoiceService } from '@/services/ap-invoice-service';
import { FaExternalLinkAlt } from 'react-icons/fa';
import { MdEditDocument } from 'react-icons/md';
import { keyValueService } from '@/services/keyvalue-service';
import { PurchaseOrderReceiveLineDto } from '@/models/po-receive-models';
import { DocumentUploadDto } from '@/models/document-models';
import { DateTimeRender } from '@/components/ag-grid/date-time-renderer';
import CommentsListComponent from '@/components/lists/comments-list-component';
import { useAuth } from '@/lib/auth/auth-context';

import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);


function ViewAPPage() {
  const params = useParams();
  const router = useRouter();
  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const [headerModel, setHeaderModel] = useState<APInvoiceHeaderDto>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [rowLineData, setRowLineData] = useState<APInvoiceLineDto[]>([]);
  const [rowAssData, setRowAssData] = useState<UIAssociatedLines[]>([]);
  const [rowRecData, setRowRecData] = useState<PurchaseOrderReceiveLineDto[]>([]);
  const [rowDocData, setRowDocData] = useState<DocumentUploadDto[]>([]);

  const [gLAccounts, setGLAccounts] = useState<any[]>([]);

  const hasInitialized = useRef(false);

  // Load AP invoice data on component mount
  useEffect(() => {

    if(auth.authenticated == false) return;

    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.APModule,
      ERPModulePermission.Read,
      realmRoles
    );
    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    setHasEditPermission(permissionsService.HasPermission(
      ERPModules.APModule,
      ERPModulePermission.Edit,
      realmRoles
    ));

    if (hasInitialized.current) return;
    hasInitialized.current = true;

    const loadAPInvoice = async () => {
      try {
        setLoading(true);
        const apInvoiceId = String(params.id);

        if (apInvoiceId === "") {
          setError('Invalid AP invoice ID');
          return;
        }

        const gl_response = await keyValueService.GetDtoByModule("eea9df53-1b36-41ea-94fa-31420315ff60", auth.token || "");
        if (gl_response.success && gl_response.data !== undefined) {
          setGLAccounts(gl_response.data.map(kv => ({
            label: kv.value,
            value: kv.key
          })));
        }

        const response = await apInvoiceService.getByGuid(apInvoiceId, auth.token || "");
        if (response.success && response.data) {
          setHeaderModel(response.data);

          if (response.data.ap_invoice_lines && response.data.ap_invoice_lines.length > 0) {
            setRowLineData(response.data.ap_invoice_lines);
          }

          if (response.data.receive_lines && response.data.receive_lines.length > 0) {
            setRowRecData(response.data.receive_lines);
          }

          if (response.data.po_lines && response.data.po_lines.length > 0) {
            setRowAssData(response.data.po_lines.map((line) => ({
              id: line.id,
              line_number: line.line_number,
              description: line.description,
              is_purchase_order: true,
              units: line.quantity
            } as UIAssociatedLines)));
          }

          if (response.data.ar_lines && response.data.ar_lines.length > 0) {
            setRowAssData(response.data.ar_lines.map((line) => ({
              id: line.id,
              line_number: line.line_number,
              description: line.line_description,
              is_purchase_order: false,
              units: line.invoice_qty
            } as UIAssociatedLines)));
          }

          if (response.data.order_lines && response.data.order_lines.length > 0) {
            setRowAssData(response.data.order_lines.map((line) => ({
              id: line.id,
              line_number: line.line_number,
              description: line.line_description,
              is_purchase_order: false,
              units: line.quantity
            } as UIAssociatedLines)));
          }
        } else {
          setError('Failed to load AP invoice');
        }
      } catch (err) {
        console.error('Error loading AP invoice:', err);
        setError('Error loading AP invoice');
      } finally {
        setLoading(false);
      }
    };

    loadAPInvoice();
  }, [params.id, auth.authenticated]);

  const [colLineDefs, setColLineDefs] = useState<ColDef<APInvoiceLineDto>[]>([]);

  useEffect(() => {
    setColLineDefs([
      { field: "line_number", headerName: "Line #"},
      { field: "gl_account", headerName: "GL Account", cellRenderer: (param: any) => {
        const account = gLAccounts.find(m => m.value == param.value);
        return account ? account.label : '';
      }},
      { field: "description", headerName: "Line Description"},
      { headerName: "Units Ordered", cellRenderer: (params: any) => { return params.data.units_ordered } },
      { field: "qty_invoiced", headerName: "Qty Invoiced" }
    ]);
  }, [gLAccounts]);

  const [colAssDefs, setColAssDefs] = useState<ColDef<UIAssociatedLines>[]>([
    { field: "line_number", headerName: "Line #"},
    { field: "description", headerName: "Line Description"},
    { field: "units", headerName: "Units" },
  ]);

  const [colRecDefs, setColRecDefs] = useState<ColDef<PurchaseOrderReceiveLineDto>[]>([
    { field: "line_number", headerName: "Line #"},
    { field: "product_name", headerName: "Line Description"},
    { field: "units_ordered", headerName: "Units Ordered" },
    { field: "units_received", headerName: "Units Received" },
  ]);

  const [colDocDefs, setColDocDefs] = useState<ColDef<DocumentUploadDto>[]>([
      { headerName: "Document Name", cellRenderer: (p: any) =>{
          const revisions = p.data.document_revisions;
          if (!revisions || revisions.length === 0) return '';

          const most_recent = revisions
              .sort((a: any, b: any) => new Date(b.created_on).getTime() - new Date(a.created_on).getTime())[0];

          return most_recent?.document_name || '';
      }},
      { field: "created_on", headerName: "Uploaded On", cellRenderer: DateTimeRender  }
  ]);

  const defaultColDef: ColDef = {
    flex: 1,
    filter: true,
    sortable: true,
    editable: false,
  };

  const getRowId = useCallback((params: any) => String(params.data.id), []);

  const getAssociatedObjectType = useCallback((header: APInvoiceHeaderDto | undefined) => {
    if(header?.association_is_ar_invoice != undefined && header.association_is_ar_invoice == true)
    {
      return "Accounts Receivable";
    }

    if(header?.association_is_purchase_order != undefined && header.association_is_purchase_order == true)
    {
      return "Purchase Order";
    }

    if(header?.association_is_sales_order != undefined && header.association_is_sales_order == true)
    {
      return "Sales Order";
    }
  }, []);

  // Dates come back as ISO strings; only use the date part so the timezone doesn't shift the day
  const formatDate = (value: Date | string | undefined) => {
    if (!value) return "";

    const date_string = value.toString();
    const date_part = date_string.indexOf("T") > -1 ? date_string.substring(0, date_string.indexOf("T")) : date_string;

    return format(parse(date_part, 'yyyy-MM-dd', new Date()), 'MM/dd/yyyy');
  }

  const formatCurrency = (value: number | undefined) => {
    return "$" + (value || 0).toFixed(2).toLocaleString();
  }

  const handleEditClick = () => {
    router.push("/erp/ap/edit/" + headerModel?.guid);
  };

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  if (loading) {
    return <div>Loading AP invoice...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!headerModel) {
    return <div>AP invoice not found</div>;
  }

  return (
    <Grid
          templateColumns="repeat(5, 2fr)"
          gap={6}
          display="grid"
          width="100%"
          p="auto"
          m="auto"
        >
          <GridItem colSpan={4}>
              <h1>AP Invoice - {headerModel.invoice_number}</h1>
          </GridItem>
          <GridItem colSpan={1} textAlign="right">
              <Button hidden={!hasEditPermission} type="button" colorPalette="green" onClick={handleEditClick}><MdEditDocument /> Edit</Button>
          </GridItem>

          <GridItem colSpan={2}>
              <div><h3>Invoice</h3></div>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Invoice Number</DataList.ItemLabel>
                  <DataList.ItemValue>{headerModel.invoice_number}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Vendor Name</DataList.ItemLabel>
                  <DataList.ItemValue>{headerModel.vendor_name}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Invoice Date</DataList.ItemLabel>
                  <DataList.ItemValue>{formatDate(headerModel.invoice_date)}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Received Date</DataList.ItemLabel>
                  <DataList.ItemValue>{formatDate(headerModel.invoice_received_date)}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Due Date</DataList.ItemLabel>
                  <DataList.ItemValue>{formatDate(headerModel.invoice_due_date)}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Invoice Total</DataList.ItemLabel>
                  <DataList.ItemValue>{formatCurrency(headerModel.invoice_total)}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Is Paid</DataList.ItemLabel>
                  <DataList.ItemValue>
                    <Checkbox.Root checked={headerModel.is_paid || false} disabled>
                      <Checkbox.Control />
                    </Checkbox.Root>
                  </DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
          </GridItem>

          <GridItem colSpan={2}>
              <div><h3>Associated Object</h3></div>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Object Type</DataList.ItemLabel>
                  <DataList.ItemValue>{getAssociatedObjectType(headerModel)}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Object Number</DataList.ItemLabel>
                  <DataList.ItemValue>{headerModel.association_number}&nbsp;<FaExternalLinkAlt size="15" /></DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <div style={{marginTop: "50px"}}><h3>Purchase Order Receive</h3></div>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>First Receive Date</DataList.ItemLabel>
                  <DataList.ItemValue>{headerModel.first_po_receive_date ? format(headerModel.first_po_receive_date, 'MM-dd-yyyy') : ""}</DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
              <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                <DataList.Item>
                  <DataList.ItemLabel>Receive Lines</DataList.ItemLabel>
                  <DataList.ItemValue><div hidden={rowRecData.length == 0}>Lines &nbsp;<FaExternalLinkAlt size="15" /></div></DataList.ItemValue>
                </DataList.Item>
              </DataList.Root>
          </GridItem>

          <GridItem colSpan={6} >
            <Tabs.Root defaultValue="invoice-lines">
                <Tabs.List>
                  <Tabs.Trigger value="invoice-lines">Invoice Lines</Tabs.Trigger>
                  <Tabs.Trigger value="ass-lines">Assoicated Lines</Tabs.Trigger>
                  <Tabs.Trigger value="rec-lines">Received Lines</Tabs.Trigger>
                  <Tabs.Trigger value="documents">Documents</Tabs.Trigger>
                  <Tabs.Trigger value="comments">Comments</Tabs.Trigger>
                </Tabs.List>
                <Tabs.Content value="invoice-lines">
                    <div style={{ width: "100%", height: "500px" }}>
                      <AgGridReact
                          rowData={rowLineData}
                          columnDefs={colLineDefs}
                          defaultColDef={defaultColDef}
                          getRowId={getRowId}
                        />
                    </div>
                </Tabs.Content>
                <Tabs.Content value="ass-lines">
                    <div style={{ width: "100%", height: "500px" }}>
                      <AgGridReact
                          rowData={rowAssData}
                          columnDefs={colAssDefs}
                          defaultColDef={defaultColDef}
                          getRowId={getRowId}
                        />
                    </div>
                </Tabs.Content>
                <Tabs.Content value="rec-lines">
                    <div style={{ width: "100%", height: "500px" }}>
                      <AgGridReact
                          rowData={rowRecData}
                          columnDefs={colRecDefs}
                          defaultColDef={defaultColDef}
                        />
                    </div>
                </Tabs.Content>
                <Tabs.Content value="documents">
                    <div style={{ width: "100%", height: "500px" }}>
                      <AgGridReact
                          rowData={rowDocData}
                          columnDefs={colDocDefs}
                          defaultColDef={defaultColDef}
                        />
                    </div>
                </Tabs.Content>
                <Tabs.Content value="comments">
                    <CommentsListComponent />
                </Tabs.Content>
            </Tabs.Root>
          </GridItem>
      </Grid>
  )
}

export default ViewAPPage;
