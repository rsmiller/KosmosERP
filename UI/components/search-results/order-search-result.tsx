"use client"

import { OrderHeaderListDto } from '@/models/sales-order-models';
import { Grid, GridItem } from '@chakra-ui/react';
import Link from 'next/link';


export class OrderSearchResultParams
{
    entity: OrderHeaderListDto | undefined;
}

function SalesOrderSearchResult({entity}: OrderSearchResultParams) {

    return (
        <div className="search-result-block">
            <Grid
                templateColumns="repeat(6, 2fr)"
                gap={6}
                display="grid"
                width="100%"
                p="auto"
                m="auto"
            >
                <GridItem colSpan={5}>
                    <h4><strong>{ entity?.order_number }</strong></h4>
                    <h5><strong>Customer:</strong> { entity?.customer_name }</h5>
                    <h5><strong>Amount:</strong> { entity?.price }</h5>
                </GridItem>
                 <GridItem colSpan={1} className='search-result-link-block'>
                    <Link href={ `/erp/salesorders/view/${entity?.guid}`} className='search-result-link'>View</Link>
                 </GridItem>
            </Grid>
            
        </div>
    );
}

export default SalesOrderSearchResult;