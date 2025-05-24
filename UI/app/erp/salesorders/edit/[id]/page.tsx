
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

import CustomerCombobox from '@/components/customer-combobox'
import DatePicker from "react-datepicker";
import PaymentMethodCombobox from '@/components/payment-method-combobox';
import ShipmentMethodCombobox from '@/components/shipment-method-combobox';

import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { OrderLineDto } from '@/models/sales-order-models';
import { AgGridReact } from 'ag-grid-react';
import { useEffect, useState } from "react";
import AddressSelectorCombobox from '@/components/address-selector';
import PageActionsComponent from '@/components/page-actions';
import HeaderTypeSelectorCombobox from '@/components/header-type-selector';

ModuleRegistry.registerModules([AllCommunityModule]);

interface FormValues {
  required_date: string;
  customer_id: number;
  po_number: string;
}


function EditSalesOrderPage() {

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>();


  useEffect(() => {
      console.log("Component initialized (like ngOnInit)");
  }, []);


  const onSubmit = handleSubmit((data) => {
    

  })

  const handleCustomerSelect = (value: any) => {
    console.log(value);
  }

  const handlePaymentSelect = (value: any) => {
    console.log(value);
  }

  const handleShipmentSelect = (value: any) => {
    console.log(value);
  }

  const handleDeleteClick = (guid: any) => {

  };

  const handleSaveClick = (guid: any) => {

  };

  const handleDeleteLineClick = (guid: any) => {

  };

  const handleEditLineClick = (guid: any) => {

  };

  
  const handleTypeSelect = (value: any) => {

  };

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 1, line_number: 1, product_id: 1, product_name: "Miller Welder", line_description: "Miller Model 203 Welder", quantity: 6, unit_price: 1234, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
    setRowData(prev => [...prev, { id: 2, line_number: 2, product_id: 2, product_name: "Lincoln Welder", line_description: "Lincoln Model 1200 Welder", quantity: 2, unit_price: 1234, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
    setRowData(prev => [...prev, { id: 3, line_number: 3, product_id: 3, product_name: "Miller Spot Welder", line_description: "Miller Model 102 Spot Welder", quantity: 4, unit_price: 109, guid: "a2832a33-f023-40dc-961d-67b458f1b00f"  }]);

  };



  const [rowData, setRowData] = useState<OrderLineDto[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<OrderLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "line_description", headerName: "Description" },
    { field: "quantity", headerName: "Quantity" },
    { field: "unit_price", headerName: "Price" },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="orange" onClick={() => handleEditLineClick(props.value)}>Edit</Button>&nbsp;
              <Button type="button" colorPalette="red" onClick={() => handleDeleteLineClick(props.value)}>Delete</Button>
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
            <GridItem colSpan={4} >
              <h1>1203844</h1>
            </GridItem>
            <GridItem colSpan={1} >
              <Field.Root invalid={!!errors.customer_id}>
                <HeaderTypeSelectorCombobox title="PO Type" onChange={handleTypeSelect} />
              </Field.Root>
            </GridItem>

            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.customer_id}>
                <CustomerCombobox onChange={handleCustomerSelect} />
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root>
                <Field.Label>Order Date</Field.Label>
                <span>10/11/2025</span>
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.required_date}>
                <Field.Label>Required Date</Field.Label>
                <DatePicker />
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.po_number}>
                <Field.Label>PO Number</Field.Label>
                <Input />
              </Field.Root>
            </Stack>
            <Stack></Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.po_number}>
                <PaymentMethodCombobox onChange={handlePaymentSelect} />
              </Field.Root>
            </Stack>
            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.po_number}>
                <ShipmentMethodCombobox onChange={handleShipmentSelect} />
              </Field.Root>
            </Stack>
            <GridItem colSpan={2}></GridItem>
            <GridItem colSpan={2}>
              <AddressSelectorCombobox title="Select Billing Address"/>
            </GridItem>
            
            <GridItem colSpan={2}>
              <AddressSelectorCombobox title="Select Shipping Address"/>
            </GridItem>

            <GridItem colSpan={5} >
              <div style={{ width: "100%", height: "500px" }}>
                <AgGridReact
                    rowData={rowData}
                    columnDefs={colDefs}
                    defaultColDef={defaultColDef}
                    onGridReady={getTableData}
                  />
                </div>
            </GridItem>

            <GridItem colSpan={5} >
              <PageActionsComponent onSave={() => handleSaveClick} onDelete={() => handleDeleteClick}/>
            </GridItem>
        </Grid>
      </form>
  )
}

export default EditSalesOrderPage;