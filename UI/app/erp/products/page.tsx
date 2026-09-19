"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { ProductFindCommand, ProductListDto } from '@/models/product-models';
import { productService } from '@/services/product-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);


function ProductsPage() {
  const auth = useAuth();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const [rowData, setRowData] = useState<ProductListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);
  const hasInitialized = useRef(false);

  const fetchData = async () => {
      let pageStart = (page * pageSize) - pageSize + 1;

      if(pageStart == 0) {
        pageStart = 1;    
      }

      setRowData([]);
      
      setLoading(true); // Show loading

      let command = new ProductFindCommand();

      await productService.find(command, auth.token || "", pageStart, pageSize).then((response) => {

        setLoading(false);

        if(response.success && response.data)
        {
          for(let i=0;i<response.data?.length; i++)
          {
            let record = response.data[i];

            setRowData(prev => [...prev, { 
              id: record.id, 
              product_name: record.product_name, 
              identifier1: record.identifier1, 
              category_name: record.category_name, 
              product_class: record.product_class, 
              vendor_name: record.vendor_name,
              guid: record.guid 
            }]);
          }
        }
      });
  };

  useEffect(() => {
    if(auth.authenticated == false) return;

    if (hasInitialized.current) return;

    hasInitialized.current = true;

    // Check permission
    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.ProductModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    setHasEditPermission(permissionsService.HasPermission(
      ERPModules.ProductModule,
      ERPModulePermission.Edit,
      realmRoles
    ));

    fetchData();
    
  }, [page, pageSize, auth.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/products/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/products/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/products/edit/" + guid);
  };


  const onPageEvent = async (page: any) =>
  {
    setPage(page);
  }

  const onPageSizeEvent = async (size: any) =>
  {
    setPageSize(size);
  }

  const colDefs = useMemo<ColDef<ProductListDto>[]>(() => [
    { field: "product_name", headerName: "Product Name" },
    { field: "identifier1", headerName: "Identifier" },
    { field: "category_name", headerName: "Category" },
    { field: "product_class", headerName: "Product Class" },
    { field: "vendor_name", headerName: "Vendor Name" },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle"onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
              <Button hidden={!hasEditPermission} type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>
            </div>
          );
      }
    }
  ], [hasEditPermission]);

  const defaultColDef: ColDef = {
    flex: 1,
    filter: true,
    sortable: true,
  };

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

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
        <Grid
            templateColumns="repeat(5, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto"
          >
          <GridItem colSpan={6}>
            <div style={{ width: "100%", height: "500px" }}>
              <AgGridReact
                  loading={loading}
                  rowData={rowData}
                  columnDefs={colDefs}
                  defaultColDef={defaultColDef}
                  modules={[
                    CsvExportModule
                  ]}
              />
            </div>
          </GridItem>
          <GridItem colSpan={3}></GridItem>
          <GridItem colSpan={3}>
            <AgGridCustomPagination totalCount={rowData.length} onPage={onPageEvent} onSizeChange={onPageSizeEvent}></AgGridCustomPagination>
          </GridItem>
        </Grid>
    </div>
  );
}


export default ProductsPage;