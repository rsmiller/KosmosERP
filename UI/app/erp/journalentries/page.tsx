"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { Button, Grid, GridItem, Badge } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { JournalEntryHeaderFindCommand, JournalEntryHeaderListDto } from '@/models/journal-entry-models';
import { journalEntryService } from '@/services/journal-entry-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function JournalEntriesPage() {
  const auth = useAuth();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const [rowData, setRowData] = useState<JournalEntryHeaderListDto[]>([]);
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
    
    setLoading(true);

    let command = new JournalEntryHeaderFindCommand();

    await journalEntryService.find(command, auth.token || "", pageStart, pageSize).then((response) => {
      setLoading(false);

      if(response.success && response.data) {
        for(let i=0; i<response.data?.length; i++) {
          let record = response.data[i];

          setRowData(prev => [...prev, { 
            id: record.id, 
            entry_number: record.entry_number, 
            entry_date: record.entry_date,
            description: record.description,
            is_posted: record.is_posted,
            is_reversed: record.is_reversed,
            total_debits: record.total_debits,
            total_credits: record.total_credits,
            line_count: record.line_count,
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

    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.JournalEntryModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    setHasEditPermission(permissionsService.HasPermission(
      ERPModules.JournalEntryModule,
      ERPModulePermission.Edit,
      realmRoles
    ));

    fetchData();
    
  }, [page, pageSize, auth.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/journalentries/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/journalentries/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/journalentries/edit/" + guid);
  };

  const onPageEvent = async (page: any) => {
    setPage(page);
  }

  const onPageSizeEvent = async (size: any) => {
    setPageSize(size);
  }

  const formatCurrency = (value: number | undefined) => {
    if (value === undefined) return '$0.00';
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  };

  const formatDate = (value: string | undefined) => {
    if (!value) return '';
    return new Date(value).toLocaleDateString();
  };

  const colDefs = useMemo<ColDef<JournalEntryHeaderListDto>[]>(() => [
    { field: "entry_number", headerName: "Entry #" },
    { 
      field: "entry_date", 
      headerName: "Date",
      cellRenderer: (props: any) => formatDate(props.value)
    },
    { field: "description", headerName: "Description" },
    { 
      field: "total_debits", 
      headerName: "Debits",
      cellRenderer: (props: any) => formatCurrency(props.value)
    },
    { 
      field: "total_credits", 
      headerName: "Credits",
      cellRenderer: (props: any) => formatCurrency(props.value)
    },
    { 
      field: "is_posted", 
      headerName: "Status",
      cellRenderer: (props: any) => {
        if (props.data?.is_reversed) {
          return <Badge colorPalette="red">Reversed</Badge>;
        }
        return props.value 
          ? <Badge colorPalette="green">Posted</Badge> 
          : <Badge colorPalette="yellow">Draft</Badge>;
      }
    },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
        const isPosted = props.data?.is_posted;
        return ( 
          <div>
            <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
            <Button hidden={!hasEditPermission || isPosted} type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>
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
          <h1>Journal Entries</h1>
        </div>
        <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
          <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Journal Entry</Button>
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
              modules={[CsvExportModule]}
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

export default JournalEntriesPage;
