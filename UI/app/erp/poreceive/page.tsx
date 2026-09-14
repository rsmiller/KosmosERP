"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule, GridReadyEvent, GridApi } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { PurchaseOrderReceiveHeaderFindCommand, PurchaseOrderReceiveHeaderListDto } from '@/models/po-receive-models';
import { poReceiveService } from '@/services/po-receive-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import ReceivingCountsRenderer from '@/components/ag-grid/receiving-counts-renderer';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { FaMagnifyingGlass, FaPenToSquare } from 'react-icons/fa6';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';


ModuleRegistry.registerModules([AllCommunityModule]);


function ReceievePurchaseOrdersPage() {
  const { keycloak } = useKeycloak();
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const [rowData, setRowData] = useState<PurchaseOrderReceiveHeaderListDto[]>([]);
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
      ERPModules.PurchaseOrderReceiveModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    setHasEditPermission(permissionsService.HasPermission(
      ERPModules.PurchaseOrderReceiveModule,
      ERPModulePermission.Edit,
      realmRoles
    ));

    getTableData();
    
  }, [keycloak.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/poreceive/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/poreceive/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/poreceive/edit/" + guid);
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

    let command = new PurchaseOrderReceiveHeaderFindCommand();

    await poReceiveService.find(command, keycloak.token || "", pageStart, pageSize).then((response)=> {

      setLoading(false);

      if(response.success && response.data)
      {
        for(let i=0;i<response.data?.length; i++)
        {
          let record = response.data[i];

          setRowData(prev => [...prev, { 
            id: record.id, 
            purchase_order_id: record.purchase_order_id, 
            units_ordered: record.units_ordered, 
            units_received: record.units_received, 
            is_complete: record.is_complete,
            completed_on: record.completed_on,
            guid: record.guid,
            po_number: record.po_number
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

  const colDefs = useMemo<ColDef<PurchaseOrderReceiveHeaderListDto>[]>(() => [
    { field: "po_number", headerName: "PO #", cellDataType: 'string'},
    { headerName: "Received/Ordered", cellRenderer: ReceivingCountsRenderer },
    { field: "completed_on", headerName: "Completed On", cellRenderer: DateOnlyRender },
    { field: "is_complete", headerName: "Is Complete?" },
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
            <h1>Receive Purchase Orders</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>Receive PO</Button>
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


export default ReceievePurchaseOrdersPage;