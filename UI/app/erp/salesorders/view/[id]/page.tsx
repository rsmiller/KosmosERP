"use client"

import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import {
  Grid,
  Stack,
  Input,
  GridItem,
  Field,
} from '@chakra-ui/react'

import CustomerCombobox, { CustomerComboboxRef } from '@/components/customer-combobox'
import DatePicker from "react-datepicker";
import PaymentMethodCombobox from '@/components/payment-method-combobox';
import ShipmentMethodCombobox from '@/components/shipment-method-combobox';

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { OrderLineDto, OrderHeaderDto } from '@/models/sales-order-models';
import { AgGridReact } from 'ag-grid-react';
import { useEffect, useRef, useState } from "react";
import AddressSelectorCombobox, { AddressSelectorComboboxRef } from '@/components/address-selector';
import HeaderTypeSelectorCombobox, { HeaderTypeSelectorRef } from '@/components/header-type-selector';

import { CurrencyFormatter} from '@/components/ag-grid/currency-formatter';
import { useParams } from 'next/navigation';
import { orderService } from '@/services/order-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { useKeycloak } from '@react-keycloak/web';
import { useRouter } from 'next/navigation';

ModuleRegistry.registerModules([AllCommunityModule]);

function ViewSalesOrderPage() {
  const { keycloak } = useKeycloak();
  const router = useRouter();
  const params = useParams();
  const [hasAccess, setHasAccess] = useState(true);

  const [salesOrder, setSalesOrder] = useState<OrderHeaderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const customerComboboxRef = useRef<CustomerComboboxRef>(null);
  const headerTypeComboboxRef = useRef<HeaderTypeSelectorRef>(null);
  const billingAddressComboboxRef = useRef<AddressSelectorComboboxRef>(null);
  const shippingAddressComboboxRef = useRef<AddressSelectorComboboxRef>(null);
  const hasInitialized = useRef(false);

  useEffect(() => {
    if(keycloak.authenticated == false) return;

    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.OrderModule,
      ERPModulePermission.Read,
      realmRoles
    );
    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }
  }, [keycloak.authenticated, router]);

  const loadSalesOrder = async () => {
      try {
        setLoading(true);
        const salesOrderId = String(params.id);
        
        if (salesOrderId == "") {
          setError('Invalid sales order ID');
          return;
        }

        const response = await orderService.getByGuid(salesOrderId, keycloak?.token || "");
        //console.log(response)
        if (response.success && response.data) {
          setSalesOrder(response.data);
          
          if(response.data.order_lines) {
            RenderLines(response.data.order_lines);
          }
        } else {
          setError('Failed to load sales order');
        }
      } catch (err) {
        console.error('Error loading sales order:', err);
        setError('Error loading sales order');
      } finally {
        setLoading(false);
      }
  };

  useEffect(() => {

    if(keycloak.authenticated == false) return;

    if (hasInitialized.current) return;
    hasInitialized.current = true;
    

    loadSalesOrder();
  }, [params.id, keycloak.authenticated]);

  const RenderLines = (order_lines: OrderLineDto[]) => {
    setRowData(order_lines);
  };

  const getRequiredDate = () => {
    if (!salesOrder?.required_date) return undefined;
    return new Date(salesOrder.required_date);
  };

  const [rowData, setRowData] = useState<OrderLineDto[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<OrderLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "line_description", headerName: "Description"},
    { field: "quantity", headerName: "Quantity"},
    { field: "unit_price", headerName: "Unit Price", 
      cellRenderer: CurrencyFormatter 
    },
    { headerName: "Total", 
      cellRenderer: (props: any) => 
      {
        const total = props.data.quantity * props.data.unit_price;
        return "$" + total.toFixed(2);
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
    return <div>Loading sales order...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!salesOrder) {
    return <div>Sales order not found</div>;
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
        <h1>View Sales Order - {salesOrder.order_number}</h1>
      </GridItem>
      <GridItem colSpan={1} >
        <Field.Root>
          <HeaderTypeSelectorCombobox
            ref={headerTypeComboboxRef}
            dbKey={salesOrder.order_type || ''}
            title="Order Type"
            onChange={() => {}}
            disabled={true}/>
        </Field.Root>
      </GridItem>

      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Customer</Field.Label>
          <CustomerCombobox 
            ref={customerComboboxRef}
            dbKey={salesOrder.customer_id || 0}
            onChange={() => {}}
            disabled={true}/>
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Order Date</Field.Label>
          <span>{salesOrder.order_date}</span>
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Required Date</Field.Label>
          <DatePicker
            selected={getRequiredDate()}
            onChange={() => {}}
            disabled={true}
            dateFormat="MM/dd/yyyy"
            placeholderText="Select date"
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>PO Number</Field.Label>
          <Input 
            value={salesOrder.po_number || ''}
            disabled={true}
          />
        </Field.Root>
      </Stack>
      <Stack></Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Payment Method</Field.Label>
          <PaymentMethodCombobox 
            dbKey={salesOrder.pay_method || ''}
            onChange={() => {}}
            disabled={true}
          />
        </Field.Root>
      </Stack>
      <Stack gap="4" align="flex-start" maxW="md">
        <Field.Root>
          <Field.Label>Shipping Method</Field.Label>
          <ShipmentMethodCombobox 
            dbKey={salesOrder.shipping_method || ''}
            onChange={() => {}}
            disabled={true}
          />
        </Field.Root>
      </Stack>
      <GridItem colSpan={2}></GridItem>
      <GridItem colSpan={2}>
        <AddressSelectorCombobox 
          title="Billing Address"
          dbKey={salesOrder.billing_address_id || 0}
          customer_id={salesOrder.customer_id || 0}
          onChange={() => {}}
          ref={billingAddressComboboxRef}
          disabled={true}
          hideAddBtn={true}
        />
      </GridItem>
      
      <GridItem colSpan={2}>
        <AddressSelectorCombobox 
          title="Shipping Address"
          dbKey={salesOrder.ship_to_address_id || 0}
          customer_id={salesOrder.customer_id || 0}
          onChange={() => {}}
          ref={shippingAddressComboboxRef}
          disabled={true}
          hideAddBtn={true}
        />
      </GridItem>

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

export default ViewSalesOrderPage;