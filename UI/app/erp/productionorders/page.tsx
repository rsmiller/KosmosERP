"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'
import 'react-big-calendar/lib/css/react-big-calendar.css';

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState, useEffect, useRef, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { ProductionOrderHeaderListDto, ProductionOrderHeaderFindCommand } from '@/models/production-orders-models';
import { productionOrderService } from '@/services/production-order-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import {
  Calendar,
  dateFnsLocalizer
} from 'react-big-calendar';
import { format } from 'date-fns/format'
import { parse } from 'date-fns/parse'
import { startOfWeek } from 'date-fns/startOfWeek'
import { getDay } from 'date-fns/getDay'
import { enUS } from 'date-fns/locale/en-US';
import { FaRegFilePdf } from 'react-icons/fa6';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import { useKeycloak } from '@react-keycloak/web';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function ProductionOrdersPage() {
  const { keycloak } = useKeycloak();
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);

  const locales = {
    'en-US': enUS,
  };

  const localizer = dateFnsLocalizer({
    format,
    parse,
    startOfWeek,
    getDay,
    locales,
  });
  
  class CalendarEvent
  {
    id: number = 0;
    title: string = "";
    allDay: boolean = false;
    start: Date = new Date();
    end: Date = new Date();
  }

  const [rowData, setRowData] = useState<ProductionOrderHeaderListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);

  const [eventData, setEventData] = useState<CalendarEvent[]>([]);
  const hasInitialized = useRef(false);


  const handleViewClick = (guid: any) => {
    router.push("/erp/productionorders/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/productionorders/edit/" + guid);
  };

  useEffect(() => {
      if(keycloak.authenticated == false) return;

      if (hasInitialized.current) return;

      hasInitialized.current = true;

      // Check permission
      const realmRoles = keycloak?.tokenParsed?.realm_access?.roles || [];
      const hasPermission = permissionsService.HasPermission(
        ERPModules.ProductionOrderModule,
        ERPModulePermission.Read,
        realmRoles
      );

      if (!hasPermission) {
        setHasAccess(false);
        router.push('/erp');
        return;
      }

      setHasEditPermission(permissionsService.HasPermission(
        ERPModules.ProductionOrderModule,
        ERPModulePermission.Edit,
        realmRoles
      ));

      getTableData();
      
  }, [keycloak.authenticated]);

  const getTableData = async () => {
    let pageStart = (page * pageSize) - pageSize;

    if(pageStart == 0) {
      pageStart = 1;    
    }

    setRowData([]);
    
    setLoading(true); // Show loading

    let command = new ProductionOrderHeaderFindCommand();

    await productionOrderService.find(command, keycloak.token || "", pageStart, pageSize).then((response) => {

      setLoading(false);

      if(response.success && response.data) {
        for(let i=0; i<response.data?.length; i++) {
          let record = response.data[i];

          setRowData(prev => [...prev, { 
            id: record.id, 
            order_number: record.order_number, 
            priority_id: record.priority_id, 
            status_name: record.status_name,
            planned_start_date: record.planned_start_date,
            planned_complete_date: record.planned_complete_date,
            is_complete: record.is_complete,
            guid: record.guid 
          }]);

          if(record.id && record.order_number && record.planned_start_date && record.planned_complete_date)
          {
            let start = parse(record.planned_start_date?.toString(), 'yyyy-MM-dd', new Date());
            let end = parse(record.planned_complete_date?.toString(), 'yyyy-MM-dd', new Date());
            
            setEventData(prev => [...prev, {
              id: record.id,
              title: record.order_number?.toString(),
              allDay: false,
              start: start,
              end: end,
            } as CalendarEvent]);
          }
          
        }
      }

      console.log(eventData)
    });
  };

  const onPageEvent = async (page: any) => {
    setPage(page);
    await getTableData();
  };

  const onPageSizeEvent = async (size: any) => {
    setPageSize(size);
    await getTableData();
  };

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<ProductionOrderHeaderListDto>[]>([
    { field: "order_number", headerName: "Order #"},
    { field: "priority_id", headerName: "Priority"},
    { field: "status_name", headerName: "Status Name" },
    { 
      field: "planned_start_date", 
      headerName: "Planned Start",
      valueFormatter: (params) => {
        if (params.value) {
          return new Date(params.value).toLocaleDateString();
        }
        return '';
      }
    },
    { 
      field: "planned_complete_date", 
      headerName: "Planned Complete",
      valueFormatter: (params) => {
        if (params.value) {
          return new Date(params.value).toLocaleDateString();
        }
        return '';
      }
    },
    { 
      field: "is_complete", 
      headerName: "Is Complete?",
      valueFormatter: (params) => {
        return params.value ? 'Yes' : 'No';
      }
    },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button hidden={!hasEditPermission} type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>&nbsp;
              <Button type="button" colorPalette="gray" variant="outline" onClick={() => window.open(`/docs/api/?url=${encodeURIComponent('/docs/productionorder/' + props.value)}`, "_blank") }><FaRegFilePdf /></Button>
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
      {!hasAccess ? (
        <div>Redirecting...</div>
      ) : (
        <>
      <div style={{paddingBottom: "25px"}}>
        <div style={{ width: "49%", display: "inline-block" }}>
          <h1>Production Orders</h1>
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
                <GridItem colSpan={6}></GridItem>
        <GridItem colSpan={6}>
          <h3>Production Calendar</h3>
          <Calendar
            localizer={localizer}
            events={eventData}
            startAccessor={(e) => e.start as Date}
            endAccessor={(e) => e.end as Date}
            style={{ height: 600 }}
          />
        </GridItem>
      </Grid>
        </>
      )}
    </div>
  );
}

export default ProductionOrdersPage;