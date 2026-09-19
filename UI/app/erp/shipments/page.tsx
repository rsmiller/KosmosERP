"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useRef, useState, useMemo } from "react";
import { Button, Grid, GridItem } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { ShipmentHeaderListDto, ShipmentHeaderFindCommand, vw_ReadyToShip } from '@/models/shipments-models';
import { shipmentService } from '@/services/shipment-service';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import AddressRenderer from '@/components/ag-grid/address-renderer';
import ShippingCountsRenderer from '@/components/ag-grid/shipping-counts-renderer';
import { FaRegFilePdf } from 'react-icons/fa6';
import { MdEditDocument, MdOutlinePageview, MdPreview } from 'react-icons/md';
import { useAuth } from '@/lib/auth/auth-context';
import { permissionsService, ERPModules, ERPModulePermission } from '@/services/permissions-service';

ModuleRegistry.registerModules([AllCommunityModule]);

function ShipmentsPage() {
  const auth = useAuth();

  const router = useRouter();
  const hasInitialized = useRef(false);
  const [hasAccess, setHasAccess] = useState(true);
  const [hasEditPermission, setHasEditPermission] = useState(false);
  
  const [rowData, setRowData] = useState<ShipmentHeaderListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);
  const [loading, setLoading] = useState(true);

  const [rowReadyToShipData, setRowReadyToShipData] = useState<vw_ReadyToShip[]>([]);
  const [pageReadyToShip, setPageReadyToShip] = useState<number>(1);
  const [pageSizeReadyToShip, setPageSizeReadyToShip] = useState<number>(50);
  const [loadingReadyToShip, setLoadingReadyToShip] = useState(true);
  

  useEffect(() => {
      if(auth.authenticated == false) return;
  
      if (hasInitialized.current) return;
  
      hasInitialized.current = true;
      
      // Check permission
      const realmRoles = auth.roles || [];
      const hasPermission = permissionsService.HasPermission(
        ERPModules.ShippingModule,
        ERPModulePermission.Read,
        realmRoles
      );

      if (!hasPermission) {
        setHasAccess(false);
        router.push('/erp');
        return;
      }

      setHasEditPermission(permissionsService.HasPermission(
        ERPModules.ShippingModule,
        ERPModulePermission.Edit,
        realmRoles
      ));
  
      getTableData();
      getReadyToShipTableData();
  }, [auth.authenticated]);

  const handleNewClick = () => {
    router.push("/erp/shipments/new");
  };

  const handleNewFromOrderClick = (order_guid: string) => {
    router.push("/erp/shipments/new/" + order_guid);
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/shipments/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/shipments/edit/" + guid);
  };

  const getReadyToShipTableData = async () => {
    let pageStart = (pageReadyToShip * pageSizeReadyToShip) - pageSizeReadyToShip;

    if(pageStart == 0) {
      pageStart = 1;    
    }

    setRowReadyToShipData([]);
    
    setLoadingReadyToShip(true);

    await shipmentService.getReadyToShip(auth.token || "").then((response) => {
      //console.log("Ready to ship response:", response);
      if(response.success && response.data) 
      {
        setRowReadyToShipData(response.data);
      }

      setLoadingReadyToShip(false);
    });
  }


  const getTableData = async () => {
    let pageStart = (page * pageSize) - pageSize;

    if(pageStart == 0) {
      pageStart = 1;    
    }

    setRowData([]);
    
    setLoading(true); // Show loading

    let command = new ShipmentHeaderFindCommand();

    await shipmentService.find(command, auth.token || "", pageStart, pageSize).then((response) => {

      setLoading(false);

      if(response.success && response.data) {
        setRowData(response.data);
      }
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


  const colDefs = useMemo<ColDef<ShipmentHeaderListDto>[]>(() => [
    { field: "shipment_number", headerName: "Shipment #" },
    { field: "is_released", headerName: "Released" },
    { field: "ship_via", headerName: "Ship Via" },
    { field: "freight_carrier_name", headerName: "Carrier" },
    { field: "units_to_ship", headerName: "Ship/Shipped", cellRenderer: ShippingCountsRenderer },
    { field: "address", headerName: "Address", cellRenderer: AddressRenderer },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props.data.guid)}><MdOutlinePageview /></Button>&nbsp;
              <Button hidden={props.data.is_released || !hasEditPermission} type="button" colorPalette="green" onClick={() => handleEditClick(props.data.guid)}><MdEditDocument /></Button>&nbsp;
              <Button hidden={!props.data.is_released} type="button" colorPalette="gray" variant="outline" onClick={() => window.open(`/docs/api/?url=${encodeURIComponent('/docs/packinglist/' + props.data.guid)}`, "_blank") }><FaRegFilePdf /></Button>
            </div>
          );
      }
    }
  ], [hasEditPermission]);


  const [colDefsReadyToShip, setColDefsReadyToShip] = useState<ColDef<vw_ReadyToShip>[]>([
    { field: "order_number", headerName: "Order #" },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "product_name", headerName: "Product Name" },
    { field: "sold_quantity", headerName: "Sold Qty" },
    { field: "produced_quantity", headerName: "Produced Qty"},
    { field: "shipped_quantity", headerName: "Shipped Qty"},
    {
      field: "order_guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="blue" onClick={() => handleNewFromOrderClick(props.data.order_guid)}>New Shipment</Button>
            </div>
          );
      }
    }
  ]);

  const defaultColDef: ColDef = {
    flex: 1,
    filter: false,
    sortable: true,
    wrapText: true,
    autoHeight: true, 
  };

  if (!hasAccess) {
    return <div>Redirecting...</div>;
  }

  return (
    <div style={{ width: "100%", height: "500px" }}>
      <div style={{paddingBottom: "25px"}}>
        <div style={{ width: "49%", display: "inline-block" }}>
          <h1>Shipments</h1>
        </div>
        <div style={{ width: "49%", display: "inline-block", textAlign: "right" }}>
          <Button type="submit" colorPalette="blue" onClick={handleNewClick}>New Shipment</Button>
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
        <GridItem colSpan={6}><h3>Ready To Ship</h3></GridItem>
        <GridItem colSpan={6}>
          <div style={{ width: "100%", height: "500px" }}>
            <AgGridReact
              loading={loadingReadyToShip}
              rowData={rowReadyToShipData}
              columnDefs={colDefsReadyToShip}
              defaultColDef={defaultColDef}
              modules={[
                CsvExportModule
              ]}
            />
          </div>
        </GridItem>
        <GridItem colSpan={6}><h3>Recently Shipped</h3></GridItem>
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

export default ShipmentsPage;