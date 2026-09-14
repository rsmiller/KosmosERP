"use client"

import { CustomerListDto } from '@/models/customer-models';
import { Grid, GridItem } from '@chakra-ui/react';
import Link from 'next/link';


export class CustomerSearchResultParams
{
    entity: CustomerListDto | undefined;
}

function CustomerSearchResult({entity}: CustomerSearchResultParams) {

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
                    <h4><strong>{ entity?.customer_number }</strong> - { entity?.customer_name }</h4>
                    <h5><strong>Terms:</strong> { entity?.payment_terms_name }</h5>
                </GridItem>
                 <GridItem colSpan={1} className='search-result-link-block'>
                    <Link href={ `/erp/customers/view/${entity?.guid}`} className='search-result-link'>View</Link>
                 </GridItem>
            </Grid>
            
        </div>
    );
}

export default CustomerSearchResult;