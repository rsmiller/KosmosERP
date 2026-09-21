"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import "../../app/styles/page.component.css"

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule, GridReadyEvent } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { DocumentUploadDto, DocumentUploadFindCommand } from '@/models/document-models';
import { documentService } from '@/services/document-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdOutlinePageview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';


ModuleRegistry.registerModules([AllCommunityModule]);


function DocumentsListComponent() {

  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const auth = useAuth();
  
  const [rowData, setRowData] = useState<DocumentUploadDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);

  useEffect(() => {
    /// FETCH DATA
    if(auth.authenticated == false) return;
    
    getTableData();
  }, [auth.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/documents/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/documents/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/documents/edit/" + guid);
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

    let command = new DocumentUploadFindCommand();

    
    let response = await documentService.find(command, auth.token || "", pageStart, pageSize);
  
    setLoading(false);

    if(response.success && response.data)
    {
      for(let i=0;i<response.data?.length; i++)
      {
        let record = response.data[i];

        setRowData(prev => [...prev, { 
          id: record.id, 
          rev_num: record.rev_num, 
          document_object_id: record.document_object_id,
          guid: record.guid 
        }]);
      }
    }
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

  const [colDefs, setColDefs] = useState<ColDef<DocumentUploadDto>[]>([
    { field: "id", headerName: "Document Name", cellRenderer: (props: any) => {
                                    
                                    let revision = props.data.document_revisions.filter((m: any) => m.rev_num == props.data.rev_num);
                                    return revision[0].document_name;
                                  } 
    },
    { field: "rev_num", headerName: "Revision #" },
    { field: "created_on", headerName: "Uploaded On" },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle"onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>
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


export default DocumentsListComponent;