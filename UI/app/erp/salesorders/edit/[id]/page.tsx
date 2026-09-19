
"use client"


import '../../../../styles/date-picker.css';
import '../../../../styles/page.component.css';
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
import { OrderHeaderEditCommand, OrderLineDto, OrderHeaderDto, OrderHeaderDeleteCommand, OrderLineEditCommand, OrderLineDeleteCommand } from '@/models/sales-order-models';
import { AgGridReact } from 'ag-grid-react';
import { useEffect, useRef, useState } from "react";
import AddressSelectorCombobox, { AddressSelectorComboboxRef } from '@/components/address-selector';
import PageActionsComponent from '@/components/page-actions';
import HeaderTypeSelectorCombobox, { HeaderTypeSelectorRef } from '@/components/header-type-selector';

import { CurrencyFormatter} from '@/components/ag-grid/currency-formatter';
import { useParams, useRouter } from 'next/navigation';
import SessionStorage from '@/components/session-storage';
import { orderService } from '@/services/order-service';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';
import { format } from 'date-fns';
import AddSalesOrderLineDialog, { AddSalesOrderLineDialogRef } from '@/components/dialogs/add-sales-order-line';
import { useAuth } from '@/lib/auth/auth-context';

ModuleRegistry.registerModules([AllCommunityModule]);

