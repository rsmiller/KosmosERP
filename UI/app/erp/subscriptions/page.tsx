"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useRef, useState, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { SubscriptionListDto, SubscriptionFindCommand } from '@/models/subscription-models';
import { subscriptionService } from '@/services/subscription-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { format } from 'date-fns';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function SubscriptionsPage() {
  const { keycloak } = useKeycloak();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const [rowData, setRowData] = useState<SubscriptionListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);
  const hasInitialized = useRef(false);
  
  const handleViewClick = (guid: any) => {
    router.push("/erp/subscriptions/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/subscriptions/edit/" + guid);
  };

  const getTableData = async () => {
    let pageStart = (page * pageSize) - pageSize;

    if(pageStart == 0) {
      pageStart = 1;    
    }

    setRowData([]);
    
    setLoading(true); // Show loading

    let command = new SubscriptionFindCommand();

    await subscriptionService.find(command, keycloak?.token || "", pageStart, pageSize).then((response) => {

      setLoading(false);

      if(response.success && response.data) {
        for(let i=0; i<response.data?.length; i++) {
          let record = response.data[i];
          setRowData(prev => [...prev, { 
            id: record.id, 
            subscription_number: record.subscription_number, 
            customer_name: record.customer_name, 
            quantity: record.quantity,
            price: record.price,
            cycle_days: record.cycle_days,
            start_date: record.start_date,
            next_date: record.next_date,
            end_date: record.end_date,
            guid: record.guid 
          }]);
        }
      }
    });
  };

  useEffect(() => {
    if(keycloak.authenticated == false) return;

    if (hasInitialized.current) return;
    hasInitialized.current = true;

    // Check permission
    const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.SubscriptionModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    setHasEditPermission(permissionsService.HasPermission(
      ERPModules.SubscriptionModule,
      ERPModulePermission.Edit,
      realmRoles
    ));

    getTableData();
  }, [keycloak.authenticated]);

  const onPageEvent = async (page: any) => {
    setPage(page);
    await getTableData();
  };

  const onPageSizeEvent = async (size: any) => {
    setPageSize(size);
    await getTableData();
  };

  const formatDateString = (dateString: string | undefined): string => {
      if(!dateString || dateString.trim() === ""){
          return "";
      }

      return format(dateString || "", 'MM/dd/yyyy');
  }

  // Column Definitions: Defines & controls grid columns.
  const colDefs = useMemo<ColDef<SubscriptionListDto>[]>(() => [
    { field: "subscription_number", headerName: "Subscription #" },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "cycle_days", headerName: "Cycle Days" },
    { field: "price", headerName: "Price" },
    { field: "next_date", headerName: "Next Date", cellRenderer: (props: any) => formatDateString(props.value) },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
              <Button hidden={!hasEditPermission} type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>&nbsp;
            </div>
          );
      }
    }
  ], [hasEditPermission]);

  const defaultColDef: ColDef = {
    flex: 1,
    filter: true,
    sortable: true,
    wrapText: true,
    autoHeight: true, 
  };

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  return (
    <div style={{ width: "100%", height: "500px" }}>
      <div style={{paddingBottom: "25px"}}>
        <div style={{ width: "49%", display: "inline-block" }}>
          <h1>Subscriptions</h1>
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

export default SubscriptionsPage;