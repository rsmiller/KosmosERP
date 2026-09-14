"use client"

import 'ag-grid-community/styles/ag-theme-quartz.css';
import "../../app/styles/page.component.css"

import { AllCommunityModule, ColDef, ModuleRegistry, CsvExportModule } from "ag-grid-community";
import { AgGridReact } from "ag-grid-react";
import { useEffect, useState } from "react";
import { Button, CloseButton, Dialog, Grid, GridItem, Portal } from '@chakra-ui/react'
import { useRouter } from 'next/navigation';
import { SubscriptionListDto, SubscriptionFindCommand, SubscriptionCreateCommand } from '@/models/subscription-models';
import { subscriptionService } from '@/services/subscription-service';
import SessionStorage from '@/components/session-storage';
import AgGridCustomPagination from '@/components/ag-grid/pagination-control';
import { MdEditDocument, MdOutlinePageview } from 'react-icons/md';
import SalesOrderSelectorComponent from '../sales-order-selector';
import { useKeycloak } from '@react-keycloak/web';

ModuleRegistry.registerModules([AllCommunityModule]);

export class SubscriptionListComponentParams
{
    customer_id: any;
    onChange: any;
}

function SubscriptionListComponent({customer_id, onChange}: SubscriptionListComponentParams) {
  const router = useRouter();
  const userId = SessionStorage.getUserId();
  const sessionId = SessionStorage.getSession();
  const { keycloak } = useKeycloak();
  
  const [rowData, setRowData] = useState<SubscriptionListDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(50);

  const [isDialogOpen, setDialogOpen] = useState<boolean>(false);
  const [saveable, canSave] = useState(false);

  const [loading, setLoading] = useState(true);

  const [newSubscription, setNewSubscription] = useState<any>();

  useEffect(() => {

    if(keycloak.authenticated == false) return;

    getTableData();
  }, [keycloak.authenticated]);

  const handleNewClick = () => {
    setDialogOpen(true);
  };

  const handleViewClick = (guid: any) => {
    router.push("/erp/subscriptions/view/" + guid);
  };

  const handleEditClick = (guid: any) => {
    router.push("/erp/subscriptions/edit/" + guid);
  };

  const getTableData = async () => {
    let pageStart = (page * pageSize) - pageSize;

    if(pageStart == 0) {
      pageStart = 1;    
    }

    setRowData([]);
    
    setLoading(true); // Show loading

    let command = new SubscriptionFindCommand();
    command.customer_id = customer_id;

    await subscriptionService.find(command, keycloak?.token || "", pageStart, pageSize).then((response) => {
      console.log(response);

      setLoading(false);

      if(response.success && response.data) {
        setRowData([]);
        
        for(let i=0; i<response.data?.length; i++) {
          let record = response.data[i];
          setRowData(prev => [...prev, { 
            id: record.id, 
            subscription_number: record.subscription_number, 
            customer_name: record.customer_name, 
            quantity: record.quantity,
            price: record.price,
            cycle_days: record.cycle_days,
            start_date: record.start_date,
            next_date: record.next_date,
            end_date: record.end_date,
            guid: record.guid 
          }]);
        }
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

  // Column Definitions: Defines & controls grid columns.
  const [colDefs, setColDefs] = useState<ColDef<SubscriptionListDto>[]>([
    { field: "subscription_number", headerName: "Subscription #" },
    { field: "customer_name", headerName: "Customer Name" },
    { field: "cycle_days", headerName: "Cycle Days" },
    { field: "price", headerName: "Price" },
    { field: "next_date", headerName: "Next Renew Date" },
    {
      field: "guid",
      headerName: "Actions",
      cellRenderer: (props: any) => {
          return ( 
            <div>
              <Button type="button" colorPalette="black" variant="subtle" onClick={() => handleViewClick(props.value)}><MdOutlinePageview /></Button>&nbsp;
              <Button type="button" colorPalette="green" onClick={() => handleEditClick(props.value)}><MdEditDocument /></Button>&nbsp;
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

  const receiveSubscriptionModel = (model: any) => {
    //console.log(model);
    setNewSubscription(model);

    canSave(true);
  }

  const saveSubscription = () => {
    if(newSubscription != undefined)
    {

      let command = new SubscriptionCreateCommand();
      command.customer_id = customer_id;
      command.quantity = 1;
      command.tax = 0;
      command.price = newSubscription.price;
      command.start_date = newSubscription.start_date;
      command.cycle_days  = newSubscription.terms_value;
      command.order_header_id = newSubscription.form_order_header_id;

      //console.log(command);

      subscriptionService.create(command, keycloak?.token || "").then( (response) => {
        //console.log(response);

        if(response.success && response.data != undefined)
        {
          let theRecord = response.data;
          setRowData(prev => [...prev, { 
            id: theRecord.id, 
            subscription_number: theRecord.subscription_number, 
            customer_name: theRecord.customer ? theRecord.customer.customer_name : '', 
            quantity: theRecord.quantity,
            price: theRecord.price,
            cycle_days: theRecord.cycle_days,
            start_date: theRecord.start_date,
            next_date: theRecord.next_date,
            end_date: theRecord.end_date,
            guid: theRecord.guid 
          }]);
        }
      });
    }
  }

  return (
    <div>
      <Grid
        templateColumns="repeat(5, 2fr)"
        gap={6}
        display="grid"
        width="100%"
        p="auto"
        m="auto"
      >
        <GridItem colSpan={6} hidden={customer_id == null || customer_id == ""}>
            <Button type="button" colorPalette="blue" onClick={() => handleNewClick()}>New Subscription</Button>
        </GridItem>
        <GridItem colSpan={6}>
          <div style={{ width: "100%", height: "500px" }}>
            <AgGridReact
              loading={loading}
              rowData={rowData}
              columnDefs={colDefs}
              defaultColDef={defaultColDef}
              onGridReady={getTableData}
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

      <Dialog.Root size="sm" open={isDialogOpen} onOpenChange={(details) => setDialogOpen(details.open)} role="alertdialog">
          <Portal>
              <Dialog.Backdrop />
              <Dialog.Positioner>
                  <Dialog.Content>
                      <Dialog.Header>
                          <Dialog.Title>New Subscription</Dialog.Title>
                      </Dialog.Header>
                      <Dialog.Body>
                          <SalesOrderSelectorComponent customer_id={customer_id} order_header_id="" onChange={(model: any) => { receiveSubscriptionModel(model) }}/>
                      </Dialog.Body>
                      <Dialog.Footer>
                          <Dialog.ActionTrigger asChild>
                              <Button disabled={!saveable} onClick={() => {saveSubscription()}}>Save</Button>
                          </Dialog.ActionTrigger>
                          <Dialog.CloseTrigger asChild>
                              <CloseButton size="sm" />
                          </Dialog.CloseTrigger>
                      </Dialog.Footer>
                  </Dialog.Content>
              </Dialog.Positioner>
          </Portal>
      </Dialog.Root>
    </div>
  );
}

export default SubscriptionListComponent;