"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import "../../app/styles/page.component.css"

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useRef, useState } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { OrderHeaderListDto, OrderHeaderFindCommand } from '@/models/sales-order-models';
import { orderService } from '@/services/order-service';
import { reportsService } from '@/services/reports-service';
import SessionStorage from '@/components/session-storage';
import { useAuth } from '@/lib/auth/auth-context';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { FaRegFilePdf } from 'react-icons/fa6';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';

ModuleRegistry.registerModules([AllCommunityModule]);


export class SalesOrdersListComponentParams
{
    customer_id: any;
    onChange: any;
}

function SalesOrdersListComponent({customer_id, onChange}: SalesOrdersListComponentParams) {
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const auth = useAuth();

  const [rowData, setRowData] = useState<OrderHeaderListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);
  const [totalCount, setTotalCount] = useState<number>(1);

  const [loading, setLoading] = useState(true);

  const handleViewClick = (guid: any) => {
    router.push("/erp/salesorders/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/salesorders/edit/" + guid);
  };

  // colDefs is captured once in state, so its cell renderers read the token through a ref.
  const tokenRef = useRef<string>("");
  tokenRef.current = auth.token || "";

  const handlePrintClick = (guid: string) => {
    reportsService.openInNewTab(() => reportsService.getSalesOrderAcknowledgementReport(guid, "pdf", tokenRef.current));
  };

  const fetchData = async () => {
      let pageStart = (page * pageSize) - pageSize + 1;

      if(pageStart == 0) {
        pageStart = 1;    
      }

      setRowData([]);
      
      setLoading(true); // Show loading

      let command = new OrderHeaderFindCommand();

      if(customer_id)
      {
        command.customer_id = customer_id;
      }

      //console.log("pageStart: ", pageStart);
      //console.log("pageSize: ", pageSize);

      setTotalCount(1);

      let response = await orderService.find(command, auth.token || "", pageStart, pageSize);
      //console.log(response)
      setLoading(false);

      if(response.success && response.data) {
        setTotalCount(response.totalResultCount);
        setRowData([]);

        for(let i=0; i<response.data?.length; i++) {
          let record = response.data[i];
          setRowData(prev => [...prev, { 
            id: record.id, 
            order_number: record.order_number, 
            customer_name: record.customer_name, 
            order_date: record.order_date,
            price: record.price,
            po_number: record.po_number,
            guid: record.guid 
          }]);
        }
      }
  };


  useEffect(() => {

    if(auth.authenticated == false) return;

    fetchData();
    
  }, [page, pageSize, auth.authenticated]);

  const onPageEvent = async (page: any) => {
    setPage(page);
  };

  const onPageSizeEvent = async (size: any) => {
    setPageSize(size);
  };

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<OrderHeaderListDto>[]>([
    { field: "order_number", headerName: "Order #"},
    { field: "customer_name", headerName: "Customer Name" },
    { field: "order_date", headerName: "Order Date", cellRenderer: DateOnlyRender },
    { field: "price", headerName: "Price", cellRenderer: CurrencyFormatter },
    { field: "po_number", headerName: "PO #" },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
              <Button type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>&nbsp;
              <Button type="button" colorPalette="gray" variant="outline" onClick={() => handlePrintClick(props.value)}><FaRegFilePdf /></Button>
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
          <AgGridCustomPagination totalCount={totalCount} onPage={onPageEvent} onSizeChange={onPageSizeEvent}></AgGridCustomPagination>
        </GridItem>
      </Grid>
    </div>
  );
}

export default SalesOrdersListComponent;