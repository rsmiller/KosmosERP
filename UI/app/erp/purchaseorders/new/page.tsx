"use client"

import '../../../styles/page.component.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Button,
  GridItem,
  Field,
} from '@chakra-ui/react'

import { useEffect, useState, useRef } from "react";
import { PurchaseOrderLineDto, PurchaseOrderHeaderCreateCommand, PurchaseOrderLineCreateCommand } from '@/models/purchase-order-models';
import VendorCombobox, { VendorComboboxRef } from '@/components/vendor-combobox';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import HeaderTypeSelectorCombobox from '@/components/header-type-selector';
import PageActionsComponent from '@/components/page-actions';
import { purchaseOrderService } from '@/services/purchase-order-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useKeycloak } from '@react-keycloak/web';
import { useRouter } from 'next/navigation';
import SessionStorage from '@/components/session-storage';
import AddPurchaseOrderLineDialog, { AddPurchaseOrderLineDialogRef } from '@/components/dialogs/add-purchase-order-line';


ModuleRegistry.registerModules([AllCommunityModule]);


function NewPurchaseOrdersPage() {
  const router = useRouter();

  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const { keycloak } = useKeycloak();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasWritePermission, setHasWritePermission] = useState(false);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);

  const [openDialog, setOpenDialog] = useState(false);

  const vendorComboboxRef = useRef<VendorComboboxRef>(null);
  const addPurchaseOrderLineDialogRef = useRef<AddPurchaseOrderLineDialogRef>(null);

  const [rowData, setRowData] = useState<PurchaseOrderLineDto[]>([]);

  const {
    formState: { errors, isValid },
    setValue,
    watch,
    control,
  } = useForm<PurchaseOrderHeaderCreateCommand>();

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
  }, [keycloak.authenticated, router]);

  const CheckFormValidity = () => {
    const vendorValid = vendorComboboxRef.current?.isValid() || false;
    const hasRequiredFields = Boolean(watch('po_type') && watch('vendor_id'));
    const hasLines = Boolean(rowData.length > 0);

    const allValid = vendorValid && hasRequiredFields && hasLines;

    //console.log("vendorValid: ", vendorValid);
    //console.log("hasRequiredFields: ", hasRequiredFields);
    //console.log("allValid: ", allValid);
    //console.log("hasLines: ", hasLines);

    canSave(allValid);
  };

  const handleVendorSelect = (value: any) => {
    setValue('vendor_id', value?.id || 0);
    CheckFormValidity();
  }

  const handleSaveClick = async () => {
    setSuccessSaved(false);

    let command = new PurchaseOrderHeaderCreateCommand();
    command.vendor_id = watch('vendor_id');
    command.po_type = watch('po_type');
    command.po_number = watch('po_number');
    command.price = watch('price');
    command.tax = watch('tax');
    command.po_quote_number = watch('po_quote_number');
    command.is_complete = Boolean(watch('is_complete'));
    command.is_canceled = Boolean(watch('is_canceled'));

    // Convert rowData to PurchaseOrderLineCreateCommand array
    const purchaseOrderLines: PurchaseOrderLineCreateCommand[] = rowData.map((line, index) => ({
      purchase_order_header_id: 0, // Will be set by the backend
      product_id: line.product_id,
      line_number: line.line_number || index + 1,
      quantity: line.quantity,
      description: line.description,
      unit_price: line.unit_price,
      tax: line.tax || 0,
      is_taxable: line.is_taxable || false,
      is_complete: line.is_complete || false,
      is_canceled: line.is_canceled || false,
      calling_user_id: Number(userId),
      token: sessionId?.toString(),
      product_name: line.product_name || "",
    }));

    command.purchase_order_lines = purchaseOrderLines;
    
    try {
      await purchaseOrderService.create(command, keycloak?.token || "").then((response) => {
        console.log(response)

        if (response.success) {
          setSuccessSaved(true);
          setFailedSaved(false);
          // Route to purchase orders list on successful save
          router.push("/erp/purchaseorders");
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

  const handleTypeSelect = (value: any) => {
    setValue('po_type', value?.value || '');
    CheckFormValidity();
  };


  const handleAddSelect = async (line: any) =>
  {
    setOpenDialog(false);
    line.purchase_order_header_id = 0; // Will be set when the header is created
    line.line_number = rowData.length + 1;

    // Add the line to the local state for now
    setRowData(prev => [...prev, {  id: 0, // Temporary ID
                      line_number: line.line_number, 
                      product_id: line.product_id, 
                      product_name: line.product_name, 
                      description: line.description, 
                      quantity: line.quantity, 
                      unit_price: line.unit_price, 
                      guid: '',
                      purchase_order_header_id: 0 }]);
  }

  

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
        return ( 
          <div>
            <Button type="button" colorPalette="red" onClick={() => {
              // Remove the line from local state
              setRowData(prev => prev.filter((_, index) => index !== props.rowIndex));
            }}>Delete</Button>
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
              <h1>New Purchase Order</h1>
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
                  disabled={false} />
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
                  disabled={false}
                />
              </Field.Root>
            </Stack>

            <GridItem colSpan={5} >
              <div style={{ width: "100%", height: "500px" }}>
                <Button onClick={() => setOpenDialog(!openDialog)} colorPalette="blue">Add New Line</Button>
                <AgGridReact
                    rowData={rowData}
                    columnDefs={colDefs}
                    defaultColDef={defaultColDef}
                  />
                </div>
            </GridItem>

            <GridItem colSpan={5} >
              <PageActionsComponent 
                canSave={!saveable || !hasWritePermission} 
                onSave={handleSaveClick} 
                onDelete={undefined}
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

export default NewPurchaseOrdersPage;