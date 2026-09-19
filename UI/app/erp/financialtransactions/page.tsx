"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef, useMemo } from "react";
import { Button, Grid, GridItem, Badge } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { FinancialTransactionFindCommand, FinancialTransactionListDto } from '@/models/financial-transaction-models';
import { financialTransactionService } from '@/services/financial-transaction-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdOutlinePageview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function FinancialTransactionsPage() {
  const auth = useAuth();
  const router = useRouter();
  const [hasAccess, setHasAccess] = useState(true);

  const [rowData, setRowData] = useState<FinancialTransactionListDto[]>([]);
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

    let command = new FinancialTransactionFindCommand();

    await financialTransactionService.find(command, auth.token || "", pageStart, pageSize).then((response) => {
      setLoading(false);

      if(response.success && response.data) {
        for(let i=0; i<response.data?.length; i++) {
          let record = response.data[i];

          setRowData(prev => [...prev, { 
            id: record.id, 
            transaction_date: record.transaction_date,
            transaction_type_name: record.transaction_type_name,
            account_number: record.account_number,
            account_name: record.account_name,
            debit_amount: record.debit_amount,
            credit_amount: record.credit_amount,
            running_balance: record.running_balance,
            description: record.description,
            source_module: record.source_module,
            is_reversal: record.is_reversal,
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
      ERPModules.FinancialTransactionModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    fetchData();
    
  }, [page, pageSize, auth.authenticated]);

  const handleViewClick = (guid: any) => {
    router.push("/erp/financialtransactions/view/" + guid);
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

  const colDefs = useMemo<ColDef<FinancialTransactionListDto>[]>(() => [
    { 
      field: "transaction_date", 
      headerName: "Date",
      cellRenderer: (props: any) => formatDate(props.value)
    },
    { field: "transaction_type_name", headerName: "Type" },
    { field: "account_number", headerName: "Account #" },
    { field: "account_name", headerName: "Account Name" },
    { 
      field: "debit_amount", 
      headerName: "Debit",
      cellRenderer: (props: any) => formatCurrency(props.value)
    },
    { 
      field: "credit_amount", 
      headerName: "Credit",
      cellRenderer: (props: any) => formatCurrency(props.value)
    },
    { 
      field: "running_balance", 
      headerName: "Balance",
      cellRenderer: (props: any) => formatCurrency(props.value)
    },
    { 
      field: "is_reversal", 
      headerName: "Reversal",
      cellRenderer: (props: any) => props.value ? <Badge colorPalette="red">Yes</Badge> : ''
    },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
        return ( 
          <div>
            <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>
          </div>
        );
      }
    }
  ], []);

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
        <div style={{ width: "100%", display: "inline-block" }}>
          <h1>Financial Transactions</h1>
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

export default FinancialTransactionsPage;
