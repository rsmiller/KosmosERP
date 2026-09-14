"use client"

import { DocumentUploadListDto, DocumentUploadRevisionTagDto } from '@/models/document-models';
import { Grid, GridItem } from '@chakra-ui/react';
import Link from 'next/link';


export class DocumentUploadSearchResultParams
{
    entity: DocumentUploadListDto | undefined;
}

function DocumentUploadSearchResult({entity}: DocumentUploadSearchResultParams) {

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
                    <h4><strong>{ entity?.document_name }</strong></h4>

                    {entity && entity.revision_tags && entity?.revision_tags.map((c: DocumentUploadRevisionTagDto) => (
                        <h5><strong>{c.tag_name}</strong> {c.tag_value}</h5>
                    ))}
                    
                </GridItem>
                 <GridItem colSpan={1} className='search-result-link-block'>
                    <Link href={ `/erp/documents/${entity?.guid}`} className='search-result-link'>View</Link>
                 </GridItem>
            </Grid>
            
        </div>
    );
}

export default DocumentUploadSearchResult;