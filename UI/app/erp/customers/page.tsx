"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { CustomerListDto } from '@/models/customer-models';


ModuleRegistry.registerModules([AllCommunityModule]);


function SalesOrdersPage() {

  const router = useRouter();

  const handleNewClick = () => {
    router.push("/erp/customers/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/customers/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/customers/edit/" + guid);
  };

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 7, customer_number: 700001, customer_name: "Some Cool Customer", payment_terms: "NET 15", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
  }

  const [rowData, setRowData] = useState<CustomerListDto[]>([
    { id: 1, customer_number: 700001, customer_name: "Some Cool Customer", category: "Frequent", payment_terms: "NET 30", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" },
    { id: 3, customer_number: 700002, customer_name: "Customer 1234", category: "", payment_terms: "NET 30",guid: "a2832a33-f023-40dc-961d-67b458f1b00f"  },
    { id: 4, customer_number: 700003, customer_name: "Bob's Home Building Suppplies", category: "First Order", payment_terms: "NET 30",guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864"  },
    { id: 5, customer_number: 700004, customer_name: "House of Chicken Tenders", category: "Frequent", payment_terms: "NET 30",guid: "d6a14781-92a7-44e3-becc-661a419963e9"  },
    { id: 6, customer_number: 700005, customer_name: "Another Cool Customer", category: "", payment_terms: "NET 60",guid: "fd87b425-08a8-4f8f-8223-7ef462576df7"  },
    { id: 2, customer_number: 700006, customer_name: "ABC Achme Inc", category: "", payment_terms: "NET 90",guid: "9856993f-f966-43e2-aca7-2eaedfc565f7" },
  ]);

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<CustomerListDto>[]>([
    { field: "customer_number", headerName: "Customer #" },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "category", headerName: "Category" },
    { field: "payment_terms", headerName: "Payment Terms" },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle"onClick={() => handleViewClick(props.value)}>View</Button>&nbsp;
              <Button type="button" colorPalette="orange" onClick={() => handleEditClick(props.value)}>Edit</Button>
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
            <h1>Customers</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Customer</Button>
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


export default SalesOrdersPage;