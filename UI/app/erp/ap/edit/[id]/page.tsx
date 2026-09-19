"use client"
import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
import '../../../../styles/data-list.css';

import { useForm } from 'react-hook-form'
import {
  Checkbox,
  DataList,
  Field,
  Grid,
  GridItem,
  Input,
  Tabs,
} from '@chakra-ui/react';
import { parse } from 'date-fns/parse';
import { useCallback, useEffect, useRef, useState } from "react";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import { useParams, useRouter } from 'next/navigation';
import PageActionsComponent from '@/components/page-actions';
import { APInvoiceHeaderDto, APInvoiceHeaderEditCommand, APInvoiceLineDto, APInvoiceHeaderDeleteCommand, UIAssociatedLines, APInvoiceLineEditCommand, APInvoiceLineCreateCommand } from '@/models/ap-models';
import { apInvoiceService } from '@/services/ap-invoice-service';
import { FaExternalLinkAlt } from 'react-icons/fa';
import { keyValueService } from '@/services/keyvalue-service';
import { PurchaseOrderLineDto } from '@/models/purchase-order-models';
import { ARInvoiceLineDto } from '@/models/ar-models';
import { OrderLineDto } from '@/models/sales-order-models';
import APInvoiceQuantityInvoicedNumerticEditor from '@/components/ag-grid/apinvoice-quantity-invoiced-numeric-editor';
import DatePicker from 'react-datepicker';
import VendorCombobox, { VendorComboboxRef } from '@/components/vendor-combobox';
import { format } from 'date-fns';
import { PurchaseOrderReceiveLineDto } from '@/models/po-receive-models';
import { DocumentUploadDto } from '@/models/document-models';
import { DateTimeRender } from '@/components/ag-grid/date-time-renderer';
import  GLAccountSelector from '@/components/ag-grid/gl-account-selector';
import CommentsListComponent from '@/components/lists/comments-list-component';
import { useAuth } from '@/lib/auth/auth-context';

import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);


