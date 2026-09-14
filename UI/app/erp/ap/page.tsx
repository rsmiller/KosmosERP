"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule, GridReadyEvent } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { APInvoiceHeaderFindCommand, APInvoiceHeaderListDto } from '@/models/ap-models';
import { apInvoiceService } from '@/services/ap-invoice-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);


function AccountsPayablePage() {

  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();

  const [rowData, setRowData] = useState<APInvoiceHeaderListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);
  const hasInitialized = useRef(false);
  const { keycloak } = useKeycloak();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);
  
  useEffect(() => {

    if(keycloak.authenticated == false) return;

    if (hasInitialized.current) return;

    hasInitialized.current = true;

    // Check permission
    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.APModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    setHasEditPermission(permissionsService.HasPermission(
      ERPModules.APModule,
      ERPModulePermission.Edit,
      realmRoles
    ));

    getTableData();
    
  }, [keycloak.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/ap/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/ap/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/ap/edit/" + guid);
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

    let command = new APInvoiceHeaderFindCommand();

    await apInvoiceService.find(command, keycloak?.token || "", pageStart, pageSize).then((response) =>
    {
      setLoading(false);

      if(response.success && response.data)
      {
        for(let i=0;i<response.data?.length; i++)
        {
          let record = response.data[i];

          setRowData(prev => [...prev, { 
            id: record.id, 
            invoice_number: record.invoice_number, 
            invoice_date: record.invoice_date, 
            invoice_due_date: record.invoice_due_date, 
            vendor_name: record.vendor_name, 
            invoice_total: record.invoice_total,
            is_paid: record.is_paid,
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

  const colDefs = useMemo<ColDef<APInvoiceHeaderListDto>[]>(() => [
    { field: "invoice_number", headerName: "Invoice #" },
    { field: "invoice_date", headerName: "Invoice Date", cellRenderer: DateOnlyRender },
    { field: "invoice_due_date", headerName: "Due Date", cellRenderer: DateOnlyRender },
    { field: "vendor_name", headerName: "Vendor Name" },
    { field: "invoice_total", headerName: "Invoice Total", cellRenderer: CurrencyFormatter },
    { field: "is_paid", headerName: "Is Paid?" },
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
            <h1>Accounts Payable</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New AP Invoice</Button>
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


export default AccountsPayablePage;