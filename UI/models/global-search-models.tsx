import { DataCommand, PagingSortingParameters } from "./base-models";
import { ContactListDto } from "./contact-models";
import { CustomerListDto } from "./customer-models";
import { DocumentUploadListDto } from "./document-models";
import { LeadListDto } from "./lead-models";
import { OpportunityListDto } from "./opportunity-models";
import { PurchaseOrderHeaderListDto } from "./purchase-order-models";
import { OrderHeaderListDto } from "./sales-order-models";
import { VendorListDto } from "./vendor-models";


export class GlobalSearchFindCommand extends DataCommand 
{
    parameters?: PagingSortingParameters;
    wildcard?: string;
}

export class GlobalSearchResultDto 
{
    sales_order: OrderHeaderListDto[] = new Array<OrderHeaderListDto>();
    purchase_orders: PurchaseOrderHeaderListDto[] = new Array<PurchaseOrderHeaderListDto>();
    customers: CustomerListDto[] = new Array<CustomerListDto>();
    vendors: VendorListDto[] = new Array<VendorListDto>();
    contacts: ContactListDto[] = new Array<ContactListDto>();
    opportunities: OpportunityListDto[] = new Array<OpportunityListDto>();
    leads: LeadListDto[] = new Array<LeadListDto>();
    documents: DocumentUploadListDto[] = new Array<DocumentUploadListDto>();
}