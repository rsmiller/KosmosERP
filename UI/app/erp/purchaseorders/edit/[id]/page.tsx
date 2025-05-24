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

import { useEffect, useState } from "react";
import { PurchaseOrderLineDto } from '@/models/purchase-order-models';
import VendorCombobox from '@/components/vendor-combobox';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import HeaderTypeSelectorCombobox from '@/components/header-type-selector';
import PageActionsComponent from '@/components/page-actions';


ModuleRegistry.registerModules([AllCommunityModule]);

interface FormValues {
  required_date: string
  customer_id: number
}


function EditPurchaseOrdersPage() {

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>();


  useEffect(() => {
      console.log("Component initialized (like ngOnInit)");
  }, []);


  const handleVendorSelect = (value: any) => {
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

  const onSubmit = handleSubmit((data) => {
    

  });


  const getTableData = () => 
  {
    setRowData(prev => [...prev, { id: 1, line_number: 1, product_id: 1, product_name: "Miller Welder", description: "Miller Model 203 Welder", quantity: 6, unit_price: 1234, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
    setRowData(prev => [...prev, { id: 2, line_number: 2, product_id: 2, product_name: "Lincoln Welder", description: "Lincoln Model 1200 Welder", quantity: 2, unit_price: 1234, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
    setRowData(prev => [...prev, { id: 3, line_number: 3, product_id: 3, product_name: "Miller Spot Welder", description: "Miller Model 102 Spot Welder", quantity: 4, unit_price: 109, guid: "a2832a33-f023-40dc-961d-67b458f1b00f"  }]);

  };

  const [rowData, setRowData] = useState<PurchaseOrderLineDto[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<PurchaseOrderLineDto>[]>([
    { field: "product_name", headerName: "Product Name"},
    { field: "description", headerName: "Description" },
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
              <h1>200553</h1>
            </GridItem>
            <GridItem colSpan={1} >
              <Field.Root invalid={!!errors.customer_id}>
                <HeaderTypeSelectorCombobox title="PO Type" onChange={handleTypeSelect} />
              </Field.Root>
            </GridItem>

            <Stack gap="4" align="flex-start" maxW="md">
              <Field.Root invalid={!!errors.customer_id}>
                <VendorCombobox onChange={handleVendorSelect} />
              </Field.Root>
            </Stack>

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

export default EditPurchaseOrdersPage;