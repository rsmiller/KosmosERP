"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { DocumentUploadDto, DocumentUploadRevisionDto } from '@/models/document-models';


ModuleRegistry.registerModules([AllCommunityModule]);


function DocumentsPage() {

  const router = useRouter();

  const handleNewClick = () => {
    router.push("/erp/vendors/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/vendors/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/vendors/edit/" + guid);
  };

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 1, rev_num: 1, document_object_id: 700001, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d68021", document_revisions: revisionRowData, created_on: "01/21/2025" }]);
    setRowData(prev => [...prev, { id: 2, rev_num: 2, document_object_id: 700006, guid: "9856993f-f966-43e2-aca7-2eaedfc565f7", document_revisions: revisionRowData, created_on: "06/30/2025" }]);
    setRowData(prev => [...prev, { id: 3, rev_num: 3, document_object_id: 700002, guid: "a2832a33-f023-40dc-961d-67b458f1b00f", document_revisions: revisionRowData, created_on: "04/16/2025" }]);
    setRowData(prev => [...prev, { id: 4, rev_num: 4, document_object_id: 700003, guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864", document_revisions: revisionRowData, created_on: "08/03/2025" }]);
    setRowData(prev => [...prev, { id: 5, rev_num: 5, document_object_id: 700004, guid: "d6a14781-92a7-44e3-becc-661a419963e9", document_revisions: revisionRowData, created_on: "09/15/2025" }]);
    setRowData(prev => [...prev, { id: 6, rev_num: 6, document_object_id: 700005, guid: "fd87b425-08a8-4f8f-8223-7ef462576df7", document_revisions: revisionRowData, created_on: "02/23/2025" }]);
    setRowData(prev => [...prev, { id: 7, rev_num: 7, document_object_id: 700001, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c", document_revisions: revisionRowData, created_on: "01/12/2025" }]);
  }

  const [rowData, setRowData] = useState<DocumentUploadDto[]>([]);
  const [revisionRowData, setRevisionRowData] = useState<DocumentUploadRevisionDto[]>([
    { id: 1, rev_num: 1, document_name: "Hello1.jpg", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" },
    { id: 2, rev_num: 2, document_name: "Hello2.jpg", guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864" },
    { id: 3, rev_num: 3, document_name: "Hello3.jpg", guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c" },
    { id: 4, rev_num: 4, document_name: "Hello4.jpg", guid: "fd87b425-08a8-4f8f-8223-7ef462576df7" },
    { id: 5, rev_num: 5, document_name: "Hello5.jpg", guid: "d6a14781-92a7-44e3-becc-661a419963e9" },
    { id: 6, rev_num: 6, document_name: "Hello6.jpg", guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864" },
    { id: 7, rev_num: 7, document_name: "Hello7.jpg", guid: "a2832a33-f023-40dc-961d-67b458f1b00f" }
  ]);
  
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
              <Button type="button" colorPalette="black" variant="subtle"onClick={() => handleViewClick(props.value)}>View</Button>
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
            <h1>Documents</h1>
          </div>
          <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
            <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Document</Button>
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


export default DocumentsPage;