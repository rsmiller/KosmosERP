"use client"

import { VendorListDto } from '@/models/vendor-models';
import { Grid, GridItem } from '@chakra-ui/react';
import Link from 'next/link';


export class VendorSearchResultParams
{
    entity: VendorListDto | undefined;
}

function VendorSearchResult({entity}: VendorSearchResultParams) {

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
                    <h4><strong>{ entity?.vendor_number }</strong> - { entity?.vendor_name }</h4>
                    <h5><strong>Description:</strong> { entity?.vendor_description }</h5>
                </GridItem>
                 <GridItem colSpan={1} className='search-result-link-block'>
                    <Link href={ `/erp/vendors/view/${entity?.guid}`} className='search-result-link'>View</Link>
                 </GridItem>
            </Grid>
            
        </div>
    );
}

export default VendorSearchResult;