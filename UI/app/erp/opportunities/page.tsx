"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule, GridReadyEvent } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { OpportunityFindCommand, OpportunityListDto } from '@/models/opportunity-models';
import { opportunityService } from '@/services/opportunity-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';


ModuleRegistry.registerModules([AllCommunityModule]);


function OpportunitiesPage() {
  const { keycloak } = useKeycloak();
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const [rowData, setRowData] = useState<OpportunityListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);

  const [totalCount, setTotalCount] = useState<number>(1);
  const hasInitialized = useRef(false);

  const fetchData = async () => {
      let pageStart = (page * pageSize) - pageSize + 1;

      if(pageStart == 0) {
        pageStart = 1;    
      }

      setRowData([]);

      //console.log("pageStart: ", pageStart);
      //console.log("pageSize: ", pageSize);
      
      setLoading(true); // Show loading
      setTotalCount(1);

      let command = new OpportunityFindCommand();

      await opportunityService.find(command, keycloak.token || "", pageStart, pageSize).then((response) => {

        setLoading(false);

        if(response.success && response.data)
        {
          setTotalCount(response.totalResultCount);

          for(let i=0;i<response.data?.length; i++)
          {
            let record = response.data[i];

            setRowData(prev => [...prev, { 
              id: record.id, 
              opportunity_name: record.opportunity_name, 
              customer_name: record.customer_name, 
              contact_name: record.contact_name, 
              owner_name: record.owner_name, 
              stage: record.stage, 
              stage_name: record.stage_name,
              amount: record.amount,
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
      ERPModules.OpportunityModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    setHasEditPermission(permissionsService.HasPermission(
      ERPModules.OpportunityModule,
      ERPModulePermission.Edit,
      realmRoles
    ));

    fetchData();
    
  }, [page, pageSize, keycloak.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/opportunities/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/opportunities/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/opportunities/edit/" + guid);
  };

  const onPageEvent = async (page: any) =>
  {
    setPage(page);
  }

  const onPageSizeEvent = async (size: any) =>
  {
    setPageSize(size);
  }

  const colDefs = useMemo<ColDef<OpportunityListDto>[]>(() => [
    { field: "opportunity_name", headerName: "Opportunity Name" },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "contact_name", headerName: "Contact Name" },
    { field: "owner_name", headerName: "Owner Name" },
    { field: "stage_name", headerName: "Stage" },
    { field: "amount", headerName: "Amount", cellRenderer: CurrencyFormatter},
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
            <h1>Opportunities</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Opportunity</Button>
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
            <AgGridCustomPagination totalCount={totalCount} onPage={onPageEvent} onSizeChange={onPageSizeEvent}></AgGridCustomPagination>
          </GridItem>
        </Grid>
    </div>
  );
}


export default OpportunitiesPage;