"use client"

import { docsService } from '@/services/docs-service';
import '../../../styles/docs.css';

import { For, Grid, GridItem, QrCode } from "@chakra-ui/react";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { format } from 'date-fns';
import { ARInvoiceHeaderDto, ARInvoiceLineDto } from '@/models/ar-models';
import { settingsService } from '@/services/settings-service';
import { SettingsDto } from '@/models/settings-models';


function DocAR() {
    const params = useParams();

    const [arHeader, setARHeader] = useState<ARInvoiceHeaderDto | null>(null);
    const [arLines, setARLines] = useState<ARInvoiceLineDto[]>([]);

    const [settings, setSetting] = useState<SettingsDto | null>(null);
    
    useEffect(() => {
        //console.log(params.id)
        
        const getSalesOrder = async() =>
        {
            const arOrderId = String(params.id);

            await settingsService.getBaseSettings("").then( async (setting_response) => 
            {
                //console.log(setting_response);
                if(setting_response && setting_response.data)
                {
                    setSetting(setting_response.data);
                }

                await docsService.getARInvoiceByGuid(arOrderId, "").then( (response) => {
                    //console.log(response);

                    if(response.success && response.data)
                    {
                        setARHeader(response.data);

                        if(response.data.ar_invoice_lines)
                        {
                            setARLines(response.data.ar_invoice_lines);
                        }
                    }
                    
                });
            });

            
            
        };
        
        getSalesOrder();
    }, [params.id]);

    const GetSalesQRCodeValue = () =>
    {
        return "ar/" + arHeader?.invoice_number;
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
                        Form F4.1
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
                                    <h2 className='center-all'>Invoice</h2>
                                    <h2 className='center-all'>{arHeader?.invoice_number}</h2>
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
                                {arHeader?.customer_name}
                            </GridItem>

                            
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Order #:
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {arHeader?.order_number}
                            </GridItem>
                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Order Date: 
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {DateFormatter(arHeader?.order_date)}
                            </GridItem>

                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Invoice Date:
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {DateFormatter(arHeader?.invoice_date)}
                            </GridItem>

                            <GridItem colSpan={1} className="details-row details-block-strong">
                                Due Date:
                            </GridItem>
                            <GridItem colSpan={1} className="details-row">
                                {DateFormatter(arHeader?.invoice_due_date)}
                            </GridItem>

                            <GridItem colSpan={1} className="details-row-no-border details-block-strong">
                                Pay Method:
                            </GridItem>
                            <GridItem colSpan={1} className="details-row-no-border">
                                {arHeader?.pay_method}
                            </GridItem>
                        </Grid>
                    </GridItem>
                </Grid>

                <Grid templateColumns="repeat(9, 2fr)" className='lines-block'>
                    <GridItem colSpan={1} className="line-header line-header-first">
                        Line #
                    </GridItem>
                    <GridItem colSpan={2} className="line-header">
                        Product
                    </GridItem>
                    <GridItem colSpan={3} className="line-header">
                        Descriptionpay_method
                    </GridItem>
                    <GridItem colSpan={1} className="line-header">
                        Qty Inv
                    </GridItem>
                    <GridItem colSpan={1} className="line-header">
                        Unit Price
                    </GridItem>
                    <GridItem colSpan={1} className="line-header">
                        Line Total
                    </GridItem>
                </Grid>


                <For each={arLines}>
                    {(line) => (
                        <Grid templateColumns="repeat(9, 2fr)" className='inner-lines-block' key={line.id}>
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
                                {line.invoice_qty}
                            </GridItem>
                            <GridItem colSpan={1} className="line-row">
                                ${line.unit_price}
                            </GridItem>
                            <GridItem colSpan={1} className="line-row line-row-last-col">
                                ${line.line_total}
                            </GridItem>
                        </Grid>
                    )}
                </For>

                <Grid templateColumns="repeat(8, 2fr)" className="total-block">
                    <GridItem colSpan={7} className="total-line-no-border">
                        Tax:
                    </GridItem>
                    <GridItem colSpan={1} className="total-line-price-no-border">
                        ${RoundMoney(arHeader?.tax_total)}
                    </GridItem>
                    <GridItem colSpan={7} className="total-line-no-border">
                        Total:
                    </GridItem>
                    <GridItem colSpan={1} className="total-line-price-no-border">
                        ${RoundMoney(arHeader?.invoice_total)}
                    </GridItem>
                </Grid>
        </Grid>
        
    );

}

export default DocAR;