"use client"

import '../../../styles/docs.css';

import { useEffect, useState } from 'react';
import { useParams } from "next/navigation";
import { For, Grid, GridItem } from '@chakra-ui/react';
import { format } from 'date-fns';

import { SettingsDto } from '@/models/settings-models';
import { docsService } from '@/services/docs-service';
import { OrderHeaderDto } from '@/models/sales-order-models';
import { BOMListDto } from '@/models/bom-models';

class RowDisplayDto
{
    id: number = 0;
    line_number: number = 0;
    product_name: string = "";
    identifier1: string = "";
    product_description: string = "";
    quanitity: number = 0;
    attributes: any[] = [];
    boms: BOMListDto[] = [];
}

function DocProductionOrder() {
    const params = useParams();

    const [settings, setSetting] = useState<SettingsDto | null>(null);
    const [orderHeader, setOrderHeader] = useState<OrderHeaderDto | null>(null);
    const [rowData, setRowData] = useState<RowDisplayDto[]>([]);
    
    useEffect(() => {
        const productionOrderId = String(params.id);

        docsService.getProductionOrderByGuid(productionOrderId, "").then( (response) => {
            //console.log(response);
            setRowData([]);

            if(response.success && response.data)
            {
                setOrderHeader(response.data?.order_header || null);

                for(const line of response.data?.production_order_lines || [])
                {
                    let rowdata = new RowDisplayDto();
                    rowdata.id = line.id || 0;
                    rowdata.line_number = line.order_line?.line_number || 0;
                    rowdata.product_name = line.order_line?.product_name || "";
                    rowdata.identifier1 = line.order_line?.identifier1 || "";
                    rowdata.product_description = line.order_line?.line_description || "";
                    rowdata.quanitity = line.quantity || 0;
                    rowdata.attributes = line.order_line?.attributes || [];
                    rowdata.boms = line.boms || [];

                    setRowData(prev => [...prev, rowdata]);
                }
            }
        });

    }, [params.id]);

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
                                <h2 className='center-all'>Production Order</h2>
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
                    </Grid>
                </GridItem>
            </Grid>

            <Grid templateColumns="repeat(9, 2fr)" className='lines-block'>
                <GridItem colSpan={1} className="line-header line-header-first">
                    Line #
                </GridItem>
                <GridItem colSpan={2} className="line-header">
                    Product Id
                </GridItem>
                <GridItem colSpan={2} className="line-header">
                    Product Name
                </GridItem>
                <GridItem colSpan={3} className="line-header">
                    Description
                </GridItem>
                <GridItem colSpan={1} className="line-header">
                    Qty
                </GridItem>
            </Grid>

            <For each={rowData}>
                {(line) => (
                    <Grid templateColumns="repeat(9, 2fr)" className='inner-lines-block break-after top-border' key={line.id}>
                        <GridItem colSpan={1} className="line-row line-row-first-col ">
                            {line.line_number}
                        </GridItem>
                        <GridItem colSpan={2} className="line-row">
                            {line.identifier1}
                        </GridItem>
                        <GridItem colSpan={2} className="line-row">
                            {line.product_name}
                        </GridItem>
                        <GridItem colSpan={3} className="line-row">
                            {line.product_description}
                        </GridItem>
                        <GridItem colSpan={1} className="line-row line-row-last-col">
                            {line.quanitity}
                        </GridItem>

                        <GridItem colSpan={9} className="line-row line-row-first-col line-row-last-col">
                            <Grid templateColumns="repeat(9, 2fr)" className='line-block' key={line.id + "-bom-header"}>
                                <GridItem colSpan={1} className="line-header line-header-first backedout">
                                    Piece Qty
                                </GridItem>
                                <GridItem colSpan={2} className="line-header backedout">
                                    Product
                                </GridItem>
                                <GridItem colSpan={6} className="line-header backedout">
                                    Instructions
                                </GridItem>
                            </Grid>
                            <For each={line.boms}>
                                {(bom) => (
                                    <Grid templateColumns="repeat(9, 2fr)" className='inner-lines-block' key={bom.guid}>
                                        <GridItem colSpan={1}>
                                            &nbsp;{line.quanitity} x {bom.quantity}
                                        </GridItem>
                                        <GridItem colSpan={2}>
                                            {bom.product_name}
                                        </GridItem>
                                        <GridItem colSpan={6}>
                                            {bom.instructions}
                                        </GridItem>
                                    </Grid>
                                )}
                            </For>
                            
                        </GridItem>
                    </Grid>
                )}
            </For>

        </Grid>
    );
}

export default DocProductionOrder;