function EditAPPage() {
  const params = useParams();
  const router = useRouter();
  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);
  const [hasDeletePermission, setHasDeletePermission] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    setValue,
    watch,
    control,
  } = useForm<APInvoiceHeaderEditCommand>();

  const [headerModel, setHeaderModel] = useState<APInvoiceHeaderDto>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saveable, setSaveable] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);
  const [saving, setSaving] = useState(false);

  const [rowAPData, setRowAPData] = useState<APInvoiceLineCreateCommand[]>([]);
  const [rowPrevData, setRowPrevData] = useState<APInvoiceLineDto[]>([]);
  const [rowAssData, setRowAssData] = useState<UIAssociatedLines[]>([]);
  const [rowRecData, setRowRecData] = useState<PurchaseOrderReceiveLineDto[]>([]);
  const [rowDocData, setRowDocData] = useState<DocumentUploadDto[]>([]);

  const [gLAccounts, setGLAccounts] = useState<any[]>([]);

  const vendorComboboxRef = useRef<VendorComboboxRef>(null);
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

    // Check Edit permission
    const canEdit = permissionsService.HasPermission(
      ERPModules.APModule,
      ERPModulePermission.Edit,
      realmRoles
    );
    setHasEditPermission(canEdit);

    // Check Delete permission
    const canDelete = permissionsService.HasPermission(
      ERPModules.APModule,
      ERPModulePermission.Delete,
      realmRoles
    );
    setHasDeletePermission(canDelete);

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

        await keyValueService.GetDtoByModule("eea9df53-1b36-41ea-94fa-31420315ff60", auth.token || "").then(async (gl_response) =>
        {
            //console.log("KEY VALUES ", response)
            if (gl_response.success && gl_response.data !== undefined) {
                const accounts = gl_response.data.map(kv => ({
                    label: kv.value,
                    value: kv.key
                }));

                //console.log("Accounts ", accounts)

                setGLAccounts(accounts);


            const response = await apInvoiceService.getByGuid(apInvoiceId, auth.token || "");
            if (response.success && response.data) {
              //setHeaderModel(response.data);

              //console.log(response)
              // Need to clean some data
              var invoice_date = parse(response.data.invoice_date ? response.data.invoice_date.toString().substring(0, response.data.invoice_date.toString().indexOf("T")) : '', 'yyyy-MM-dd', new Date());
              var invoice_received_date = parse(response.data.invoice_received_date ? response.data.invoice_received_date.toString().substring(0, response.data.invoice_received_date.toString().indexOf("T")) : '', 'yyyy-MM-dd', new Date());
              var invoice_due_date = parse(response.data.invoice_due_date ? response.data.invoice_due_date.toString().substring(0, response.data.invoice_due_date.toString().indexOf("T")) : '', 'yyyy-MM-dd', new Date());

              response.data.invoice_date = invoice_date;
              response.data.invoice_received_date = invoice_received_date;
              response.data.invoice_due_date = invoice_due_date;
              
              setHeaderModel(response.data);

              // Set form values with loaded data
              setValue('id', response.data.id as number);
              setValue('invoice_number', response.data.invoice_number || '');
              setValue('invoice_date', invoice_date);
              setValue('invoice_received_date', invoice_received_date);
              setValue('invoice_due_date', invoice_due_date);
              setValue('vendor_id', response.data.vendor_id);
              setValue('association_is_purchase_order', response.data.association_is_purchase_order || false);
              setValue('association_object_id', response.data.association_object_id);
              setValue('is_paid', response.data.is_paid || false);
              
              // Load invoice lines if available
              if (response.data.ap_invoice_lines && response.data.ap_invoice_lines.length > 0) {
                setRowPrevData(response.data.ap_invoice_lines);

              }

              if(response.data.receive_lines && response.data.receive_lines.length > 0)
              {
                setRowRecData(response.data.receive_lines);
              }


              if(response.data.po_lines && response.data.po_lines.length > 0)
              {
                const some_lines = response.data.po_lines.map((line) => ({
                  id: line.id,
                  line_number: line.line_number,
                  description: line.description,
                  is_purchase_order: true,
                  units: line.quantity
                } as UIAssociatedLines));

                setRowAssData(some_lines);

                const edits = response.data.po_lines.map((line: PurchaseOrderLineDto) => ({
                  ap_invoice_header_id: response.data?.id,
                  line_number: line.line_number,
                  association_is_ar_invoice: false,
                  association_is_purchase_order: true,
                  association_is_sales_order: false,
                  association_object_id: line.purchase_order_header_id,
                  association_object_line_id: line.id,
                  description: line.description,
                  qty_ordered: line.quantity,
                  max_quantity: GeMaxLineQuantity(line.id, line.quantity ? line.quantity : 0, response.data?.ap_invoice_lines),
                  total_invoiced: GetLineSumQuantity(line.id, line.quantity ? line.quantity : 0, response.data?.ap_invoice_lines),
                } as APInvoiceLineCreateCommand));

                //console.log(edits);
                let filtered = edits.filter(m => m.max_quantity < m.total_invoiced)

                setRowAPData(filtered);
              }

              if(response.data.ar_lines && response.data.ar_lines.length > 0)
              {
                const some_lines = response.data.ar_lines.map((line) => ({
                  id: line.id,
                  line_number: line.line_number,
                  description: line.line_description,
                  is_purchase_order: true,
                  units: line.invoice_qty
                } as UIAssociatedLines));

                setRowAssData(some_lines);

                const edits = response.data.ar_lines.map((line: ARInvoiceLineDto) => ({
                  ap_invoice_header_id: response.data?.id,
                  line_number: line.line_number,
                  association_is_ar_invoice: true,
                  association_is_purchase_order: false,
                  association_is_sales_order: false,
                  association_object_id: line.ar_invoice_header_id,
                  association_object_line_id: line.id,
                  description: line.line_description,
                  qty_ordered: line.invoice_qty,
                  max_quantity: GeMaxLineQuantity(line.id, line.invoice_qty ? line.invoice_qty : 0, response.data?.ap_invoice_lines),
                  total_invoiced: GetLineSumQuantity(line.id, line.invoice_qty ? line.invoice_qty : 0, response.data?.ap_invoice_lines),
                  
                } as APInvoiceLineCreateCommand));

                

                setRowAPData(edits);
              }

              if(response.data.order_lines && response.data.order_lines.length > 0)
              {
                const some_lines = response.data.order_lines.map((line) => ({
                  id: line.id,
                  line_number: line.line_number,
                  description: line.line_description,
                  is_purchase_order: true,
                  units: line.quantity
                } as UIAssociatedLines));

                setRowAssData(some_lines);


                const edits = response.data.order_lines.map((line: OrderLineDto) => ({
                  ap_invoice_header_id: response.data?.id,
                  line_number: line.line_number,
                  association_is_ar_invoice: false,
                  association_is_purchase_order: false,
                  association_is_sales_order: true,
                  association_object_id: line.order_header_id,
                  association_object_line_id: line.id,
                  description: line.line_description,
                  qty_ordered: line.quantity,
                  max_quantity: GeMaxLineQuantity(line.id, line.quantity ? line.quantity : 0, response.data?.ap_invoice_lines),
                  total_invoiced: GetLineSumQuantity(line.id, line.quantity ? line.quantity : 0, response.data?.ap_invoice_lines),
                } as APInvoiceLineCreateCommand));

                setRowAPData(edits);
              }
              
              setSaveable(true);
            } else {
              setError('Failed to load AP invoice');
            }
            }
        });
      } catch (err) {
        console.error('Error loading AP invoice:', err);
        setError('Error loading AP invoice');
      } finally {
        setLoading(false);
      }
    };

    loadAPInvoice();
  }, [params.id, setValue, setGLAccounts, auth.authenticated]);


  const GeMaxLineQuantity = (association_line_id: number, association_line_quantity: number, existing_lines?: APInvoiceLineDto[]) =>
  {
    if(existing_lines && existing_lines.length > 0)
    {
      var filter = existing_lines.filter(m => m.association_object_line_id == association_line_id);

      if(filter && filter.length > 0)
      {
        let sum = 0;
        
        // Sum all filtered entities with qty_invoiced
        filter.forEach(line => {
          if (line.qty_invoiced) {
            sum += line.qty_invoiced;
          }
        });

        return association_line_quantity - sum;
      }
      else
      {
        return association_line_quantity;
      }
    }
    else
    {
      return association_line_quantity;
    }
    
  }

  const GetLineSumQuantity = (association_line_id: number, association_line_quantity: number, existing_lines?: APInvoiceLineDto[]) =>
  {
    if(existing_lines && existing_lines.length > 0)
    {
      var filter = existing_lines.filter(m => m.association_object_line_id == association_line_id);

      if(filter && filter.length > 0)
      {
        let sum = 0;
        
        // Sum all filtered entities with qty_invoiced
        filter.forEach(line => {
          if (line.qty_invoiced) {
            sum += line.qty_invoiced;
          }
        });

        return association_line_quantity - sum;
      }
      else
      {
        return 0;
      }
    }
    else
    {
      return 0;
    }
    
  }

  const handleDeleteClick = async () => {
    if (!headerModel) return;

    let command = new APInvoiceHeaderDeleteCommand();
    command.id = headerModel.id;

    try {
      const response = await apInvoiceService.delete(command, auth.token || "");
      if (response.success) {
        router.push("/erp/ap/");
      } else {
        setSuccessSaved(false);
        setFailedSaved(true);
      }
    } catch (e) {
      console.error(e);
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const handleSaveClick = async () => {
    if (!headerModel) return;

    setSuccessSaved(false);
    setSaving(true);

    let command = new APInvoiceHeaderEditCommand();
    command.id = headerModel.id;
    command.invoice_number = watch('invoice_number');
    command.invoice_date = watch('invoice_date');
    command.invoice_received_date = watch('invoice_received_date');
    command.invoice_due_date = watch('invoice_due_date');
    command.vendor_id = watch('vendor_id');
    command.association_is_purchase_order = watch('association_is_purchase_order');
    command.association_object_id = watch('association_object_id');
    command.is_paid = watch('is_paid') == null || watch('is_paid') == false? false : true;

    for(let i=0; i<rowAPData.length; i++)
    {
      let ap_line = rowAPData[i];

      if(ap_line.gl_account && ap_line.qty_invoiced && ap_line.qty_invoiced > 0)
      {
        
        command.ap_create_invoice_lines.push(ap_line);
      } 
    }

    //console.log(command);
    //return;

    try {
      const response = await apInvoiceService.update(command, auth.token || "");
      if (response.success) {
        setSuccessSaved(true);
        setFailedSaved(false);
        // Refresh the header model with updated data
        if (response.data) {
          setHeaderModel(response.data);
        }
      } else {
        setSuccessSaved(false);
        setFailedSaved(true);
      }
    } catch (e) {
      console.error(e);
      setSuccessSaved(false);
      setFailedSaved(true);
    } finally {
      setSaving(false);
    }
  };

  const onSubmit = handleSubmit((data) => {
    handleSaveClick();
  });

  
  const [colDefs, setColDefs] = useState<ColDef<APInvoiceLineEditCommand>[]>([]);
  const [colPrevDefs, setColPrevDefs] = useState<ColDef<APInvoiceLineDto>[]>([]);

  useEffect(() => {
    setColDefs([
      { field: "line_number", headerName: "Line #"},
      { field: "gl_account", headerName: "GL Account", 
          editable: true, 
          cellEditor: GLAccountSelector, 
          cellEditorParams:{ accounts: gLAccounts } 
      },
      { field: "description", headerName: "Line Description"},
      { field: "qty_ordered", headerName: "Units Ordered"},
      { field: "qty_invoiced", headerName: "Qty Invoiced", cellEditor: APInvoiceQuantityInvoicedNumerticEditor, editable: true }
    ]);

    setColPrevDefs([
      { field: "line_number", headerName: "Line #"},
      { field: "gl_account", headerName: "GL Account", cellRenderer: (param: any) => { 
        let derp = gLAccounts.filter(m => m.value == param.value);

        if(derp.length > 0)
        {
          return derp[0].label;
        }
        else
        {
          return '';
        }
      }},
      { field: "description", headerName: "Line Description"},
      { headerName: "Units Ordered", cellRenderer: (params: any) => { return params.data.units_ordered } },
      { field: "qty_invoiced", headerName: "Qty Invoiced", cellEditor: "agNumberCellEditor" }
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

  if (loading) {
    return <div>Loading AP invoice...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!headerModel) {
    return <div>AP invoice not found</div>;
  }

  const IsDirty = (formName: any) => {
    if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
    {
        return true;
    }

    return false;
  }

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  const CheckFormValidity = () => {
    const vendorValid = vendorComboboxRef.current?.isValid() || false;
    const hasRequiredFields = Boolean(watch('invoice_number') && watch('vendor_id'));
    const allValid = vendorValid && hasRequiredFields;

    setSaveable(allValid);
  }


  const getDueDate = () => {
    const auditOn = watch('invoice_due_date');
    return auditOn ? new Date(auditOn) : undefined;
  };

  const handleDueDateChange = (date: Date | null) => {
    setValue('invoice_due_date', date || undefined);
    CheckFormValidity();
  };

  const getReceivedDate = () => {
    const receivedOn = watch('invoice_received_date');
    return receivedOn ? new Date(receivedOn) : undefined;
  };

  const handleReceivedDateChange = (date: Date | null) => {
    setValue('invoice_received_date', date || undefined);
    CheckFormValidity();
  };

  const getInvoiceDate = () => {
    const invoiceDate = watch('invoice_date');
    return invoiceDate ? new Date(invoiceDate) : undefined;
  };

  const handleInvoiceDateChange = (date: Date | null) => {
    setValue('invoice_date', date || undefined);
    CheckFormValidity();
  };

  const handleVendorSelect = (value: any) => {
    setValue('vendor_id', value?.id || 0);
    CheckFormValidity();
  }

  const massageReceievedDate = () => {
    let da_date = headerModel?.first_po_receive_date;

    if(da_date)
    {
      return format(da_date, 'MM-dd-yyyy');
    }
    else
    {
      return "";
    }
  }

  return (
    <form onSubmit={onSubmit}>
      <Grid
            templateColumns="repeat(5, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
          >
            <GridItem colSpan={2}>
                <div><h3>Invoice</h3></div>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                  <DataList.Item>
                    <DataList.ItemLabel>Invoice Number</DataList.ItemLabel>
                    <DataList.ItemValue>
                      <Field.Root required={true} invalid={IsDirty('invoice_number')}>
                        <Input  {...register('invoice_number')} />
                      </Field.Root>
                    </DataList.ItemValue>
                  </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                  <DataList.Item>
                    <DataList.ItemLabel>Vendor Name</DataList.ItemLabel>
                    <DataList.ItemValue>
                      <VendorCombobox 
                          ref={vendorComboboxRef}
                          dbKey={watch('vendor_id') || 0}
                          control={control}
                          name="vendor_id"
                          error={errors.vendor_id}
                          onChange={handleVendorSelect}
                          onValidationChange={CheckFormValidity} 
                          disabled={false}                      
                        />
                    </DataList.ItemValue>
                  </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                  <DataList.Item>
                    <DataList.ItemLabel>Invoice Date</DataList.ItemLabel>
                    <DataList.ItemValue>
                      <DatePicker 
                          selected={getInvoiceDate()}
                          onChange={handleInvoiceDateChange}
                          dateFormat="MM/dd/yyyy"
                          placeholderText="Select date"
                          
                        />
                    </DataList.ItemValue>
                  </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                  <DataList.Item>
                    <DataList.ItemLabel>Received Date</DataList.ItemLabel>
                    <DataList.ItemValue>
                       <DatePicker 
                          selected={getReceivedDate()}
                          onChange={handleReceivedDateChange}
                          dateFormat="MM/dd/yyyy"
                          placeholderText="Select date"
                        />
                    </DataList.ItemValue>
                  </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                  <DataList.Item>
                    <DataList.ItemLabel>Due Date</DataList.ItemLabel>
                    <DataList.ItemValue>
                      <DatePicker 
                          selected={getDueDate()}
                          onChange={handleDueDateChange}
                          dateFormat="MM/dd/yyyy"
                          placeholderText="Select date"
                        />
                    </DataList.ItemValue>
                  </DataList.Item>
                </DataList.Root>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                  <DataList.Item>
                    <DataList.ItemLabel>Is Paid</DataList.ItemLabel>
                    <DataList.ItemValue>
                      <Checkbox.Root checked={watch('is_paid')}>
                        <Checkbox.HiddenInput {...register('is_paid')} />
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
                    <DataList.ItemValue>{headerModel?.association_number}&nbsp;<FaExternalLinkAlt size="15" /></DataList.ItemValue>
                  </DataList.Item>
                </DataList.Root>
                <div style={{marginTop: "50px"}}><h3>Purchase Order Receive</h3></div>
                <DataList.Root size="lg" orientation="horizontal" divideY="1px" maxW="md">
                  <DataList.Item>
                    <DataList.ItemLabel>First Receive Date</DataList.ItemLabel>
                    <DataList.ItemValue>{massageReceievedDate()}</DataList.ItemValue>
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
                    <Tabs.Trigger value="prev-lines">Previous Entries</Tabs.Trigger>
                    <Tabs.Trigger value="rec-lines">Received Lines</Tabs.Trigger>
                    <Tabs.Trigger value="documents">Documents</Tabs.Trigger>
                    <Tabs.Trigger value="comments">Comments</Tabs.Trigger>
                  </Tabs.List>
                  <Tabs.Content value="invoice-lines">
                      <div style={{ width: "100%", height: "500px" }}>
                        <AgGridReact
                            rowData={rowAPData}
                            columnDefs={colDefs}
                            defaultColDef={defaultColDef}
                            stopEditingWhenCellsLoseFocus={true}
                            singleClickEdit={true}
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
                  <Tabs.Content value="prev-lines">
                      <div style={{ width: "100%", height: "500px" }}>
                        <AgGridReact
                            rowData={rowPrevData}
                            columnDefs={colPrevDefs}
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

            <GridItem colSpan={5} >
              <PageActionsComponent 
                canSave={!saveable || !hasEditPermission} 
                canDelete={hasDeletePermission}
                onSave={handleSaveClick} 
                onDelete={handleDeleteClick} 
                successSaved={successSaved}
                failedSaved={failedSaved}
              />
            </GridItem>
        </Grid>
      </form>
  )
}

export default EditAPPage;