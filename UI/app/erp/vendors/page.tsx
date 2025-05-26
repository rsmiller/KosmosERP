"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { VendorListDto } from '@/models/vendor-models';


ModuleRegistry.registerModules([AllCommunityModule]);


function VendorsPage() {

  const router = useRouter();

  const handleNewClick = () => {
    router.push("/erp/vendors/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/vendors/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/vendors/edit/" + guid);
  };

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 7, vendor_number: 700001, vendor_name: "Some Cool Customer", is_critial_vendor: true, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" }]);
  }

  const [rowData, setRowData] = useState<VendorListDto[]>([
    { id: 1, vendor_number: 700001, vendor_name: "Some Cool Customer", category: "Frequent", is_critial_vendor: true, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" },
    { id: 3, vendor_number: 700002, vendor_name: "Customer 1234", category: "", is_critial_vendor: false,guid: "a2832a33-f023-40dc-961d-67b458f1b00f"  },
    { id: 4, vendor_number: 700003, vendor_name: "Bob's Home Building Suppplies", category: "First Order", is_critial_vendor: false,guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864"  },
    { id: 5, vendor_number: 700004, vendor_name: "House of Chicken Tenders", category: "Frequent", is_critial_vendor: true, guid: "d6a14781-92a7-44e3-becc-661a419963e9"  },
    { id: 6, vendor_number: 700005, vendor_name: "Another Cool Customer", category: "", is_critial_vendor: false,guid: "fd87b425-08a8-4f8f-8223-7ef462576df7"  },
    { id: 2, vendor_number: 700006, vendor_name: "ABC Achme Inc", category: "", is_critial_vendor: false,guid: "9856993f-f966-43e2-aca7-2eaedfc565f7" },
  ]);

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<VendorListDto>[]>([
    { field: "vendor_number", headerName: "Vendor #" },
    { field: "vendor_name", headerName: "Vendor Name" },
    { field: "category", headerName: "Category" },
    { field: "is_critial_vendor", headerName: "Is Critical?" },
    { field: "approved_on", headerName: "Approved On" },
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
            <h1>Vendors</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Vendor</Button>
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


export default VendorsPage;