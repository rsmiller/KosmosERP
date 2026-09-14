"use client"

import { docsService } from '@/services/docs-service';
import '../../../styles/docs.css';

import { OrderHeaderDto, OrderLineDto } from "@/models/sales-order-models";
import { For, Grid, GridItem } from "@chakra-ui/react";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { format } from 'date-fns';
import { settingsService } from '@/services/settings-service';
import { SettingsDto } from '@/models/settings-models';


function DocSalesOrder() {
    const params = useParams();

    const [orderHeader, setOrderHeader] = useState<OrderHeaderDto | null>(null);
    const [orderLines, setOrderLines] = useState<OrderLineDto[]>([]);

    const [settings, setSetting] = useState<SettingsDto | null>(null);

    useEffect(() => {
        const salesOrderId = String(params.id);

        settingsService.getBaseSettings("").then((setting_response) => 
        {
            console.log(setting_response);
            if(setting_response && setting_response.data)
            {
                setSetting(setting_response.data);
            }

            docsService.getSalesOrderByGuid(salesOrderId, "").then( (response) => {
                console.log(response);

                if(response.success && response.data)
                {
                    setOrderHeader(response.data);

                    if(response.data.order_lines)
                    {
                        setOrderLines(response.data.order_lines);
                    }
                }
            });
        });

    }, [params.id]);

    const GetSalesQRCodeValue = () =>
    {
        return "salesorders/" + orderHeader?.order_number;
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
                        Form F3.1
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
                                    <h2 className='center-all'>Sales Order</h2>
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
                            <GridItem colSpan={3} className="details-row">
                                {orderHeader?.po_number}
                            </GridItem>
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Order Date: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {DateFormatter(orderHeader?.order_date)}
                            </GridItem>
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Order By: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {orderHeader?.created_by_name}
                            </GridItem>

                            <GridItem colSpan={1} className="details-row-no-border details-block-strong">
                                Required Date: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row-no-border">
                                {DateFormatter(orderHeader?.required_date)}
                            </GridItem>

                            <GridItem colSpan={1} className="details-row-no-border details-block-strong">
                                Ship Method: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row-no-border">
                                {orderHeader?.shipping_method_name}
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
                                {line.line_description}
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

export default DocSalesOrder;