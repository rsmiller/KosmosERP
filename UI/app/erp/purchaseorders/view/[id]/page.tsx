"use client"

import '../../../../styles/page.component.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  GridItem,
  Field,
} from '@chakra-ui/react'

import { useEffect, useState, useRef } from "react";
import { PurchaseOrderLineDto, PurchaseOrderHeaderDto } from '@/models/purchase-order-models';
import VendorCombobox, { VendorComboboxRef } from '@/components/vendor-combobox';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import HeaderTypeSelectorCombobox from '@/components/header-type-selector';
import { purchaseOrderService } from '@/services/purchase-order-service';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useParams, useRouter } from 'next/navigation';


ModuleRegistry.registerModules([AllCommunityModule]);

function ViewPurchaseOrdersPage() {
  const router = useRouter();
  const params = useParams();

  const { keycloak } = useKeycloak();
  const [hasAccess, setHasAccess] = useState(true);

  const [purchaseOrder, setPurchaseOrder] = useState<PurchaseOrderHeaderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const vendorComboboxRef = useRef<VendorComboboxRef>(null);

  const hasInitialized = useRef(false);

  const {
    setValue,
    watch,
    control,
  } = useForm<PurchaseOrderHeaderDto>();

  
  const loadPurchaseOrder = async () => {
      try {
        setLoading(true);
        const purchaseOrderId = String(params.id);
        
        if (purchaseOrderId == "") {
          setError('Invalid purchase order ID');
          return;
        }

        const response = await purchaseOrderService.getByGuid(purchaseOrderId, keycloak?.token || "");
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

          if(response.data.purchase_order_lines)
          {
            RenderLines(response.data.purchase_order_lines);
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
    if(keycloak.authenticated == false) return;

    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
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

    if (hasInitialized.current) return;
    
    hasInitialized.current = true;
    

    loadPurchaseOrder();
  }, [params.id, setValue, setPurchaseOrder, keycloak.authenticated]);

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

  const [rowData, setRowData] = useState<PurchaseOrderLineDto[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<PurchaseOrderLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "description", headerName: "Description"},
    { field: "quantity", headerName: "Quantity"},
    { field: "unit_price", headerName: "Price"}
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
    <Grid
          templateColumns="repeat(5, 2fr)"
          gap={6}
          display="grid"
          width="100%"
          p="auto"
          m="auto"
        >
          <GridItem colSpan={4} >
            <h1>View Purchase Order</h1>
          </GridItem>
          <GridItem colSpan={1} >
            <Field.Root>
              <HeaderTypeSelectorCombobox
                ref={vendorComboboxRef}
                dbKey={watch('po_type') || ''}
                title="PO Type"
                control={control}
                name="po_type"
                onChange={() => {}}
                disabled={true}/>
            </Field.Root>
          </GridItem>

          <Stack gap="4" align="flex-start" maxW="md">
            <Field.Root>
              <Field.Label>Vendor</Field.Label>
              <VendorCombobox 
                ref={vendorComboboxRef}
                dbKey={watch('vendor_id') || 0}
                control={control}
                name="vendor_id"
                onChange={() => {}}
                disabled={true}
              />
            </Field.Root>
          </Stack>

          <GridItem colSpan={5} >
            <div style={{ width: "100%", height: "500px" }}>
              <AgGridReact
                  rowData={rowData}
                  columnDefs={colDefs}
                  defaultColDef={defaultColDef}
                />
              </div>
          </GridItem>
      </Grid>
  )
}

export default ViewPurchaseOrdersPage;