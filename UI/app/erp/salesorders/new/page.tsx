"use client"

import '../../../styles/date-picker.css';
import '../../../styles/page.component.css';
import 'ag-grid-community/styles/ag-theme-quartz.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  Stack,
  Input,
  Button,
  GridItem,
  Field,
} from '@chakra-ui/react'

import CustomerCombobox, { CustomerComboboxRef } from '@/components/customer-combobox'
import DatePicker from "react-datepicker";
import PaymentMethodCombobox from '@/components/payment-method-combobox';
import ShipmentMethodCombobox from '@/components/shipment-method-combobox';

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { OrderHeaderCreateCommand, OrderLineDto, OrderLineCreateCommand } from '@/models/sales-order-models';
import { AgGridReact } from 'ag-grid-react';
import { useEffect, useRef, useState } from "react";
import AddressSelectorCombobox, { AddressSelectorComboboxRef } from '@/components/address-selector';
import PageActionsComponent from '@/components/page-actions';
import HeaderTypeSelectorCombobox, { HeaderTypeSelectorRef } from '@/components/header-type-selector';

import { CurrencyFormatter} from '@/components/ag-grid/currency-formatter';
import { useRouter } from 'next/navigation';

import { orderService } from '@/services/order-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { format } from 'date-fns';
import AddSalesOrderLineDialog, { AddSalesOrderLineDialogRef } from '@/components/dialogs/add-sales-order-line';
import { useKeycloak } from '@react-keycloak/web';

ModuleRegistry.registerModules([AllCommunityModule]);

