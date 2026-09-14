using KosmosERP.BusinessLayer;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.GlobalSearch.Dto;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;

public interface IGlobalSearchModule
{
    Task<Response<GlobalSearchResultDto>> GlobalSearch(GlobalSearchFindCommand commandModel);
}

public class GlobalSearchModule : IGlobalSearchModule
{
    private readonly IBaseERPContext _Context;
    private readonly ILogProvider _LogProvider;
    private readonly  IOrderModule _OrderModule;
    private readonly  ICustomerModule _CustomerModule;
    private readonly  IPurchaseOrderModule _PurchaseOrderModule;
    private readonly  IContactModule _ContactModule;
    private readonly  IOpportunityModule _OpportunityModule;
    private readonly  IVendorModule _VendorModule;
    private readonly  ILeadModule _LeadModule;
    private readonly  IDocumentUploadModule _DocumentUploadModule;


    public GlobalSearchModule(IBaseERPContext context,
                                ILogProviderFactory logProviderFactory,
                                IOrderModule orderModule,
                                ICustomerModule customerModule,
                                IPurchaseOrderModule purchaseOrderModule,
                                IContactModule contactModule,
                                IOpportunityModule opportunityModule,
                                IVendorModule vendorModule,
                                ILeadModule leadModule,
                                IDocumentUploadModule documentUploadModule)
    {
        _Context = context;
        _OrderModule = orderModule;
        _CustomerModule = customerModule;
        _PurchaseOrderModule = purchaseOrderModule;
        _ContactModule = contactModule;
        _OpportunityModule = opportunityModule;
        _VendorModule = vendorModule;
        _LeadModule = leadModule;
        _DocumentUploadModule = documentUploadModule;

        _LogProvider = logProviderFactory.GetProvider();
    }

    public async Task<Response<GlobalSearchResultDto>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<GlobalSearchResultDto>(validationResult.Exception, ResultCode.DataValidationError);


        var result = new Response<GlobalSearchResultDto>();
        result.Data = new GlobalSearchResultDto();
        
        /////////////////////////////////////////////////////////////////////////////////////
        try
        {
            var orders_result = await _OrderModule.GlobalSearch(commandModel);
            if(orders_result.Success)
                result.Data.sales_order = orders_result.Data;
        }
        catch(Exception ex)
        {
            await _LogProvider.LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
        }
        
        /////////////////////////////////////////////////////////////////////////////////////
        try
        {
            var purchase_orders_result = await _PurchaseOrderModule.GlobalSearch(commandModel);
            if(purchase_orders_result.Success)
                result.Data.purchase_orders = purchase_orders_result.Data;
        }
        catch(Exception ex)
        {
            await _LogProvider.LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        try
        {
            var customers_result = await _CustomerModule.GlobalSearch(commandModel);
            if(customers_result.Success)
                result.Data.customers = customers_result.Data;
        }
        catch(Exception ex)
        {
            await _LogProvider.LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        try
        {
            var vendor_result = await _VendorModule.GlobalSearch(commandModel);
            if(vendor_result.Success)
                result.Data.vendors = vendor_result.Data;
        }
        catch(Exception ex)
        {
            await _LogProvider.LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        try
        {
            var contact_result = await _ContactModule.GlobalSearch(commandModel);
            if(contact_result.Success)
                result.Data.contacts = contact_result.Data;
        }
        catch(Exception ex)
        {
            await _LogProvider.LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        try
        {
            var opportunity_result = await _OpportunityModule.GlobalSearch(commandModel);
            if(opportunity_result.Success)
                result.Data.opportunities = opportunity_result.Data;
        }
        catch(Exception ex)
        {
            await _LogProvider.LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        try
        {
            var leads_result = await _LeadModule.GlobalSearch(commandModel);
            if(leads_result.Success)
                result.Data.leads = leads_result.Data;
        }
        catch(Exception ex)
        {
            await _LogProvider.LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
        }

        /////////////////////////////////////////////////////////////////////////////////////
        try
        {
            var documents_result = await _DocumentUploadModule.GlobalSearch(commandModel);
            if(documents_result.Success)
                result.Data.documents = documents_result.Data;
        }
        catch(Exception ex)
        {
            await _LogProvider.LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
        }

        return result;
    }
}