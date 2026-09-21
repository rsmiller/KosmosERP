"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import "../../app/styles/page.component.css"


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule, GridReadyEvent } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { PurchaseOrderHeaderListDto, PurchaseOrderHeaderFindCommand } from '@/models/purchase-order-models';
import { purchaseOrderService } from '@/services/purchase-order-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import { FaRegFilePdf } from 'react-icons/fa6';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';

ModuleRegistry.registerModules([AllCommunityModule]);

export class PurchaseOrdersListComponentParams
{
    vendor_id: any;
    onChange: any;
}

function PurchaseOrdersListComponentPage({vendor_id, onChange}: PurchaseOrdersListComponentParams) {
  const auth = useAuth();
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();

  const [rowData, setRowData] = useState<PurchaseOrderHeaderListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);
  const [totalCount, setTotalCount] = useState<number>(1);

  const [loading, setLoading] = useState(true);

  const fetchData = async () => {
      let pageStart = (page * pageSize) - pageSize + 1;

      if(pageStart == 0)
      {
        pageStart = 1;    
      }

      setRowData([]);
      
      setLoading(true); // Show loading

      let command = new PurchaseOrderHeaderFindCommand();

      if(vendor_id)
      {
        command.vendor_id = vendor_id;
      }

      setTotalCount(1);

      
      let response = await purchaseOrderService.find(command, auth.token || "", pageStart, pageSize);
    
      setLoading(false);
      
      if(response.success && response.data)
      {
        console.log(response)

        for(let i=0;i<response.data?.length; i++)
        {
          setTotalCount(response.totalResultCount);

          let record = response.data[i];

          setRowData(prev => [...prev, { 
            id: record.id, 
            po_number: record.po_number, 
            po_type: record.po_type,
            vendor_name: record.vendor_name, 
            created_on: record.created_on, 
            price: record.price,
            guid: record.guid 
          }]);
        }
      }
  };

  useEffect(() => {
    /// FETCH DATA
    if(auth.authenticated == true )
    {
      fetchData();
    }
    
  }, [page, pageSize, auth.authenticated]);

  const handleViewClick = (guid: any) => {
    router.push("/erp/purchaseorders/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/purchaseorders/edit/" + guid);
  };


  const onPageEvent = async (page: any) =>
  {
    setPage(page);
  }

  const onPageSizeEvent = async (size: any) =>
  {
    setPageSize(size);
  }

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<PurchaseOrderHeaderListDto>[]>([
    { field: "po_number", headerName: "Purchase Order #"},
    { field: "po_type", headerName: "Type"},
    { field: "vendor_name", headerName: "Vendor Name" },
    { field: "created_on", headerName: "Created Date", cellRenderer: DateOnlyRender },
    { field: "price", headerName: "Price", cellRenderer: CurrencyFormatter },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle"onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
              <Button type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>&nbsp;
              <Button type="button" colorPalette="gray" variant="outline" onClick={() => window.open(`/docs/api/?url=${encodeURIComponent('/docs/purchaseorder/' + props.value)}`, "_blank") }><FaRegFilePdf /></Button>
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


export default PurchaseOrdersListComponentPage;