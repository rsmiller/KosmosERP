"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import '../../styles/page.component.css'


import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useState } from "react";
import { Button } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { ShipmentHeaderListDto } from '@/models/shipments-models';
import { AddressDto } from '@/models/address-models';
import  AddressRenderer from '@/components/ag-grid/address-renderer';
import  ShippingCountsRenderer from '@/components/ag-grid/shipping-counts-renderer';

ModuleRegistry.registerModules([AllCommunityModule]);


function ShipmentsPage() {

  const router = useRouter();

  const handleNewClick = () => {
    router.push("/erp/shipments/new");
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/shipments/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/shipments/edit/" + guid);
  };


  const [addressData, setaddressData] = useState<AddressDto[]>([
    {id: 1, street_address1: "11008 Chicken Nugget Ln", street_address2: "APT 2", city: "Austin", state: "TX", postal_code: "76273", country: "US"},
    {id: 1, street_address1: "12 Welcome Ave", city: "Dallas", state: "TX", postal_code: "54321", country: "US"}
  ]);

  const getTableData = () =>
  {
    setRowData(prev => [...prev, { id: 6, shipment_number: 300009, ship_via: "Freight", freight_carrier: "TBD", units_to_ship: 8, units_shipped: 0, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c", address: addressData[0] }]);
  }

  const [rowData, setRowData] = useState<ShipmentHeaderListDto[]>([
    { id: 1, shipment_number: 300001, ship_via: "FedEx", freight_carrier: "", units_to_ship: 101, units_shipped: 101, guid: "cf741d9a-e4d4-4e17-ac23-0e26f7d6802c", address: addressData[1]  },
    { id: 3, shipment_number: 300002, ship_via: "UPS", freight_carrier: "", units_to_ship: 1, units_shipped: 1, guid: "a2832a33-f023-40dc-961d-67b458f1b00f"  },
    { id: 4, shipment_number: 300003, ship_via: "Freight", freight_carrier: "SAP Delivery", units_shipped: 0, units_to_ship: 10,guid: "9d6ee5a7-20dd-4ad1-a16d-6f94775d6864"  },
    { id: 5, shipment_number: 300004, ship_via: "Freight", freight_carrier: "Yellow Truck", units_to_ship: 20, units_shipped: 13, guid: "d6a14781-92a7-44e3-becc-661a419963e9"  },
    { id: 6, shipment_number: 300005, ship_via: "FedEX", freight_carrier: "", units_to_ship: 6, units_shipped: 5, guid: "fd87b425-08a8-4f8f-8223-7ef462576df7"  },
    { id: 2, shipment_number: 300006, ship_via: "Freight", freight_carrier: "Ryan's Hot Shot", units_to_ship: 3, units_shipped: 0, guid: "9856993f-f966-43e2-aca7-2eaedfc565f7" },
  ]);

  
  const [colDefs, setColDefs] = useState<ColDef<ShipmentHeaderListDto>[]>([
    { field: "shipment_number", headerName: "Shipment #" },
    { field: "ship_via", headerName: "Ship Via" },
    { field: "freight_carrier", headerName: "Carrier" },
    { field: "units_to_ship", headerName: "Ship/Shipped", cellRenderer: ShippingCountsRenderer },
    { field: "address", headerName: "Address", cellRenderer: AddressRenderer },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle"onClick={() => handleViewClick(props.value)}>View</Button>&nbsp;
              <Button type="button" colorPalette="orange" onClick={() => handleEditClick(props.value)}>Edit</Button>
            </div>
          );
      }
    }
  ]);

  const defaultColDef: ColDef = {
    flex: 1,
    filter: true,
    sortable: true,
    wrapText: true,
    autoHeight: true, 
  };



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


export default ShipmentsPage;