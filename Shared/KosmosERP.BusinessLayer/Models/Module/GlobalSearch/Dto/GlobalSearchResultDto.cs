
using KosmosERP.BusinessLayer.Models.Module.Contact.Dto;
using KosmosERP.BusinessLayer.Models.Module.Customer.Dto;
using KosmosERP.BusinessLayer.Models.Module.DocumentUpload.Dto;
using KosmosERP.BusinessLayer.Models.Module.Lead.Dto;
using KosmosERP.BusinessLayer.Models.Module.Opportunity.Dto;
using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.BusinessLayer.Models.Module.PurchaseOrder.Dto;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Dto;

namespace KosmosERP.BusinessLayer.Models.Module.GlobalSearch.Dto;

public class GlobalSearchResultDto
{
    public List<OrderHeaderListDto> sales_order { get; set; } = new List<OrderHeaderListDto>();
    public List<PurchaseOrderHeaderListDto> purchase_orders { get; set; } = new List<PurchaseOrderHeaderListDto>();
    public List<CustomerListDto> customers { get; set; } = new List<CustomerListDto>();
    public List<VendorListDto> vendors { get; set; } = new List<VendorListDto>();
    public List<ContactListDto> contacts { get; set; } = new List<ContactListDto>();
    public List<OpportunityListDto> opportunities { get; set; } = new List<OpportunityListDto>();
    public List<LeadListDto> leads { get; set; } = new List<LeadListDto>();
    public List<DocumentUploadListDto> documents { get; set; } = new List<DocumentUploadListDto>();
}