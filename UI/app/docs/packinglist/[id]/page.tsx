"use client"

import { docsService } from '@/services/docs-service';
import '../../../styles/docs.css';


import { For, Grid, GridItem, QrCode } from "@chakra-ui/react";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { format } from 'date-fns';
import { ShipmentHeaderDto, ShipmentLineDto } from '@/models/shipments-models';
import { AddressDto } from '@/models/address-models';
import { SettingsDto } from '@/models/settings-models';
import { settingsService } from '@/services/settings-service';


function DocPackingList() {
    const params = useParams();

    const [orderHeader, setOrderHeader] = useState<ShipmentHeaderDto | null>(null);
    const [orderLines, setOrderLines] = useState<ShipmentLineDto[]>([]);

    const [settings, setSetting] = useState<SettingsDto | null>(null);
    
    useEffect(() => {
        //console.log(params.id)
        const getSalesOrder = async() =>
        {
            const salesOrderId = String(params.id);

            await settingsService.getBaseSettings("").then( async (setting_response) => 
            {
                //console.log(setting_response);
                if(setting_response && setting_response.data)
                {
                    setSetting(setting_response.data);
                }
                
                await docsService.getShipmentByGuid(salesOrderId, "").then( (response) => {
                    console.log(response);

                    if(response.success && response.data)
                    {
                        setOrderHeader(response.data);

                        if(response.data.shipment_lines)
                        {
                            setOrderLines(response.data.shipment_lines);
                        }
                    }
                    
                });
            });
            
        };
        
        getSalesOrder();
    }, [params.id]);

    const GetSalesQRCodeValue = () =>
    {
        return "shipments/" + orderHeader?.shipment_number;
    }


    const DateFormatter = (date: any) =>
    {
        if(date)
        {
            return format(date, 'MM-dd-yyyy');
        }
        else
        {
            return "";
        }
    }

    const BuildAddress = (address: AddressDto | undefined) =>
    {
        return address?.street_address1 + "<br/>" +  + " " + address?.country;
    }

    return (
        <Grid
            templateColumns="repeat(1, 2fr)"
            gap={0}
            display="grid"
            width="8.5in"
            p="auto"
            m="auto"
            >
                <Grid templateColumns="repeat(3, 2fr)" className='doc-control-header'>
                    <GridItem className="doc-control-block">
                        Form F1.1
                    </GridItem>
                    <GridItem className="doc-control-block">
                        Rev# 1
                    </GridItem>
                    <GridItem className="doc-control-block-last">
                        08-10-2025
                    </GridItem>
                </Grid>

                <Grid templateColumns="repeat(6, 2fr)" className='header-block'>
                    <GridItem colSpan={2} style={{padding: "5px"}}>
                    <Grid templateColumns="repeat(3, 2fr)">
                            <GridItem colSpan={3}>
                                <h3 style={{textAlign: "center"}}>{settings?.company_name}</h3>
                            </GridItem>
                            <GridItem colSpan={3} style={{paddingTop: "15px"}}>
                                <Grid templateColumns="repeat(1, 2fr)">
                                    <h2 className='center-all'>Packing List</h2>
                                    <h2 className='center-all'>{orderHeader?.order_number}</h2>
                                </Grid>
                            </GridItem>
                        </Grid>
                    </GridItem>
                    <GridItem colSpan={4} className='details-block'>
                        <Grid templateColumns="repeat(4, 2fr)" >
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Customer: 
                            </GridItem>
                            <GridItem colSpan={3} className="details-row">
                                {orderHeader?.customer_name}
                            </GridItem>
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Customer PO: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {orderHeader?.po_number}
                            </GridItem>

                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Order #: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {orderHeader?.order_number}
                            </GridItem>

                            <GridItem colSpan={1} className="details-row-no-border details-block-strong">
                                Ship To: 
                            </GridItem>
                            <GridItem colSpan={3} className="details-row-no-border" hidden={orderHeader?.address?.street_address2 == undefined}>
                                {orderHeader?.address?.street_address1}<br/>
                                {orderHeader?.address?.city}, {orderHeader?.address?.state}<br/>
                                {orderHeader?.address?.country}
                            </GridItem>
                            <GridItem colSpan={3} className="details-row-no-border" hidden={orderHeader?.address?.street_address2 != undefined}>
                                {orderHeader?.address?.street_address1}<br/>
                                {orderHeader?.address?.street_address2}<br/>
                                {orderHeader?.address?.city}, {orderHeader?.address?.state}<br/>
                                {orderHeader?.address?.country}
                            </GridItem>
                        </Grid>
                    </GridItem>
                </Grid>

                <Grid templateColumns="repeat(8, 2fr)" className='lines-block'>
                    <GridItem colSpan={1} className="line-header line-header-first">
                        Line #
                    </GridItem>
                    <GridItem colSpan={2} className="line-header">
                        Product
                    </GridItem>
                    <GridItem colSpan={3} className="line-header">
                        Description
                    </GridItem>
                    <GridItem colSpan={1} className="line-header">
                        Ordered
                    </GridItem>
                    <GridItem colSpan={1} className="line-header">
                        Shipped
                    </GridItem>
                </Grid>


                <For each={orderLines}>
                    {(line) => (
                        <Grid templateColumns="repeat(8, 2fr)" className='inner-lines-block' key={line.id}>
                            <GridItem colSpan={1} className="line-row line-row-first-col">
                                {line.line_number}
                            </GridItem>
                            <GridItem colSpan={2} className="line-row">
                                {line.identifier1}
                            </GridItem>
                            <GridItem colSpan={3} className="line-row">
                                {line.line_description}
                            </GridItem>
                            <GridItem colSpan={1} className="line-row">
                                {line.units_ordered}
                            </GridItem>
                            <GridItem colSpan={1} className="line-row line-row-last-col">
                                {line.units_shipped}
                            </GridItem>
                        </Grid>
                    )}
                </For>
        </Grid>
        
    );

}

export default DocPackingList;