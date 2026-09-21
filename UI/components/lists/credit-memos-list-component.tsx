"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import "../../app/styles/page.component.css"

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { CreditMemoHeaderListDto, CreditMemoHeaderFindCommand } from '@/models/credit-memo-models';
import { creditMemoService } from '@/services/credit-memo-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { FaRegFilePdf } from 'react-icons/fa6';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';

ModuleRegistry.registerModules([AllCommunityModule]);

export class CreditMemosListComponentParams {
  customer_id: any;
  onChange: any;
}

function CreditMemosListComponent({customer_id, onChange}: CreditMemosListComponentParams) {
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const auth = useAuth();
  
  const [rowData, setRowData] = useState<CreditMemoHeaderListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);
  const [totalCount, setTotalCount] = useState<number>(1);
  const [loading, setLoading] = useState(true);

  const handleViewClick = (guid: any) => {
    router.push("/erp/creditmemos/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/creditmemos/edit/" + guid);
  };

  const fetchData = async () => {
      let pageStart = (page * pageSize) - pageSize + 1;
      if (pageStart == 0) pageStart = 1;

      setRowData([]);
      setLoading(true);

      let command = new CreditMemoHeaderFindCommand();

      if (customer_id) {
        command.customer_id = customer_id;
      }

      setTotalCount(1);

      
      await creditMemoService.find(command, auth.token || "", pageStart, pageSize).then( (response) =>
      {
          //console.log(response);
          setLoading(false);

          if (response.success && response.data) 
          {
              setTotalCount(response.totalResultCount);
              setRowData([]);

              for (let i = 0; i < response.data.length; i++) {
                  const record = response.data[i];
                  setRowData(prev => [...prev, {
                      id: record.id,
                      credit_memo_number: record.credit_memo_number,
                      customer_name: record.customer_name,
                      credit_memo_date: record.credit_memo_date,
                      credit_memo_total: record.credit_memo_total,
                      guid: record.guid,
                  }]);
              }
          }
      })
    
};

  useEffect(() => {
    if(auth.authenticated == false) return;

    fetchData();
  }, [page, pageSize, customer_id, auth.authenticated]);

  const onPageEvent = async (p: any) => { setPage(p); };
  const onPageSizeEvent = async (s: any) => { setPageSize(s); };

  const [colDefs, setColDefs] = useState<ColDef<CreditMemoHeaderListDto>[]>([
    { field: "credit_memo_number", headerName: "Credit Memo #" },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "credit_memo_date", headerName: "Date", cellRenderer: DateOnlyRender },
    { field: "credit_memo_total", headerName: "Total", cellRenderer: CurrencyFormatter },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
        return (
          <div>
            <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
            <Button type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>&nbsp;
            <Button type="button" colorPalette="gray" variant="outline" onClick={() => window.open(`/docs/api/?url=${encodeURIComponent('/docs/creditmemo/' + props.value)}`, "_blank") }><FaRegFilePdf /></Button>
          </div>
        );
      }
    }
  ]);

  const defaultColDef: ColDef = { flex: 1, filter: true, sortable: true };

  return (
    <div style={{ width: "100%", height: "500px" }}>
      <Grid templateColumns="repeat(5, 2fr)" gap={6} display="grid" width="100%" p="auto" m="auto">
        <GridItem colSpan={6}>
          <div style={{ width: "100%", height: "500px" }}>
            <AgGridReact loading={loading} rowData={rowData} columnDefs={colDefs} defaultColDef={defaultColDef} modules={[CsvExportModule]} />
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

export default CreditMemosListComponent;
