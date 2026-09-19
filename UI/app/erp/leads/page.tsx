"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule, GridReadyEvent } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { LeadFindCommand, LeadListDto } from '@/models/lead-models';
import { leadService } from '@/services/lead-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);


function LeadsPage() {

  const router = useRouter();
  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const [rowData, setRowData] = useState<LeadListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);
  const hasInitialized = useRef(false);

  useEffect(() => {
    if (hasInitialized.current) return;
    hasInitialized.current = true;
    
    if(auth.authenticated == false) return;

    // Check permission
    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.LeadModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    setHasEditPermission(permissionsService.HasPermission(
      ERPModules.LeadModule,
      ERPModulePermission.Edit,
      realmRoles
    ));

    /// FETCH DATA
    const fetchData = async () => {
      
    };
    fetchData();
    
  }, [auth.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/leads/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/leads/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/leads/edit/" + guid);
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

    let command = new LeadFindCommand();

    await leadService.find(command, auth.token || "", pageStart, pageSize).then((response) => {

      setLoading(false);

      if(response.success && response.data)
      {
        for(let i=0;i<response.data?.length; i++)
        {
          let record = response.data[i];

          setRowData(prev => [...prev, { 
            id: record.id, 
            company_name: record.company_name, 
            first_name: record.first_name, 
            last_name: record.last_name, 
            owner_name: record.owner_name, 
            lead_stage: record.lead_stage,
            stage_name: record.stage_name,
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

  const colDefs = useMemo<ColDef<LeadListDto>[]>(() => [
    { field: "company_name", headerName: "Customer Name" },
    { headerName: "Lead Name", cellRenderer: (params: any) => { 
      if(params.data.first_name != undefined && params.data.last_name != undefined)
      {
        return params.data.first_name + " " + params.data.last_name ;
      }
      return ''; } 
    },
    { field: "owner_name", headerName: "Owner Name" },
    { field: "stage_name", headerName: "Lead Stage" },
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
            <h1>Leads</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Lead</Button>
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
                  onGridReady={getTableData}
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


export default LeadsPage;