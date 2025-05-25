"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { ProductListDto } from '@/models/product-models';



ModuleRegistry.registerModules([AllCommunityModule]);


function ProductsPage() {

  const router = useRouter();

  const handleNewClick = () => {
    router.push("/erp/products/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/products/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/products/edit/" + guid);
  };

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 6, product_name: "Welder", category: "FinishedGood", identifier1: "WELDER-01", vendor_name: "", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c", is_labor: false, is_material: true, is_rental_item: false, is_retired: false, is_sales_item: false, is_shippable: false, is_stock: false  }]);
  }

  const [rowData, setRowData] = useState<ProductListDto[]>([
    { id: 1, product_name: "Welding Machine Chassis", category: "Chassis", identifier1: "CHASSIS-WELD1000", vendor_name: "Bob's Metal Works", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c", is_labor: false, is_material: true, is_rental_item: false, is_retired: false, is_sales_item: false, is_shippable: false, is_stock: false },
    { id: 3, product_name: "Power Transformer", category: "Electrical", identifier1: "TRANS-WELD1000", vendor_name: "Black Rifle Electrical", guid: "a2832a33-f023-40dc-961d-67b458f1b00f", is_labor: false, is_material: true, is_rental_item: false, is_retired: false, is_sales_item: false, is_shippable: false, is_stock: false   },
    { id: 4, product_name: "Internal Wiring Kit", category: "Electrical", identifier1: "WIREKIT-WELD1000", vendor_name: "Black Rifle Electrical", guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864", is_labor: false, is_material: true, is_rental_item: false, is_retired: false, is_sales_item: false, is_shippable: false, is_stock: false   },
    { id: 5, product_name: "Digital Display Panel", category: "Electronics", identifier1: "DSP-WELD1000", vendor_name: "Black Rifle Electrical", guid: "d6a14781-92a7-44e3-becc-661a419963e9", is_labor: false, is_material: true, is_rental_item: false, is_retired: false, is_sales_item: false, is_shippable: false, is_stock: false   },
    
  ]);

  
  const [colDefs, setColDefs] = useState<ColDef<ProductListDto>[]>([
    { field: "product_name", headerName: "Product Name" },
    { field: "identifier1", headerName: "Identifier" },
    { field: "category", headerName: "Category" },
    { field: "product_class", headerName: "Product Class" },
    { field: "vendor_name", headerName: "Vendor Name" },
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
            <h1>Products</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Product</Button>
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


export default ProductsPage;