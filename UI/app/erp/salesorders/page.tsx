"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { OrderHeaderListDto } from '@/models/sales-order-models';

import { CurrencyFormatter} from '@/components/ag-grid/currency-formatter';

ModuleRegistry.registerModules([AllCommunityModule]);


function SalesOrdersPage() {

  const router = useRouter();

  const handleNewClick = () => {
    router.push("/erp/salesorders/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/salesorders/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/salesorders/edit/" + guid);
  };

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 7, order_number: 100001, order_date: "12/13/2025", customer_name: "Some Cool Customer", price: 64950, po_number: "1234B", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
  }

  const [rowData, setRowData] = useState<OrderHeaderListDto[]>([
    { id: 1, order_number: 100001, order_date: "12/13/2025", customer_name: "Some Cool Customer", price: 64950, po_number: "1234B", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" },
    { id: 2, order_number: 100002, order_date: "12/13/2025", customer_name: "Customer 1234", price: 33850, po_number: "10992", guid: "a2832a33-f023-40dc-961d-67b458f1b00f"  },
    { id: 3, order_number: 100003, order_date: "12/13/2025", customer_name: "Bob's Home Building Suppplies", price: 29600, po_number: "Timmy", guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864"  },
    { id: 4, order_number: 100004, order_date: "12/14/2025", customer_name: "House of Chicken Tenders", price: 48890, po_number: "987722022", guid: "d6a14781-92a7-44e3-becc-661a419963e9"  },
    { id: 5, order_number: 100005, order_date: "12/14/2025", customer_name: "Another Cool Customer", price: 15774, po_number: "76255263", guid: "fd87b425-08a8-4f8f-8223-7ef462576df7"  },
    { id: 6, order_number: 100006, order_date: "12/15/2025", customer_name: "ABC Achme Inc", price: 20675, po_number: "Bob1", guid: "9856993f-f966-43e2-aca7-2eaedfc565f7" },
  ]);

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<OrderHeaderListDto>[]>([
    { field: "order_number", headerName: "Order #"},
    { field: "customer_name", headerName: "Customer Name" },
    { field: "order_date", headerName: "Order Date" },
    { field: "price", headerName: "Price", cellRenderer: CurrencyFormatter },
    { field: "po_number", headerName: "PO #" },
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
            <h1>Sales Orders</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Order</Button>
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