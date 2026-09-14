"use client"

import '../../../styles/page.component.css';

import { useForm } from 'react-hook-form'
import {
  Grid,
  GridItem,
} from '@chakra-ui/react'

import { useEffect, useState } from "react";
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import PageActionsComponent from '@/components/page-actions';
import { ShipmentHeaderEditCommand, ShipmentLineDto, ShipmentLineEditCommand } from '@/models/shipments-models';
import { ARInvoiceLineCreateCommand } from '@/models/ar-models';
import { CustomerDto } from '@/models/customer-models';
import { OrderHeaderDto } from '@/models/sales-order-models';
import DisplayItemBlock from '@/components/display-item-block';


ModuleRegistry.registerModules([AllCommunityModule]);


function NewARPage() {

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<ShipmentHeaderEditCommand>();

    const [customerModel, setCustomerModel] = useState<CustomerDto>();
    const [orderModel, setOrderModel] = useState<OrderHeaderDto>();

  useEffect(() => {
    console.log("Component initialized (like ngOnInit)");

    let customer = new CustomerDto();
    customer.customer_number = 12323;
    customer.customer_name = "Bob Mechanical Services";
    customer.payment_terms = "NET30";

    let orderHeader = new OrderHeaderDto();
    orderHeader.order_number = 122733;
    orderHeader.order_date = "8/10/2025";
    orderHeader.pay_method_name = "Card";
    orderHeader.po_number = "Sam123";

    setCustomerModel(customer);
    setOrderModel(orderHeader);
  }, []);
customerModel

  const handleDeleteClick = (guid: any) => {

  };

  const handleSaveClick = (guid: any) => {

  };

  const onSubmit = handleSubmit((data) => {
    

  });


  const getTableData = () => 
  {
    setRowData(prev => [...prev, { line_number: 1, line_description: "Miller Model 203 Welder", invoice_qty: 6, units_ordered: 6, is_taxable: true, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
    setRowData(prev => [...prev, { line_number: 2, line_description: "Lincoln Model 1200 Welder", invoice_qty: 2, units_ordered: 1, is_taxable: false, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
    setRowData(prev => [...prev, { line_number: 3, line_description: "Miller Model 102 Spot Welder", invoice_qty: 4, units_ordered: 0, is_taxable: false, guid: "a2832a33-f023-40dc-961d-67b458f1b00f"  }]);

  };

  const [rowData, setRowData] = useState<ARInvoiceLineCreateCommand[]>([]);

  const [colDefs, setColDefs] = useState<ColDef<ARInvoiceLineCreateCommand>[]>([
    { field: "line_description", headerName: "Line Description", editable: true },
    { headerName: "Units Ordered", cellRenderer: (params: any) => { return params.data.units_ordered } },
    { field: "invoice_qty", headerName: "Units Invoiced", cellEditor: 'agNumberCellEditor', editable: true, cellEditorParams: {
            min: 0,
            max: 999999999
        }},
    { field: "is_taxable", headerName: "Tax", editable: true },
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
            <GridItem colSpan={2}>
                <div><h3>Customer</h3></div>
                <DisplayItemBlock label={"Number"} value={customerModel?.customer_number}/>
                <DisplayItemBlock label={"Name"} value={customerModel?.customer_name}/>
                <DisplayItemBlock label={"Terms"} value={customerModel?.payment_terms}/>
            </GridItem>

            <GridItem colSpan={2}>
                <div><h3>Order</h3></div>
                <DisplayItemBlock label={"Number"} value={orderModel?.order_number}/>
                <DisplayItemBlock label={"PO NUmber"} value={orderModel?.po_number}/>
                <DisplayItemBlock label={"Order Date"} value={orderModel?.order_date}/>
                <DisplayItemBlock label={"Pay Method"} value={orderModel?.pay_method_name}/>
            </GridItem>

            <GridItem colSpan={6} >
              <div style={{ width: "100%", height: "500px" }}>
                <AgGridReact
                    rowData={rowData}
                    columnDefs={colDefs}
                    defaultColDef={defaultColDef}
                    onGridReady={getTableData}
                    stopEditingWhenCellsLoseFocus={true}
                  />
                </div>
            </GridItem>

            <GridItem colSpan={5} >
              <PageActionsComponent saveText={"Save and Print Invoice"} canDelete={false} onSave={() => handleSaveClick} onDelete={() => handleDeleteClick}/>
            </GridItem>
        </Grid>
      </form>
  )
}

export default NewARPage;