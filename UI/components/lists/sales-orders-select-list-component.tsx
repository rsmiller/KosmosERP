"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import "../../app/styles/page.component.css"

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";
import { Button, Field, Grid, GridItem, Input } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { OrderHeaderListDto, OrderHeaderFindCommand } from '@/models/sales-order-models';
import { orderService } from '@/services/order-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { useForm } from 'react-hook-form';
import { useAuth } from '@/lib/auth/auth-context';

ModuleRegistry.registerModules([AllCommunityModule]);


export class SalesOrdersSelectListComponentParams
{
    customer_id: any;
    onSelect: any;
}

export class SalesOrderSelectorForm
{
    order_search: any;
}

function SalesOrdersSelectListComponent({customer_id, onSelect}: SalesOrdersSelectListComponentParams) {
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const auth = useAuth();

  const [rowData, setRowData] = useState<OrderHeaderListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);
  const [totalCount, setTotalCount] = useState<number>(1);

  const [loading, setLoading] = useState(true);

  const {
      register,
      formState: { errors, isValid },
      setValue,
      watch,
  } = useForm<SalesOrderSelectorForm>();


  const IsDirty = (formName: any) => {
    if(watch(formName) == undefined || watch(formName) == null || watch(formName) == '')
    {
        return true;
    }

    return false;
  }

  const selectOrder = (order: any) => {
    onSelect(order);
  };

  const fetchData = async (wildcard: string) => {
    let pageStart = (page * pageSize) - pageSize + 1;

    if(pageStart == 0) {
      pageStart = 1;    
    }

    setRowData([]);
    
    setLoading(true); // Show loading

    let command = new OrderHeaderFindCommand();
    command.wildcard = wildcard;

    if(customer_id)
    {
      command.customer_id = customer_id;
    }

    //console.log("pageStart: ", pageStart);
    //console.log("pageSize: ", pageSize);

    setTotalCount(1);

    //console.log(command)

    
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
    
    fetchData('');
    
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
              <Button type="button" colorPalette="blue" onClick={() => selectOrder(props.data) }>Select</Button>
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

  const keyDown = (key: any) => {
    if(key.key == "Enter")
    {
      fetchData(watch('order_search'));
    }
  }

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
        <GridItem colSpan={1}>
            <Field.Root>
                <Input {...register('order_search')} placeholder="Search Orders..."  onKeyDown={(key: any) => keyDown(key)}/>
            </Field.Root>
        </GridItem>
        <GridItem colSpan={6}></GridItem>
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

export default SalesOrdersSelectListComponent;