"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule, GridReadyEvent } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { CustomerFindCommand, CustomerListDto } from '@/models/customer-models';
import { customerService } from '@/services/customer-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);


function CustomersPage() {

  const router = useRouter();
  const { keycloak } = useKeycloak();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const [rowData, setRowData] = useState<CustomerListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);
  const hasInitialized = useRef(false);

  useEffect(() => {

    if(keycloak.authenticated == false) return;

    if (hasInitialized.current) return;

    hasInitialized.current = true;

    // Check permission
    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.CustomerModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    const editPermission = permissionsService.HasPermission(
      ERPModules.CustomerModule,
      ERPModulePermission.Edit,
      realmRoles
    );

    setHasEditPermission(editPermission);

    console.log("Edit Permission: ", editPermission);
    getTableData();
    
  }, [keycloak.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/customers/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/customers/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/customers/edit/" + guid);
  };

  const getTableData = async () =>
  {
    let pageStart = (page * pageSize) - pageSize;

    if(pageStart == 0)
    {
      pageStart = 1;    
    }

    setRowData([]);
    
    setLoading(true); // Show loading

    let command = new CustomerFindCommand();

    await customerService.find(command, keycloak.token || "", pageStart, pageSize).then((response) => {
      console.log(response)
      setLoading(false);

      if(response.success && response.data)
      {
        for(let i=0;i<response.data?.length; i++)
        {
          let record = response.data[i];

          setRowData(prev => [...prev, { 
            id: record.id, 
            customer_number: record.customer_number, 
            customer_name: record.customer_name, 
            payment_terms: record.payment_terms,
            category: record.category,
            payment_terms_name: record.payment_terms_name,
            guid: record.guid 
          }]);
        }
      }
    });
  }


  const onPageEvent = async (page: any) =>
  {
    setPage(page);
    await getTableData();
  }

  const onPageSizeEvent = async (size: any) =>
  {
    setPageSize(size);
    await getTableData();
  }
  

  // Column Definitions: Defines & controls grid columns.
  const colDefs = useMemo<ColDef<CustomerListDto>[]>(() => [
    { field: "customer_number", headerName: "Customer #" },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "category", headerName: "Category"},
    { field: "payment_terms_name", headerName: "Payment Terms" },
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
            <h1>Customers</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Customer</Button>
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


export default CustomersPage;