function NewSalesOrderPage() {
  const { keycloak } = useKeycloak();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasWritePermission, setHasWritePermission] = useState(false);


  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);

  const [openLineDialog, setOpenLineDialog] = useState(false);

  const customerComboboxRef = useRef<CustomerComboboxRef>(null);
  const headerTypeComboboxRef = useRef<HeaderTypeSelectorRef>(null);
  const shippingAddressComboboxRef = useRef<AddressSelectorComboboxRef>(null);
  const addSalesOrderLineDialogRef = useRef<AddSalesOrderLineDialogRef>(null);

  useEffect(() => {
    if(keycloak.authenticated == false) return;

    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.OrderModule,
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

  const {
    register,
    formState: { errors },
    setValue,
    watch,
    control,
  } = useForm<OrderHeaderCreateCommand>();

  const CheckFormValidity = () => {
    const shippingAddressValid = shippingAddressComboboxRef.current?.isValid() || false;
    const hasRequiredFields = Boolean(watch('customer_id') && watch('order_type') && watch('required_date'));
    const allValid = shippingAddressValid && hasRequiredFields;

    
    //console.log("hasRequiredFields: ", hasRequiredFields)
    //console.log("shippingAddressValid: ", shippingAddressValid)
    //console.log("customer_id: ", watch('customer_id'))
    //console.log("order_type: ", watch('order_type'))
    //console.log("required_date: ", watch('required_date'))
    //console.log("allValid: ", allValid)
    canSave(allValid);
  };

  const handleSaveClick = async () => {
    setSuccessSaved(false);

    let command = new OrderHeaderCreateCommand();
    command.customer_id = watch('customer_id');
    command.order_type = watch('order_type');
    command.po_number = watch('po_number');
    command.price = watch('price');
    command.tax = watch('tax');
    command.shipping_cost = watch('shipping_cost');
    command.shipping_method = watch('shipping_method');
    command.pay_method = watch('pay_method');
    command.ship_to_address_id = watch('ship_to_address_id');
    command.required_date = watch('required_date');

    // Convert rowData to OrderLineCreateCommand array
    const orderLines: OrderLineCreateCommand[] = rowData.map((line, index) => ({
      product_id: line.product_id,
      line_number: line.line_number || index + 1,
      line_description: line.line_description,
      quantity: line.quantity,
      unit_price: line.unit_price,
    }));

    command.order_lines = orderLines;

    try {
      await orderService.create(command, keycloak?.token || "").then((response) => {
        if (response.success) {
          setSuccessSaved(true);
          setFailedSaved(false);
          // Route to sales orders list on successful save
          router.push("/erp/salesorders");
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

  const handleCustomerSelect = (value: any) => {
    setValue('customer_id', value?.id);
    CheckFormValidity();
  };

  const handlePaymentSelect = (value: any) => {
    setValue('pay_method', value?.key);
    CheckFormValidity();
  };

  const handleShipmentSelect = (value: any) => {
    setValue('shipping_method', value?.key);
    CheckFormValidity();
  };

  const handleTypeSelect = (value: any) => {
    //console.log(value)
    setValue('order_type', value?.value);
    CheckFormValidity();
  };

  const handleBillingSelect = (value: any) => {
    // Billing address not supported in create command
    CheckFormValidity();
  };

  const handleShippingSelect = (value: any) => {
    setValue('ship_to_address_id', value?.id);
    CheckFormValidity();
  };

  const requiredDaySelected = (value: any) => {
    if( value != null)
    {
        setValue('required_date', format(value, 'yyyy-MM-dd'));
    }
    CheckFormValidity();
  }

  const getRequiredDate = () => {
    const required_date = watch('required_date');

    return required_date ? new Date(required_date + "T00:01:00") : undefined;
  };

  const handleAddLineSelect = async (line: any) =>
  {
    if(line != null)
    {

      // For new orders, we'll add the line to the local state
      // The actual line creation will happen when the order is saved
      const newLine: OrderLineDto = {
        ...line,
        id: Date.now(), // Temporary ID for display
        guid: '',
        product_name: line.product_name || '',
        line_description: line.line_description || '',
        quantity: line.quantity || 0,
        unit_price: line.unit_price || 0,
        shipped_qty: 0,
        attributes: []
      };

      setRowData(prev => [...prev, newLine]);
    }
    setOpenLineDialog(false);
  }

  const [rowData, setRowData] = useState<OrderLineDto[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<OrderLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "line_description", headerName: "Description",
      editable: true,
     },
    { field: "quantity", headerName: "Quantity",
      editable: true,
      cellEditor: 'agNumberCellEditor',
     },
    { field: "unit_price", headerName: "Unit Price", 
      editable: true,
      cellEditor: 'agNumberCellEditor',
      cellRenderer: CurrencyFormatter 
    },
    { headerName: "Total", 
      cellRenderer: (props: any) => 
      {
        const total = props.data.quantity * props.data.unit_price;
        return "$" + total.toFixed(2);
      }
    },
    {
      field: "id",
      headerName: "Actions",
      cellRenderer: (props: any) => {
        return ( 
          <div>
            <Button type="button" colorPalette="red" onClick={() => handleDeleteLineClick(props.value)}>Delete</Button>
          </div>
        );
      }
    }
  ]);

  const handleDeleteLineClick = (lineId: any) => {
    // Remove the line from the grid
    setRowData(prev => prev.filter(line => line.id !== lineId));
  };

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
              <h1>New Sales Order</h1>
            </GridItem>
            <GridItem colSpan={1} >
              <Field.Root invalid={!!errors.order_type}>
                <HeaderTypeSelectorCombobox
                  ref={headerTypeComboboxRef}
                  dbKey={watch('order_type') || ''}
                  title="Order Type"
                  control={control}
                  name="order_type"
                  error={errors.order_type}
                  onChange={handleTypeSelect} 
                  disabled={false}/>
              </Field.Root>
            </GridItem>

            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.customer_id} required={true}>
                <Field.Label><Field.RequiredIndicator /> Customer</Field.Label>
                <CustomerCombobox 
                    ref={customerComboboxRef}
                    dbKey={watch('customer_id') || 0}
                    onChange={handleCustomerSelect}
                    control={control}
                    name="customer_id"
                    error={errors.customer_id}
                    onValidationChange={CheckFormValidity}
                    disabled={false}/>
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root>
                <Field.Label>Order Date</Field.Label>
                <span>{new Date().toLocaleDateString()}</span>
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.required_date}>
                <Field.Label>Required Date</Field.Label>
                <DatePicker
                  selected={getRequiredDate()}
                  onChange={requiredDaySelected}
                  disabled={false}
                  dateFormat="MM/dd/yyyy"
                  placeholderText="Select date"
                />
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.po_number}>
                <Field.Label>PO Number</Field.Label>
                <Input 
                  {...register('po_number')}
                  disabled={false}
                />
              </Field.Root>
            </Stack>
            <Stack></Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.pay_method}>
                <Field.Label>Payment Method</Field.Label>
                <PaymentMethodCombobox 
                  dbKey={watch('pay_method') || ''}
                  onChange={handlePaymentSelect}
                  disabled={false}
                />
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.shipping_method}>
                <Field.Label>Shipping Method</Field.Label>
                <ShipmentMethodCombobox 
                  dbKey={watch('shipping_method') || ''}
                  onChange={handleShipmentSelect}
                  disabled={false}
                />
              </Field.Root>
            </Stack>
            <GridItem colSpan={2}></GridItem>
            <GridItem colSpan={2}>
              {/* Billing address not supported in create command */}
            </GridItem>
            
            <GridItem colSpan={2}>
              <AddressSelectorCombobox 
                title="Select Shipping Address"
                dbKey={watch('ship_to_address_id') || 0}
                customer_id={watch('customer_id') || 0}
                onChange={handleShippingSelect}
                ref={shippingAddressComboboxRef}
                control={control}
                name="ship_to_address_id"
                error={errors.ship_to_address_id}
                disabled={false}
                hideAddBtn={false}
              />
            </GridItem>

            <GridItem colSpan={5} >
              <div style={{ width: "100%", height: "500px" }}>
                <Button onClick={() => setOpenLineDialog(!openLineDialog)} disabled={false} colorPalette="blue">Add New Line</Button>
                <AgGridReact
                    rowData={rowData}
                    columnDefs={colDefs}
                    defaultColDef={defaultColDef}
                  />
                </div>
            </GridItem>

            <GridItem colSpan={5} >
              <PageActionsComponent 
                onSave={handleSaveClick} 
                onDelete={() => {}} // No delete for new orders
                canSave={!saveable || !hasWritePermission}
                canDelete={false}
                successSaved={successSaved}
                failedSaved={failedSaved}
              />
            </GridItem>

            <AddSalesOrderLineDialog
              openDialog={openLineDialog}
              ref={addSalesOrderLineDialogRef}
              control={control}
              name="add_dialog"
              onChange={handleAddLineSelect}
            />

        </Grid>
      </form>
  )
}

export default NewSalesOrderPage;

