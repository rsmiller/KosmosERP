"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { ARInvoiceHeaderListDto } from '@/models/ar-models';


ModuleRegistry.registerModules([AllCommunityModule]);


function AccountsReceivablePage() {

  const router = useRouter();

  const handleNewClick = () => {
    router.push("/erp/ar/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/ar/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/ar/edit/" + guid);
  };

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 1, invoice_number: 700001, customer_name: "Bob the Builder", invoice_total: 1000.23, is_paid: false, payment_terms_name: "NET 30", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d68021", invoice_date: new Date("01/21/2025") }]);
    setRowData(prev => [...prev, { id: 2, invoice_number: 700006, customer_name: "Chicken Nugget Farm", invoice_total: 5091.99, is_paid: true, payment_terms_name: "NET 30", guid: "9856993f-f966-43e2-aca7-2eaedfc565f7", invoice_date: new Date("06/30/2025") }]);
    setRowData(prev => [...prev, { id: 3, invoice_number: 700002, customer_name: "We Make Stuff", invoice_total: 90001, is_paid: false, payment_terms_name: "NET 30", guid: "a2832a33-f023-40dc-961d-67b458f1b00f", invoice_date: new Date("04/16/2025") }]);
    setRowData(prev => [...prev, { id: 4, invoice_number: 700003, customer_name: "Soap Investment Firm", invoice_total: 345678.12, is_paid: false, payment_terms_name: "NET 15", guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864", invoice_date: new Date("08/03/2025") }]);
    setRowData(prev => [...prev, { id: 5, invoice_number: 700004, customer_name: "West Palm Beach Investments", invoice_total: 1000.23, payment_terms_name: "NET 30", is_paid: false, guid: "d6a14781-92a7-44e3-becc-661a419963e9", invoice_date: new Date("09/15/2025") }]);
    setRowData(prev => [...prev, { id: 6, invoice_number: 700005, customer_name: "Bill the Truck Driver", invoice_total: 11112.13, payment_terms_name: "NET 90", is_paid: false, guid: "fd87b425-08a8-4f8f-8223-7ef462576df7", invoice_date: new Date("02/23/2025") }]);
    setRowData(prev => [...prev, { id: 7, invoice_number: 700001, customer_name: "Some PLace Over There", invoice_total: 12.23, payment_terms_name: "NET 30", is_paid: true, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c", invoice_date: new Date("01/12/2025") }]);
  }

  const [rowData, setRowData] = useState<ARInvoiceHeaderListDto[]>([]);

  
  const [colDefs, setColDefs] = useState<ColDef<ARInvoiceHeaderListDto>[]>([
    { field: "invoice_number", headerName: "Invoice #" },
    { field: "invoice_date", headerName: "Invoice Date" },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "invoice_total", headerName: "Invoice Total" },
    { field: "payment_terms", headerName: "Payment Terms" },
    { field: "is_paid", headerName: "Is Paid?" },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle"onClick={() => handleViewClick(props.value)}>View</Button>
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
    <div style={{ width: "100%", height: "500px" }}>
        
        <div style={{paddingBottom: "25px"}}>
          <div style={{ width: "49%", display: "inline-block" }}>
            <h1>Account Receivable</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Invoice</Button>
          </div>
        </div>
        <AgGridReact
            rowData={rowData}
            columnDefs={colDefs}
            defaultColDef={defaultColDef}
            onGridReady={getTableData}
            modules={[
              CsvExportModule
            ]}
        />
    </div>
  );
}


export default AccountsReceivablePage;