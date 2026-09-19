"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState, useRef } from "react";
import { Button, CloseButton, Dialog, Grid, GridItem, Portal } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { InventoryDto } from '@/models/inventory-models';
import { inventoryService } from '@/services/inventory-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { FaCalculator, FaPlus } from 'react-icons/fa6';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';


ModuleRegistry.registerModules([AllCommunityModule]);


function InventoryPage() {

  const router = useRouter();
  const auth = useAuth();
  const [hasAccess, setHasAccess] = useState(true);

  const [rowData, setRowData] = useState<InventoryDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [loading, setLoading] = useState(true);
  const hasInitialized = useRef(false);

  useEffect(() => {
    if(auth.authenticated == false) return;

    if (hasInitialized.current) return;

    hasInitialized.current = true;

    // Check permission
    const realmRoles = auth.roles || [];
    const hasPermission = permissionsService.HasPermission(
      ERPModules.InventoryModule,
      ERPModulePermission.Read,
      realmRoles
    );

    if (!hasPermission) {
      setHasAccess(false);
      router.push('/erp');
      return;
    }

    getTableData();
    
  }, [auth.authenticated]);


  const getTableData = async () =>
  {
    setRowData([]);
    setLoading(true); // Show loading

    try {
      const response = await inventoryService.getCounts(auth.token || "");
      
      setLoading(false);

      if(response.success && response.data)
      {
        setRowData(response.data);
      }
    } catch (error) {
      console.error('Error fetching inventory data:', error);
      setLoading(false);
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

  const [colDefs, setColDefs] = useState<ColDef<InventoryDto>[]>([
    { field: "product_name", headerName: "Product Name" },
    { field: "current_stock", headerName: "Current Stock" },
    { field: "on_hand", headerName: "On Hand" },
    { field: "reserved", headerName: "Reserved" },
    { field: "on_order", headerName: "On Order" },
    { field: "required_stock", headerName: "Required Stock" },
    { field: "to_order", headerName: "To Order" },
    {
      headerName: "Actions",
      field: "product_id",
      cellRenderer: (props: any) => {
        //console.log(props)
          return ( 
            <div>
              <Dialog.Root size="lg">
                <Dialog.Trigger asChild>
                  <Button size="sm">
                    <FaCalculator />
                  </Button>
                </Dialog.Trigger>
                <Portal>
                  <Dialog.Backdrop />
                  <Dialog.Positioner>
                    <Dialog.Content>
                      <Dialog.Header>
                      <Dialog.Title>{props.data.product_name}</Dialog.Title>
                    </Dialog.Header>
                      <Dialog.Body>
                        total_units_sold = <b>{props.data.total_units_sold}</b><br/>
                        total_units_received = <b>{props.data.total_units_received}</b><br/>
                        total_units_shipped = <b>{props.data.total_units_shipped}</b><br/>
                        total_units_purchased = <b>{props.data.total_on_purchased}</b><br/>
                        required_stock_level = <b>{props.data.required_stock}</b><br/>
                        <br/>
                        units_available = total_units_received<b>({props.data.total_units_received})</b> - total_units_sold<b>({props.data.total_units_sold})</b><br/>
                        units_reserved = total_units_sold<b>({props.data.total_units_sold})</b> - total_units_shipped<b>({props.data.total_units_shipped})</b><br/>
                        units_on_order = total_units_purchased<b>({props.data.total_on_purchased})</b> - total_units_received<b>({props.data.total_units_received})</b><br/>
                        <br/>
                        current_stock = units_available<b>({props.data.on_hand})</b> - units_reserved<b>({props.data.reserved})</b><br/>
                        to_order = required_stock_level<b>({props.data.required_stock})</b> - (current_stock<b>({props.data.current_stock})</b> + units_on_order<b>({props.data.on_order}))</b><br/>
                      </Dialog.Body>
                      <Dialog.CloseTrigger asChild>
                        <CloseButton size="sm" />
                      </Dialog.CloseTrigger>
                    </Dialog.Content>
                  </Dialog.Positioner>
                </Portal>
              </Dialog.Root>
              &nbsp; <Button hidden={props.data.to_order <= 0} variant="outline" onClick={() => { router.push("/erp/purchaseorders/order/" + props.value); }}><FaPlus /></Button>
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
            <h1>Inventory Counts</h1>
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
        </Grid>
    </div>
  );
}


export default InventoryPage;