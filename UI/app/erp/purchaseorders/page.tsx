"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { PurchaseOrderHeaderListDto } from '@/models/purchase-order-models';


ModuleRegistry.registerModules([AllCommunityModule]);




function PurchaseOrdersPage() {

  const router = useRouter();

  const handleNewClick = () => {
    router.push("/erp/purchaseorders/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/purchaseorders/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/purchaseorders/edit/" + guid);
  };

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 1, po_number: 100001, created_on: "12/13/2025", vendor_name: "Some Cool Customer", price: 64950, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
    setRowData(prev => [...prev, { id: 2, po_number: 100001, created_on: "12/13/2025", vendor_name: "Some Cool Customer", price: 64950, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
    setRowData(prev => [...prev, { id: 3, po_number: 100002, created_on: "12/13/2025", vendor_name: "Customer 1234", price: 33850, guid: "a2832a33-f023-40dc-961d-67b458f1b00f"  }]);
    setRowData(prev => [...prev, { id: 4, po_number: 100003, created_on: "12/13/2025", vendor_name: "Bob's Home Building Suppplies", price: 29600, guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864"  }]);
    setRowData(prev => [...prev, { id: 5, po_number: 100004, created_on: "12/14/2025", vendor_name: "House of Chicken Tenders", price: 48890, guid: "d6a14781-92a7-44e3-becc-661a419963e9"  }]);
    setRowData(prev => [...prev, { id: 6, po_number: 100005, created_on: "12/14/2025", vendor_name: "Another Cool Customer", price: 15774, guid: "fd87b425-08a8-4f8f-8223-7ef462576df7"  }]);
    setRowData(prev => [...prev, { id: 7, po_number: 100006, created_on: "12/15/2025", vendor_name: "ABC Achme Inc", price: 20675, guid: "9856993f-f966-43e2-aca7-2eaedfc565f7" }]);
  }

  const [rowData, setRowData] = useState<PurchaseOrderHeaderListDto[]>([]);

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<PurchaseOrderHeaderListDto>[]>([
    { field: "po_number", headerName: "Purchase Order #"},
    { field: "vendor_name", headerName: "Vendor Name" },
    { field: "created_on", headerName: "Created Date" },
    { field: "price", headerName: "Price" },
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
            <h1>Purchase Orders</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Purchase Order</Button>
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


export default PurchaseOrdersPage;