function EditSalesOrderPage() {
  const auth = useAuth();
  const params = useParams();
  const router = useRouter();

  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);
  const [hasDeletePermission, setHasDeletePermission] = useState(false);

  const [salesOrder, setSalesOrder] = useState<OrderHeaderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [saveable, canSave] = useState(false);
  const [successSaved, setSuccessSaved] = useState(false);
  const [failedSaved, setFailedSaved] = useState(false);
  const [completedOrDisabled, setCompletedOrDisabled] = useState(false);

  const [openLineDialog, setOpenLineDialog] = useState(false);

  const customerComboboxRef = useRef<CustomerComboboxRef>(null);
  const headerTypeComboboxRef = useRef<HeaderTypeSelectorRef>(null);
  const billingAddressComboboxRef = useRef<AddressSelectorComboboxRef>(null);
  const shippingAddressComboboxRef = useRef<AddressSelectorComboboxRef>(null);
  const addSalesOrderLineDialogRef = useRef<AddSalesOrderLineDialogRef>(null);
  const hasInitialized = useRef(false);

  useEffect(() => {
    if(auth.authenticated == false) return;

    const realmRoles = auth.roles || [];
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

    // Check Edit permission
    const canEdit = permissionsService.HasPermission(
      ERPModules.OrderModule,
      ERPModulePermission.Edit,
      realmRoles
    );
    setHasEditPermission(canEdit);

    // Check Delete permission
    const canDelete = permissionsService.HasPermission(
      ERPModules.OrderModule,
      ERPModulePermission.Delete,
      realmRoles
    );
    setHasDeletePermission(canDelete);
  }, [auth.authenticated, router]);

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    setValue,
    watch,
    control,
    trigger,
  } = useForm<OrderHeaderEditCommand>();

  const loadSalesOrder = async () => {
      try {
        setLoading(true);
        const salesOrderId = String(params.id);
        
        if (salesOrderId == "") {
          setError('Invalid sales order ID');
          return;
        }

        const response = await orderService.getByGuid(salesOrderId, auth.token || "");
        console.log(response)
        if (response.success && response.data) {
          setSalesOrder(response.data);
          
          // Set form values with loaded data
          setValue('id', response.data.id as number);
          setValue('customer_id', response.data.customer_id || 0);
          setValue('order_type', response.data.order_type || '');
          setValue('order_number', response.data.order_number || 0);
          setValue('po_number', response.data.po_number || '');
          setValue('price', response.data.price || 0);
          setValue('tax', response.data.tax || 0);
          setValue('shipping_cost', response.data.shipping_cost || 0);
          setValue('shipping_method', response.data.shipping_method || '');
          setValue('pay_method', response.data.pay_method || '');
          setValue('ship_to_address_id', response.data.ship_to_address_id || 0);
          setValue('billing_address_id', response.data.billing_address_id || 0);
          setValue('is_complete', response.data.is_complete || false);
          setValue('is_canceled', response.data.is_canceled || false);
          setValue('required_date', response.data.required_date || undefined);

          if(response.data.order_lines) {
            RenderLines(response.data.order_lines);
          }
          
          // If this is completed we can't edit this
          if(response.data.is_complete || response.data.is_canceled) {
            setCompletedOrDisabled(true);
            canSave(false);
          } else {
            CheckFormValidity();
          }

          if(response.data.order_type == 'R')
          {
            setCompletedOrDisabled(true);
            canSave(false);
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
    if(auth.authenticated == false) return;

    if (hasInitialized.current) return;

    hasInitialized.current = true;
    

    loadSalesOrder();
  }, [params.id, setValue, auth.authenticated]);

  const RenderLines = (order_lines: OrderLineDto[]) => {
    setRowData(order_lines);
  };

  const IsDirty = (formName: any) => {
    if (watch(formName) == undefined || watch(formName) == null || watch(formName) == '') {
      return true;
    }
    return false;
  };

  const CheckFormValidity = () => {
    const billingAddressValid = billingAddressComboboxRef.current?.isValid() || false;
    const shippingAddressValid = shippingAddressComboboxRef.current?.isValid() || false;
    const hasRequiredFields = Boolean(watch('customer_id') && watch('order_type') && watch('required_date'));
    const allValid = billingAddressValid && shippingAddressValid && hasRequiredFields;

    //console.log("billingAddressValid: ", billingAddressValid);
    //console.log("shippingAddressValid: ", shippingAddressValid);
    //console.log("hasRequiredFields: ", hasRequiredFields);
    //console.log("customer_id: ", watch('customer_id'));
    //console.log("order_type: ", watch('order_type'));
    //console.log("required_date: ", watch('required_date'));

    //console.log("allValid: ", allValid);

    if(completedOrDisabled == false)
    {
      canSave(allValid);
    }
  };

  const handleDeleteClick = async () => {
    let command = new OrderHeaderDeleteCommand();
    command.id = salesOrder?.id;

    try {
      await orderService.delete(command, auth.token || "").then((response) => {
        if (response.success) {
          router.push("/erp/salesorders/");
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

    let command = new OrderHeaderEditCommand();
    command.id = salesOrder?.id;
    command.customer_id = watch('customer_id');
    command.order_type = watch('order_type');
    command.order_number = watch('order_number');
    command.po_number = watch('po_number');
    command.price = watch('price');
    command.tax = watch('tax');
    command.shipping_cost = watch('shipping_cost');
    command.shipping_method = watch('shipping_method');
    command.pay_method = watch('pay_method');
    command.ship_to_address_id = watch('ship_to_address_id');
    command.is_complete = Boolean(watch('is_complete'));
    command.is_canceled = Boolean(watch('is_canceled'));
    command.billing_address_id = watch('billing_address_id');


    // Convert rowData to OrderLineEditCommand array
    const orderLines: OrderLineEditCommand[] = rowData.map((line, index) => ({
      id: line.id,
      order_header_id: salesOrder?.id,
      product_id: line.product_id,
      line_number: line.line_number || index + 1,
      line_description: line.line_description,
      quantity: line.quantity,
      unit_price: line.unit_price,
      calling_user_id: Number(userId),
      token: sessionId?.toString(),
    }));

    command.order_lines = orderLines;

    try {
      await orderService.update(command, auth.token || "").then((response) => {
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

  const handleDeleteLineClick = async (lineId: any) => {
    let command = new OrderLineDeleteCommand();
    command.id = lineId;

    //console.log("command", command);
    //console.log(lineId)
    //return;
    try {
      await orderService.deleteLine(command, auth.token || "").then((response) => {
        if (response.success) {
          // Remove the line from the grid
          setRowData(prev => prev.filter(line => line.id !== lineId));
        }
      });
    } catch (e) {
      console.error(e);
    }
  };

  const handleEditLineClick = (lineId: number) => {
    // TODO: Implement line editing dialog
    console.log('Edit line:', lineId);
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
    //console.log("Selected Type: ", value);
    setValue('order_type', value?.value);
    CheckFormValidity();
  };

  const handleBillingSelect = (value: any) => {
    setValue('billing_address_id', value?.id);
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
    //console.log(line)

    if(line != null)
    {
      line.order_header_id = salesOrder?.id;

      await orderService.createLine(line, auth.token || "").then( (response) => 
      {
        //console.log(response);

        if(response.success && response.data !== undefined)
        {
          setRowData(prev => [...prev, response.data!]);
        }
      });
    }
    setOpenLineDialog(false);
  }

  const [rowData, setRowData] = useState<OrderLineDto[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<OrderLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "line_description", headerName: "Description",
      editable: !completedOrDisabled,
     },
    { field: "quantity", headerName: "Quantity",
      editable: !completedOrDisabled,
      cellEditor: 'agNumberCellEditor',
     },
    { field: "unit_price", headerName: "Unit Price", 
      editable: !completedOrDisabled,
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
        const { completedOrDisabled } = props.context;
        return ( 
          <div>
            <Button type="button" colorPalette="red" onClick={() => handleDeleteLineClick(props.value)} disabled={completedOrDisabled}>Delete</Button>
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
    return <div>Loading sales order...</div>;
  }

  if (error) {
    return <div>Error: {error}</div>;
  }

  if (!salesOrder) {
    return <div>Sales order not found</div>;
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
              <h1>Edit Sales Order - {salesOrder.order_number}</h1>
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
                  disabled={completedOrDisabled}/>
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
                    disabled={completedOrDisabled}/>
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root>
                <Field.Label>Order Date</Field.Label>
                <span>{salesOrder.order_date}</span>
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.required_date}>
                <Field.Label>Required Date</Field.Label>
                <DatePicker
                  selected={getRequiredDate()}
                  onChange={requiredDaySelected}
                  disabled={completedOrDisabled}
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
                  disabled={completedOrDisabled}
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
                  disabled={completedOrDisabled}
                />
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.shipping_method}>
                <Field.Label>Shipping Method</Field.Label>
                <ShipmentMethodCombobox 
                  dbKey={watch('shipping_method') || ''}
                  onChange={handleShipmentSelect}
                  disabled={completedOrDisabled}
                />
              </Field.Root>
            </Stack>
            <GridItem colSpan={2}></GridItem>
            <GridItem colSpan={2}>
              <AddressSelectorCombobox 
                title="Select Billing Address"
                dbKey={watch('billing_address_id') || 0}
                customer_id={watch('customer_id') || 0}
                onChange={handleBillingSelect}
                ref={billingAddressComboboxRef}
                control={control}
                name="billing_address_id"
                error={errors.billing_address_id}
                disabled={completedOrDisabled}
                hideAddBtn={true}
              />
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
                disabled={completedOrDisabled}
                hideAddBtn={false}
              />
            </GridItem>

            <GridItem colSpan={5} >
              <div style={{ width: "100%", height: "500px" }}>
                <Button onClick={() => setOpenLineDialog(!openLineDialog)} disabled={completedOrDisabled} colorPalette="blue">Add New Line</Button>
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
                onSave={handleSaveClick} 
                onDelete={handleDeleteClick}
                canSave={!saveable || !hasEditPermission}
                canDelete={hasDeletePermission}
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

export default EditSalesOrderPage;