"use client"

import { docsService } from '@/services/docs-service';
import '../../../styles/docs.css';

import { For, Grid, GridItem, QrCode } from "@chakra-ui/react";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { format } from 'date-fns';
import { PurchaseOrderHeaderDto, PurchaseOrderLineDto } from '@/models/purchase-order-models';
import { SettingsDto } from '@/models/settings-models';
import { settingsService } from '@/services/settings-service';


function DocPurchaseOrder() {
    const params = useParams();

    const [orderHeader, setOrderHeader] = useState<PurchaseOrderHeaderDto | null>(null);
    const [orderLines, setOrderLines] = useState<PurchaseOrderLineDto[]>([]);

    const [settings, setSetting] = useState<SettingsDto | null>(null);
    
    useEffect(() => {
        //console.log(params.id)
        const getPurchaseOrder = async() =>
        {
            const salesOrderId = String(params.id);

            await settingsService.getBaseSettings("").then( async (setting_response) => 
            {   
                //console.log(setting_response);
                if(setting_response && setting_response.data)
                {
                    setSetting(setting_response.data);
                }
                
                await docsService.getPurchaseOrderByGuid(salesOrderId, "").then( (response) => {
                    //console.log(response);

                    if(response.success && response.data)
                    {
                        setOrderHeader(response.data);

                        if(response.data.purchase_order_lines)
                        {
                            setOrderLines(response.data.purchase_order_lines);
                        }
                    }
                    
                });
            });
        };
        
        getPurchaseOrder();
    }, [params.id]);

    const GetSalesQRCodeValue = () =>
    {
        return "purchaseorders/" + orderHeader?.po_number;
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

    const RoundMoney = (numb: number | undefined) => {
        if( numb == undefined)
        {
            return 0;
        }
        //console.log(numb)
        return (Math.floor(numb * 100) / 100).toFixed(2);
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
                        Form F2.1
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
                                    <h2 className='center-all'>Purchase Order</h2>
                                    <h2 className='center-all'>{orderHeader?.po_number}</h2>
                                </Grid>
                            </GridItem>
                        </Grid>
                    </GridItem>
                    <GridItem colSpan={4} className='details-block'>
                        <Grid templateColumns="repeat(4, 2fr)" >
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Vendor: 
                            </GridItem>
                            <GridItem colSpan={3} className="details-row">
                                {orderHeader?.vendor_name}
                            </GridItem>
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Requested Date: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {DateFormatter(orderHeader?.created_on)}
                            </GridItem>
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Requested By: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {orderHeader?.po_by}
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
                        Qty
                    </GridItem>
                    <GridItem colSpan={1} className="line-header">
                        Unit Price
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
                                {line.product_name}
                            </GridItem>
                            <GridItem colSpan={1} className="line-row">
                                {line.quantity}
                            </GridItem>
                            <GridItem colSpan={1} className="line-row line-row-last-col">
                                ${line.unit_price}
                            </GridItem>
                        </Grid>
                    )}
                </For>

                <Grid templateColumns="repeat(8, 2fr)" className="total-block">
                    <GridItem colSpan={7} className="total-line-no-border">
                        Tax:
                    </GridItem>
                    <GridItem colSpan={1} className="total-line-price-no-border">
                        ${RoundMoney(orderHeader?.tax)}
                    </GridItem>
                    <GridItem colSpan={7} className="total-line-no-border">
                        Total:
                    </GridItem>
                    <GridItem colSpan={1} className="total-line-price-no-border">
                        ${RoundMoney(orderHeader?.price)}
                    </GridItem>
                </Grid>
        </Grid>
        
    );

}

export default DocPurchaseOrder;