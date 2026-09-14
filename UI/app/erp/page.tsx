
"use client"

import '../../app/styles/search-results.css';

import { GlobalSearchFindCommand, GlobalSearchResultDto } from "@/models/global-search-models";
import { globalSearchService } from "@/services/global-search-service";
import { PagingSortingParameters } from "@/models/base-models";
import { Grid, GridItem, Image, Input } from "@chakra-ui/react";
import { useForm } from 'react-hook-form'
import { useState } from "react";
import { CustomerListDto } from "@/models/customer-models";
import { ContactListDto } from "@/models/contact-models";
import { PurchaseOrderHeaderListDto } from '@/models/purchase-order-models';
import { LeadListDto } from '@/models/lead-models';
import { OrderHeaderListDto } from '@/models/sales-order-models';
import { VendorListDto } from '@/models/vendor-models';
import { DocumentUploadListDto } from '@/models/document-models';

import PurchaseOrderSearchResult from '@/components/search-results/purchase-order-search-result';
import CustomerSearchResult from "@/components/search-results/customer-search-result";
import ContactSearchResult from '@/components/search-results/contact-search-result';
import LeadSearchResult from '@/components/search-results/lead-search-result';
import SalesOrderSearchResult from '@/components/search-results/order-search-result';
import VendorSearchResult from '@/components/search-results/vendor-search-result';
import DocumentUploadSearchResult from '@/components/search-results/document-upload-search-result';
import { useKeycloak } from '@react-keycloak/web';


class SearchForm {
  wildcard?: string;
}

function ERPPage() {
  const { keycloak } = useKeycloak();
  const [loading, setLoading] = useState(false);
  const [searchData, setSearchData] = useState<GlobalSearchResultDto>();
  
  const {
    register,
    watch,
  } = useForm<SearchForm>();

  const keyDown = (key: any) => {
      if(key.key == "Enter")
      {
          performSearch();
      }
  }

  const performSearch = () => {
    let command = new GlobalSearchFindCommand();
    let parameters = new PagingSortingParameters();
    parameters.start = 1;
    parameters.resultCount = 25;
    parameters.sortOrder = "created_on-desc";
    command.parameters = parameters;
    command.wildcard = watch('wildcard');

    setLoading(true);
    setSearchData(undefined);

    globalSearchService.search(command, keycloak?.token || "").then((response) => {
        //console.log(response);

        if(response.success && response.data)
        {
          setSearchData(response.data);
        }
        
        setLoading(false);
    });
  }

  return (
    <div>
      <Grid
        templateColumns="repeat(1, 2fr)"
        gap={6}
        display="grid"
        width="25%"
        p="auto"
        m="auto"
      >
        <GridItem>
          <Image height="200px" width="200px" src="./kosmos_erp_med.png" p="auto" m="auto" />
        </GridItem>
        <GridItem>
            <Input placeholder="Search..." {...register('wildcard')} onKeyDown={(key: any) => keyDown(key)}/>
        </GridItem>
      </Grid>

      <Grid
          templateColumns="repeat(3, 2fr)"
          gap={6}
          display="grid"
          width="50%"
          p="auto"
          m="auto"
      >
        <GridItem colSpan={3} textAlign="center">
          {loading && <div className='search-loader'>Searching...</div>}
        </GridItem>
        {searchData && searchData.contacts && searchData.contacts.length > 0 && (
          <GridItem colSpan={3}>
            <hr className='search-result-category-splitter'/>
            <div>
              <h1>Contacts</h1>
              {searchData.contacts.map((c: ContactListDto) => (
                <ContactSearchResult entity={c} />
              ))}
            </div>
          </GridItem>
        )}

        {searchData && searchData.customers && searchData.customers.length > 0 && (
          <GridItem colSpan={3}>
            <hr className='search-result-category-splitter'/>
            <div>
              <h1>Customers</h1>
              {searchData.customers.map((c: CustomerListDto) => (
                <CustomerSearchResult entity={c} />
              ))}
            </div>
          </GridItem>
        )}

        {searchData && searchData.purchase_orders && searchData.purchase_orders.length > 0 && (
          <GridItem colSpan={3}>
            <hr className='search-result-category-splitter'/>
            <div>
              <h1>Purchase Orders</h1>
              {searchData.purchase_orders.map((c: PurchaseOrderHeaderListDto) => (
                <PurchaseOrderSearchResult entity={c} />
              ))}
            </div>
          </GridItem>
        )}

        {searchData && searchData.sales_order && searchData.sales_order.length > 0 && (
          <GridItem colSpan={3}>
            <hr className='search-result-category-splitter'/>
            <div>
              <h1>Sales Orders</h1>
              {searchData.sales_order.map((c: OrderHeaderListDto) => (
                <SalesOrderSearchResult entity={c} />
              ))}
            </div>
          </GridItem>
        )}

        {searchData && searchData.leads && searchData.leads.length > 0 && (
          <GridItem colSpan={3}>
            <hr className='search-result-category-splitter'/>
            <div>
              <h1>Leads</h1>
              {searchData.leads.map((c: LeadListDto) => (
                <LeadSearchResult entity={c} />
              ))}
            </div>
          </GridItem>
        )}

        {searchData && searchData.vendors && searchData.vendors.length > 0 && (
          <GridItem colSpan={3}>
            <hr className='search-result-category-splitter'/>
            <div>
              <h1>Vendors</h1>
              {searchData.vendors.map((c: VendorListDto) => (
                <VendorSearchResult entity={c} />
              ))}
            </div>
          </GridItem>
        )}

        {searchData && searchData.documents && searchData.documents.length > 0 && (
          <GridItem colSpan={3}>
            <hr className='search-result-category-splitter'/>
            <div>
              <h1>Documents</h1>
              {searchData.documents.map((c: DocumentUploadListDto) => (
                <DocumentUploadSearchResult entity={c} />
              ))}
            </div>
          </GridItem>
        )}
      </Grid>
    </div>
  )
}

export default ERPPage;