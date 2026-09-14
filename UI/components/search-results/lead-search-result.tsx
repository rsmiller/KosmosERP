"use client"

import { LeadListDto } from '@/models/lead-models';
import { Grid, GridItem } from '@chakra-ui/react';
import Link from 'next/link';


export class LeadSearchResultParams
{
    entity: LeadListDto | undefined;
}

function LeadSearchResult({entity}: LeadSearchResultParams) {

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
                    <h4><strong>{ entity?.first_name } { entity?.last_name }</strong></h4>
                    <h5><strong>Company:</strong> { entity?.company_name }</h5>
                </GridItem>
                 <GridItem colSpan={1} className='search-result-link-block'>
                    <Link href={ `/erp/leads/view/${entity?.guid}`} className='search-result-link'>View</Link>
                 </GridItem>
            </Grid>
            
        </div>
    );
}

export default LeadSearchResult;