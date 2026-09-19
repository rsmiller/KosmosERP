"use client"

import '../../../../styles/page.component.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Button,
  GridItem,
  Field,
} from '@chakra-ui/react'

import { useEffect, useState, useRef } from "react";
import { PurchaseOrderLineDto, PurchaseOrderHeaderDto, PurchaseOrderHeaderEditCommand, PurchaseOrderHeaderDeleteCommand, PurchaseOrderLineEditCommand, PurchaseOrderLineDeleteCommand } from '@/models/purchase-order-models';
import VendorCombobox, { VendorComboboxRef } from '@/components/vendor-combobox';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import HeaderTypeSelectorCombobox from '@/components/header-type-selector';
import PageActionsComponent from '@/components/page-actions';
import { purchaseOrderService } from '@/services/purchase-order-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useAuth } from '@/lib/auth/auth-context';
import { useParams, useRouter } from 'next/navigation';
import AddPurchaseOrderLineDialog, { AddPurchaseOrderLineDialogRef } from '@/components/dialogs/add-purchase-order-line';


ModuleRegistry.registerModules([AllCommunityModule]);


function EditPurchaseOrdersPage() {
  const params = useParams();
  const router = useRouter();

  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);
  const [hasDeletePermission, setHasDeletePermission] = useState(false);

  const [purchaseOrder, setPurchaseOrder] = useState<PurchaseOrderHeaderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);

  const [openDialog, setOpenDialog] = useState(false);

  const [completedOrDisabled, setCompletedOrDisabled] = useState(false);

  const vendorComboboxRef = useRef<VendorComboboxRef>(null);
  const addPurchaseOrderLineDialogRef = useRef<AddPurchaseOrderLineDialogRef>(null);
  const hasInitialized = useRef(false);

  
  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    setValue,
    watch,
    control,
  } = useForm<PurchaseOrderHeaderEditCommand>();


  const loadPurchaseOrder = async () => {
      try {
        setLoading(true);
        const purchaseOrderId = String(params.id);
        
        if (purchaseOrderId == "") {
          setError('Invalid purchase order ID');
          return;
        }

        const response = await purchaseOrderService.getByGuid(purchaseOrderId, auth.token || "");
        if (response.success && response.data) {
          setPurchaseOrder(response.data);
          
          // Set form values with loaded data
          setValue('id', response.data.id as number);
          setValue('vendor_id', response.data.vendor_id || 0);
          setValue('po_type', response.data.po_type || '');
          setValue('po_number', response.data.po_number || 0);
          setValue('price', response.data.price || 0);
          setValue('tax', response.data.tax || 0);
          setValue('po_quote_number', response.data.po_quote_number || '');
          setValue('is_complete', response.data.is_complete || false);
          setValue('is_canceled', response.data.is_canceled || false);

          //console.log(response)

          if(response.data.purchase_order_lines)
          {
            RenderLines(response.data.purchase_order_lines);
          }
          
          // If this is released we can't edit this
          if(response.data.po_type == 'R')
          {
            setCompletedOrDisabled(true);
            canSave(false);
          }

        } else {
          setError('Failed to load purchase order');
        }
      } catch (err) {
        console.error('Error loading purchase order:', err);
        setError('Error loading purchase order');
      } finally {
        setLoading(false);
      }
  };

  useEffect(() => {

    if(auth.authenticated == false) return; 

    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.PurchaseOrderModule,
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
      ERPModules.PurchaseOrderModule,
      ERPModulePermission.Edit,
      realmRoles
    );
    setHasEditPermission(canEdit);

    // Check Delete permission
    const canDelete = permissionsService.HasPermission(
      ERPModules.PurchaseOrderModule,
      ERPModulePermission.Delete,
      realmRoles
    );
    setHasDeletePermission(canDelete);

    if (hasInitialized.current) return;
    hasInitialized.current = true;
    

    loadPurchaseOrder();
  }, [params.id, setValue, setPurchaseOrder, auth.authenticated]);


  const RenderLines = (purchase_order_lines: PurchaseOrderLineDto[]) =>
  {
    setRowData([]);

    for(let i=0; i<purchase_order_lines.length;i++)
    {
      let line = purchase_order_lines[i];

      setRowData(prev => [...prev, {  id: line.id, 
                                      line_number: line.line_number, 
                                      product_id: line.product_id, 
                                      product_name: line.product_name, 
                                      description: line.description, 
                                      quantity: line.quantity, 
                                      unit_price: line.unit_price, 
                                      guid: line.guid,
                                      purchase_order_header_id: line.purchase_order_header_id }]);
    }
  }

  const CheckFormValidity = () => {
    const vendorValid = vendorComboboxRef.current?.isValid() || false;
    const hasRequiredFields = Boolean(watch('po_type') && watch('vendor_id'));
    const allValid = vendorValid && hasRequiredFields;

    if(completedOrDisabled == false)
    {
      canSave(allValid);
    }
    
  };

  const handleVendorSelect = (value: any) => {
    setValue('vendor_id', value?.id || 0);
    CheckFormValidity();
  }


  const handleDeleteClick = async () => {
    let command = new PurchaseOrderHeaderDeleteCommand();
    command.id = purchaseOrder?.id;

    //console.log(command)
    //return;

    try {
      await purchaseOrderService.delete(command, auth.token || "").then((response) => {
        if (response.success) {
          router.push("/erp/purchaseorders/");
        } else {
          setSuccessSaved(false);
          setFailedSaved(true);
        }
      });
    } catch (e) {
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const handleSaveClick = async () => {
    setSuccessSaved(false);

    let command = new PurchaseOrderHeaderEditCommand();
    command.id = purchaseOrder?.id;
    command.vendor_id = watch('vendor_id');
    command.po_type = watch('po_type');
    command.po_number = watch('po_number');
    command.price = watch('price');
    command.tax = watch('tax');
    command.po_quote_number = watch('po_quote_number');
    command.is_complete = Boolean(watch('is_complete'));
    command.is_canceled = Boolean(watch('is_canceled'));

    // Convert rowData to PurchaseOrderLineEditCommand array
    const purchaseOrderLines: PurchaseOrderLineEditCommand[] = rowData.map((line, index) => ({
      id: line.id,
      purchase_order_header_id: purchaseOrder?.id,
      product_id: line.product_id,
      line_number: line.line_number || index + 1,
      quantity: line.quantity,
      description: line.description,
      unit_price: line.unit_price,
      tax: line.tax || 0,
      is_taxable: line.is_taxable || false,
      is_complete: line.is_complete || false,
      is_canceled: line.is_canceled || false,
    }));

    command.purchase_order_lines = purchaseOrderLines;
    
    //console.log(command)
    //return;

    try {
      await purchaseOrderService.update(command, auth.token || "").then((response) => {
        console.log(response)

        if (response.success) {
          setSuccessSaved(true);
          setFailedSaved(false);
        } else {
          setSuccessSaved(false);
          setFailedSaved(true);
        }
      });
    } catch (e) {
      console.error(e);
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  };

  const handleDeleteLineClick = async (id: number, purchase_order_header_id: number) => {

    let command = new PurchaseOrderLineDeleteCommand();
    command.id = id;

    //console.log(command)
    //return;

    try 
    {
      if(id && purchase_order_header_id)
      {
        await purchaseOrderService.deleteLine(command, auth.token || "").then( async (response) => 
        {
          await purchaseOrderService.get(purchase_order_header_id, auth.token || "").then((line_response) => {
            if (line_response.success && line_response.data && line_response.data.purchase_order_lines) 
            {
              RenderLines(line_response.data.purchase_order_lines);
            }
          });
        });
      }
    }
    catch (e) 
    {
        console.error(e);
        setSuccessSaved(false);
        setFailedSaved(true);
    }

  };


  const handleTypeSelect = (value: any) => {
    setValue('po_type', value?.value || '');
    CheckFormValidity();
  };

  const IsDirty = (formName: any) => {
    if (watch(formName) == undefined || watch(formName) == null || watch(formName) == '') {
      return true;
    }
    return false;
  };


  const handleAddSelect = async (line: any) =>
  {
    //console.log(line);
    setOpenDialog(false);
    line.purchase_order_header_id = watch('id');
    line.line_number = rowData.length + 1;

    try
    {
      await purchaseOrderService.createLine(line, auth.token || "").then( (response) =>
      {
        //console.log(response)
        if(response.success && response.data)
        {
          var line = response.data;

          setRowData(prev => [...prev, {  id: line.id, 
                            line_number: line.line_number, 
                            product_id: line.product_id, 
                            product_name: line.product_name, 
                            description: line.description, 
                            quantity: line.quantity, 
                            unit_price: line.unit_price, 
                            guid: line.guid,
                            purchase_order_header_id: line.purchase_order_header_id }]);
        }
        
      });
    }
    catch(e)
    {
      console.error(e);
      setSuccessSaved(false);
      setFailedSaved(true);
    }
  }



  const [rowData, setRowData] = useState<PurchaseOrderLineDto[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<PurchaseOrderLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "description", headerName: "Description",
        editable: true,
        cellEditor: 'agTextCellEditor' 
    },
    { field: "quantity", headerName: "Quantity",
        editable: true,
        cellEditor: 'agNumberCellEditor',
        cellEditorParams: {
            min: 0
        } 
    },
    { field: "unit_price", headerName: "Price",
        editable: true,
        cellEditor: 'agNumberCellEditor'
    },
    {
      field: "id",
      headerName: "Actions",
      cellRenderer: (props: any) => {
        const { completedOrDisabled } = props.context;
        return ( 
          <div>
            <Button type="button" colorPalette="red" onClick={() => handleDeleteLineClick(props.data.id, props.data.purchase_order_header_id)} disabled={completedOrDisabled}>Delete</Button>
          </div>
        );
      }
    }
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
    return <div>Loading purchase order...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!purchaseOrder) {
    return <div>Purchase order not found</div>;
  }

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
              <h1>Edit Purchase Order</h1>
            </GridItem>
            <GridItem colSpan={1} >
              <Field.Root invalid={!!errors.po_type}>
                <HeaderTypeSelectorCombobox
                  ref={vendorComboboxRef}
                  dbKey={watch('po_type') || ''}
                  title="PO Type"
                  control={control}
                  name="po_type"
                  error={errors.po_type}
                  onChange={handleTypeSelect} 
                  disabled={completedOrDisabled}/>
              </Field.Root>
            </GridItem>

            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.vendor_id} required={true}>
                <Field.Label><Field.RequiredIndicator /> Vendor</Field.Label>
                <VendorCombobox 
                  ref={vendorComboboxRef}
                  dbKey={watch('vendor_id') || 0}
                  control={control}
                  name="vendor_id"
                  error={errors.vendor_id}
                  onChange={handleVendorSelect}
                  onValidationChange={CheckFormValidity}
                  disabled={completedOrDisabled}
                />
              </Field.Root>
            </Stack>

            <GridItem colSpan={5} >
              <div style={{ width: "100%", height: "500px" }}>
                <Button onClick={() => setOpenDialog(!openDialog)} disabled={completedOrDisabled} colorPalette="blue">Add New Line</Button>
                <AgGridReact
                    rowData={rowData}
                    columnDefs={colDefs}
                    defaultColDef={defaultColDef}
                    context={{ completedOrDisabled }}
                  />
                </div>
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
            <AddPurchaseOrderLineDialog
              openDialog={openDialog}
              ref={addPurchaseOrderLineDialogRef}
              control={control}
              name="add_dialog"
              onChange={handleAddSelect}
            />
        </Grid>
      </form>
  )
}

export default EditPurchaseOrdersPage;