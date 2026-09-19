"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule, GridReadyEvent } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { ARInvoiceHeaderFindCommand, ARInvoiceHeaderListDto, vm_OrdersReadyForInvoicing, vw_PartialInvoices } from '@/models/ar-models';
import { arInvoiceService } from '@/services/ar-invoice-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { CurrencyFormatter } from '@/components/ag-grid/currency-formatter';
import { DateOnlyRender } from '@/components/ag-grid/date-only-renderer';
import { FaRegFilePdf } from 'react-icons/fa6';
import { MdOutlinePageview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);


function AccountsReceivablePage() {

  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();

  const [rowRecentData, setRowRecentData] = useState<ARInvoiceHeaderListDto[]>([]);
  const [pageRecent, setPageRecent] = useState<number>(1);
  const [pageRecentSize, setPageRecentSize] = useState<number>(50);

  const [rowReadyData, setRowReadyData] = useState<vm_OrdersReadyForInvoicing[]>([]);
  const [rowPartialData, setRowPartialData] = useState<vw_PartialInvoices[]>([]);

  const auth = useAuth();

  const [loading, setLoading] = useState(true);
  const [hasAccess, setHasAccess] = useState(true);

  useEffect(() => {
    if(auth.authenticated === false)
    {
        return;
    }
    
    // Check permission
    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.ARModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }
    
    getTableData();
    getTableReadyData();
    getTablePartialData();
  }, [auth.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/ar/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/ar/view/" + guid);
  };



  const getTableReadyData = async () => 
  {
    setRowReadyData([]);

    if(auth.authenticated === false)
    {
        return;
    }

    await arInvoiceService.getOrdersReadyForInvoicing(auth.token || "").then((response) => {
      if(response.success && response.data)
      {
        for(let i=0;i<response.data?.length; i++)
        {
          let record = response.data[i];

          setRowReadyData(prev => [...prev, { 
            order_number: record.order_number, 
            order_date: record.order_date, 
            customer_name: record.customer_name, 
            pay_method_name: record.pay_method_name,
            created_by: record.created_by,
            customer_guid: record.customer_guid,
            order_guid: record.order_guid
          }]);
        }
      }
    });
  }

  const getTablePartialData = async () => 
  {
    if(auth.authenticated === false)
    {
        return;
    }

    setRowPartialData([]);

    await arInvoiceService.getPartialInvoices(auth.token || "").then((response) => {
      if(response.success && response.data)
      {
        for(let i=0;i<response.data?.length; i++)
        {
          let record = response.data[i];

          setRowPartialData(prev => [...prev, { 
            order_number: record.order_number, 
            order_date: record.order_date, 
            customer_name: record.customer_name, 
            pay_method_name: record.pay_method_name,
            created_by: record.created_by,
            customer_guid: record.customer_guid,
            order_guid: record.order_guid,
            sold_qty: record.sold_qty,
            remaining_qty: record.remaining_qty,
            invoiced_qty: record.invoiced_qty
          }]);
        }
      }
    });
  }

  const getTableData = async () =>
  {
    if(auth.authenticated === false)
    {
        return;
    }

    let pageRecentStart = (pageRecent * pageRecentSize) - pageRecentSize;

    if(pageRecentStart == 0)
    {
      pageRecentStart = 1;    
    }

    setRowRecentData([]);
    
    setLoading(true); // Show loading

    let command = new ARInvoiceHeaderFindCommand();

    await arInvoiceService.find(command, auth.token || "", pageRecentStart, pageRecentSize).then((response) => {

      setLoading(false);

      if(response.success && response.data)
      {
        for(let i=0;i<response.data?.length; i++)
        {
          let record = response.data[i];

          setRowRecentData(prev => [...prev, { 
            id: record.id, 
            invoice_number: record.invoice_number, 
            invoice_date: record.invoice_date, 
            customer_name: record.customer_name, 
            invoice_total: record.invoice_total,
            payment_terms_name: record.payment_terms_name,
            is_paid: record.is_paid,
            guid: record.guid 
          }]);
        }
      }
    });
  }

  const onRecentPageEvent = async (page: any) =>
  {
    setPageRecent(page);
    await getTableData();
  }

  const onPageRecentSizeEvent = async (size: any) =>
  {
    setPageRecentSize(size);
    await getTableData();
  }

  const handleCreateClick = async (guid: any) => 
  {
    router.push("/erp/ar/new/" + guid);
  }


  const [colDefs, setColDefs] = useState<ColDef<ARInvoiceHeaderListDto>[]>([
    { field: "invoice_number", headerName: "Invoice #" },
    { field: "invoice_date", headerName: "Invoice Date", cellRenderer: DateOnlyRender },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "invoice_total", headerName: "Invoice Total", cellRenderer: CurrencyFormatter },
    { field: "payment_terms_name", headerName: "Payment Terms" },
    { field: "is_paid", headerName: "Is Paid?" },
    {
      field: "guid",
      sortable: false, filter: false,
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
              <Button type="button" colorPalette="gray" variant="outline" onClick={() => window.open(`/docs/api/?url=${encodeURIComponent('/docs/ar/' + props.value)}`, "_blank") }><FaRegFilePdf /></Button>
            </div>
          );
      }
    }
  ]);

  const [colReadyDefs, setColReadyDefs] = useState<ColDef<vm_OrdersReadyForInvoicing>[]>([
    { field: "order_number", headerName: "Order #" },
    { field: "order_date", headerName: "Order Date", cellRenderer: DateOnlyRender },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "created_by", headerName: "Order By" },
    { field: "pay_method_name", headerName: "Payment Method" },
    {
      field: "order_guid",
      sortable: false, filter: false,
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="blue" onClick={() => handleCreateClick(props.value)}>Create Invoice</Button>
            </div>
          );
      }
    }
  ]);

  const [colPartialDefs, setColPartialDefs] = useState<ColDef<vw_PartialInvoices>[]>([
    { field: "order_number", headerName: "Order #" },
    { field: "order_date", headerName: "Order Date", cellRenderer: DateOnlyRender },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "created_by", headerName: "Order By" },
    { field: "pay_method_name", headerName: "Payment Method" },
    { field: "remaining_qty", headerName: "Peices To Invoice" },
    { headerName: "Ord | Inv", sortable: false, filter: false, cellRenderer: (thing: any) => {
        return thing.data.sold_qty + " | " + thing.data.invoiced_qty;
    } },
    {
      field: "order_guid",
      sortable: false, filter: false,
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="blue" onClick={() => handleCreateClick(props.value)}>Create Invoice</Button>&nbsp;
              <Button type="button" colorPalette="gray" variant="outline" onClick={() => window.open(`/docs/api/?url=${encodeURIComponent('/docs/ar/' + props.value)}`, "_blank") }><FaRegFilePdf /></Button>
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

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  return (
    <div style={{ width: "100%", height: "500px" }}>
        
        <div style={{paddingBottom: "25px"}}>
          <div style={{ width: "49%", display: "inline-block" }}>
            <h1>Accounts Receivable</h1>
          </div>
        </div>

        <Grid templateColumns="repeat(5, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto">
          <GridItem colSpan={6}>
            <div style={{ width: "100%", height: "500px" }}>
              <h3>Ready to Invoice</h3>
              <AgGridReact
                  loading={loading}
                  rowData={rowReadyData}
                  columnDefs={colReadyDefs}
                  defaultColDef={defaultColDef}
                  modules={[
                    CsvExportModule
                  ]}
              />
            </div>
          </GridItem>
        </Grid>
        <div style={{margin: "75px"}}></div>
        <Grid templateColumns="repeat(5, 2fr)"
            gap={6}
            display="grid"
            width="100%"
            p="auto"
            m="auto">
          <GridItem colSpan={6}>
            <div style={{ width: "100%", height: "500px" }}>
              <h3>Partial Invoices</h3>
              <AgGridReact
                  loading={loading}
                  rowData={rowPartialData}
                  columnDefs={colPartialDefs}
                  defaultColDef={defaultColDef}
                  modules={[
                    CsvExportModule
                  ]}
              />
            </div>
          </GridItem>
        </Grid>
        <div style={{margin: "75px"}}></div>
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
              <h3>Recent Invoices</h3>
              <AgGridReact
                  loading={loading}
                  rowData={rowRecentData}
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
            <AgGridCustomPagination totalCount={rowRecentData.length} onPage={onRecentPageEvent} onSizeChange={onPageRecentSizeEvent}></AgGridCustomPagination>
          </GridItem>
        </Grid>
        
    </div>
  );
}


export default AccountsReceivablePage;