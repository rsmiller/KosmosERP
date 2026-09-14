using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Database.Models;
using KosmosERP.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Dto;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Find;
using KosmosERP.Database.Views;
using KosmosERP.BusinessLayer.Interfaces;


namespace KosmosERP.BusinessLayer.Modules;

public interface IARInvoiceModule : IERPModule<ARInvoiceHeader, ARInvoiceHeaderDto, ARInvoiceHeaderListDto, ARInvoiceHeaderCreateCommand, ARInvoiceHeaderEditCommand, ARInvoiceHeaderDeleteCommand, ARInvoiceHeaderFindCommand>, IBaseERPModule
{
    Task<Response<ARInvoiceLineDto>> GetLineDto(int object_id);
    Task<Response<ARInvoiceLineDto>> CreateLine(ARInvoiceLineCreateCommand commandModel);
    Task<Response<ARInvoiceLineDto>> EditLine(ARInvoiceLineEditCommand commandModel);
    Task<Response<ARInvoiceLineDto>> DeleteLine(ARInvoiceLineDeleteCommand commandModel);

    Task<ARInvoiceLineDto> MapToLineDto(ARInvoiceLine databaseModel);

    Task<Response<List<vw_OrdersReadyForInvoicing>>> GetOrdersReadyForInvoicing(string? customer_guid);
    Task<Response<List<vw_PartialInvoices>>> GetPartialInvoices();
}

public class ARInvoiceModule : BaseERPModule, IARInvoiceModule
{
	public override Guid ModuleIdentifier => Guid.Parse("fab75a7f-af8c-4416-9e40-9c774aba1811");
	public override string ModuleName => "AR Invoices";

	private IBaseERPContext _Context;
    private IMemoryCacheService<KeyValueStore> _KVMemoryService;
    private IMemoryCacheService<Customer> _CustomerMemoryService;
    private IMemoryCacheService<OrderHeader> _OrderMemoryService;
    private IPaymentProvider _PaymentProvider;
    private IFinancialTransactionModule? _FinancialTransactionModule;
    private IChartOfAccountModule? _ChartOfAccountModule;

    public ARInvoiceModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
    }

    public ARInvoiceModule(IBaseERPContext context,
                            IMemoryCacheService<KeyValueStore> kvMService,
                            IMemoryCacheService<Customer> customerMemService,
                            IMemoryCacheService<OrderHeader> orderHeaderMemService, 
                            IPaymentProviderFactory providerFactory,
                            IFinancialTransactionModule financialTransactionModule,
                            IChartOfAccountModule chartOfAccountModule,
                            ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Context = context;
        _KVMemoryService = kvMService;
        _CustomerMemoryService = customerMemService;
        _OrderMemoryService = orderHeaderMemService;
        _PaymentProvider = providerFactory.GetProvider();
        _FinancialTransactionModule = financialTransactionModule;
        _ChartOfAccountModule = chartOfAccountModule;
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "AR Invoice Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "AR Invoice Administrators",
            }, 1));

            _Context.SaveChanges();
        }

        var payment_terms_net_15 = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PaymentTerms && m.key == "payment_terms_net_15").SingleOrDefault();
        var payment_terms_net_30 = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PaymentTerms && m.key == "payment_terms_net_30").SingleOrDefault();
        var payment_terms_net_45 = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PaymentTerms && m.key == "payment_terms_net_45").SingleOrDefault();
        var payment_terms_net_60 = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PaymentTerms && m.key == "payment_terms_net_60").SingleOrDefault();

        var gl_account_cash = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_acccount_cash").SingleOrDefault();
        var gl_account_checking = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_acccount_checking").SingleOrDefault();
        var gl_account_payroll = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_payroll").SingleOrDefault();
        var gl_account_sales = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_sales").SingleOrDefault();
        var gl_account_general_expenses = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_general_expenses").SingleOrDefault();


        if (payment_terms_net_15 == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "payment_terms_net_15",
                value = "NET 15",
                module_id = KeyValueIds.PaymentTerms.ToString(),
                int_value = 1
            }, 1));

            _Context.SaveChanges();
        }

        if (payment_terms_net_15 == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "payment_terms_net_30",
                value = "NET 30",
                module_id = KeyValueIds.PaymentTerms.ToString(),
                int_value = 2
            }, 1));

            _Context.SaveChanges();
        }

        if (payment_terms_net_45 == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "payment_terms_net_45",
                value = "NET 45",
                module_id = KeyValueIds.PaymentTerms.ToString(),
                int_value = 3
            }, 1));

            _Context.SaveChanges();
        }

        if (payment_terms_net_60 == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "payment_terms_net_60",
                value = "NET 60",
                module_id = KeyValueIds.PaymentTerms.ToString(),
                int_value = 4
            }, 1));

            _Context.SaveChanges();
        }


        if (gl_account_cash == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_acccount_cash",
                value = "Cash - 1000001",
                module_id = KeyValueIds.GLAccounts.ToString(),
                int_value = 1000001
            }, 1));

            _Context.SaveChanges();
        }

        if (gl_account_checking == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_acccount_checking",
                value = "Checking - 1000002",
                module_id = KeyValueIds.GLAccounts.ToString(),
                int_value = 1000002
            }, 1));

            _Context.SaveChanges();
        }

        if (gl_account_payroll == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_account_payroll",
                value = "Payroll - 1005001",
                module_id = KeyValueIds.GLAccounts.ToString(),
                int_value = 1005001
            }, 1));

            _Context.SaveChanges();
        }

        if (gl_account_sales == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_account_sales",
                value = "Sales - 1003001",
                module_id = KeyValueIds.GLAccounts.ToString(),
                int_value = 1003001
            }, 1));

            _Context.SaveChanges();
        }

        if (gl_account_general_expenses == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_account_general_expenses",
                value = "General Expenses - 1007001",
                module_id = KeyValueIds.GLAccounts.ToString(),
                int_value = 1007001
            }, 1));

            _Context.SaveChanges();
        }

        // GL Accounts for AP/AR Posting Integration
        var gl_account_accounts_receivable = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_accounts_receivable").SingleOrDefault();
        var gl_account_accounts_payable = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_accounts_payable").SingleOrDefault();
        var gl_account_sales_revenue = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_sales_revenue").SingleOrDefault();
        var gl_account_purchases = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_purchases").SingleOrDefault();

        if (gl_account_accounts_receivable == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_account_accounts_receivable",
                value = "1100",
                module_id = KeyValueIds.GLAccounts.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (gl_account_accounts_payable == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_account_accounts_payable",
                value = "2010",
                module_id = KeyValueIds.GLAccounts.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (gl_account_sales_revenue == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_account_sales_revenue",
                value = "4010",
                module_id = KeyValueIds.GLAccounts.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        if (gl_account_purchases == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "gl_account_purchases",
                value = "5010",
                module_id = KeyValueIds.GLAccounts.ToString()
            }, 1));

            _Context.SaveChanges();
        }
    }

    public ARInvoiceHeader? Get(int object_id)
    {
        return _Context.ARInvoiceHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public ARInvoiceLine? GetLine(int object_id)
    {
        return _Context.ARInvoiceLines.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<ARInvoiceHeader?> GetAsync(int object_id)
    {
        return await _Context.ARInvoiceHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<ARInvoiceLine?> GetLineAsync(int object_id)
    {
        return await _Context.ARInvoiceLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<ARInvoiceHeaderDto>> GetDto(int object_id)
    {
        Response<ARInvoiceHeaderDto> response = new Response<ARInvoiceHeaderDto>();

        var result = await _Context.ARInvoiceHeaders.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("AP Invoice Header not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<ARInvoiceHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.ARInvoiceHeaders.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<ARInvoiceHeaderDto>("ARInvoiceHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<ARInvoiceHeaderDto>(dto);
    }

    public async Task<Response<ARInvoiceLineDto>> GetLineDto(int object_id)
    {
        Response<ARInvoiceLineDto> response = new Response<ARInvoiceLineDto>();

        var result = await _Context.ARInvoiceLines.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("AP Invoice Line not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToLineDto(result);
        return response;
    }

    public async Task<Response<ARInvoiceHeaderDto>> Create(ARInvoiceHeaderCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ARInvoiceHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ARInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var customer = await _Context.Customers.SingleOrDefaultAsync(m => m.id == commandModel.customer_id && !m.is_deleted);
        if(customer == null)
            return new Response<ARInvoiceHeaderDto>("Customer not found", ResultCode.DataValidationError);

        try
        {
            var item = await MapToDatabaseModel(commandModel);
            item.invoice_number = await this.ManuallyGenerateAnInvoiceNumber();

            await _Context.ARInvoiceHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();

            // Now do lines
            foreach (var ap_line in commandModel.ar_invoice_lines.Where(m => m.invoice_qty > 0))
            {
                var db_line = await MapToLineDatabaseModel(ap_line, item.id, commandModel.calling_user_id);

                await _Context.ARInvoiceLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();
            }


            try
            {
                if(String.IsNullOrEmpty(customer.payment_external_id))
                {
                    var customer_address = await _Context.CustomerAddresses.Where(m => m.customer_id == customer.id && m.address_type_id == CustomerAddressType.Physical && !m.is_deleted).FirstOrDefaultAsync();

                    if(customer_address != null)
                    {
                        var billing_address = await _Context.Addresses.Where(m => m.id == customer_address.address_id && !m.is_deleted).SingleAsync();

                        var customer_provider_response = await _PaymentProvider.CreateCustomer(customer, billing_address);

                        if (customer_provider_response.Success && customer_provider_response.Data != null)
                        {
                            customer.payment_external_id = customer_provider_response.Data.identifier;
                            
                            _Context.Customers.Update(customer);
                            await _Context.SaveChangesAsync();
                        }
                    }
                }

                var db_lines = await _Context.ARInvoiceLines
                    .Where(m => m.ar_invoice_header_id == item.id && !m.is_deleted)
                    .ToListAsync();

                var provider_response = await _PaymentProvider.CreateInvoice(item, db_lines, customer);

                if (provider_response.Success && provider_response.Data != null)
                {
                    item.payment_external_id = provider_response.Data.id;
                    _Context.ARInvoiceHeaders.Update(item);
                    await _Context.SaveChangesAsync();
                }
                    
            }
            catch(Exception e)
            {
                await LogError(90, this.GetType().Name, "Create - Create Invoice", e);
            }

            // Post to General Ledger
            await PostToGeneralLedger(item, commandModel.calling_user_id);

            var dto = await GetDto(item.id);

            return new Response<ARInvoiceHeaderDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<ARInvoiceHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ARInvoiceLineDto>> CreateLine(ARInvoiceLineCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ARInvoiceLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ARInvoiceLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        if (!commandModel.ar_invoice_header_id.HasValue)
            return new Response<ARInvoiceLineDto>("AR Invoice Header os a required field", ResultCode.DataValidationError);

        try
        {
            var item = await MapToLineDatabaseModel(commandModel, commandModel.ar_invoice_header_id.Value, commandModel.calling_user_id);

            await _Context.ARInvoiceLines.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = await GetLineDto(item.id);

            return new Response<ARInvoiceLineDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<ARInvoiceLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<ARInvoiceHeaderDto>> Edit(ARInvoiceHeaderEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ARInvoiceHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ARInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ARInvoiceHeaderDto>("AR Invoice Header not found", ResultCode.NotFound);


        if (existingEntity.invoice_date != commandModel.invoice_date && commandModel.invoice_date.HasValue)
            existingEntity.invoice_date = commandModel.invoice_date.Value;

        if (existingEntity.invoice_due_date != commandModel.invoice_due_date && commandModel.invoice_due_date.HasValue)
            existingEntity.invoice_due_date = commandModel.invoice_due_date.Value;

        if (commandModel.is_taxable.HasValue && existingEntity.is_taxable != commandModel.is_taxable)
            existingEntity.is_taxable = commandModel.is_taxable.Value;

        if (commandModel.tax_percentage.HasValue && existingEntity.tax_percentage != commandModel.tax_percentage)
            existingEntity.tax_percentage = commandModel.tax_percentage.Value;

        if (!String.IsNullOrEmpty(commandModel.payment_terms) && existingEntity.payment_terms != commandModel.payment_terms)
            existingEntity.payment_terms = commandModel.payment_terms;


        existingEntity = CommonDataHelper<ARInvoiceHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);

        //existingEntity.up


        _Context.ARInvoiceHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<ARInvoiceHeaderDto>(dto);
    }

    public async Task<Response<ARInvoiceLineDto>> EditLine(ARInvoiceLineEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<ARInvoiceLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ARInvoiceLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ARInvoiceLineDto>("AR Invoice Line not found", ResultCode.NotFound);


        if (existingEntity.line_number != commandModel.line_number && commandModel.line_number.HasValue)
            existingEntity.line_number = commandModel.line_number.Value;

        if (existingEntity.order_line_id != commandModel.order_line_id && commandModel.order_line_id.HasValue)
            existingEntity.order_line_id = commandModel.order_line_id.Value;

        if (existingEntity.ar_invoice_header_id != commandModel.ar_invoice_header_id && commandModel.ar_invoice_header_id.HasValue)
            existingEntity.ar_invoice_header_id = commandModel.ar_invoice_header_id.Value;

        if (existingEntity.product_id != commandModel.product_id && commandModel.product_id.HasValue)
            existingEntity.product_id = commandModel.product_id.Value;

        if (existingEntity.line_description != commandModel.line_description)
            existingEntity.line_description = commandModel.line_description;

        if (commandModel.invoice_qty.HasValue && existingEntity.invoice_qty != commandModel.invoice_qty)
            existingEntity.invoice_qty = commandModel.invoice_qty.Value;

        if (commandModel.is_taxable.HasValue && existingEntity.is_taxable != commandModel.is_taxable)
            existingEntity.is_taxable = commandModel.is_taxable.Value;


        existingEntity = CommonDataHelper<ARInvoiceLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


        _Context.ARInvoiceLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<ARInvoiceLineDto>(dto);
    }

    public async Task<Response<ARInvoiceHeaderDto>> Delete(ARInvoiceHeaderDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ARInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ARInvoiceHeaderDto>("AP Invoice Header not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<ARInvoiceHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.ARInvoiceHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var lines = await _Context.ARInvoiceLines.Where(m => m.ar_invoice_header_id == existingEntity.id).ToListAsync();
        foreach(var line in lines)
        {
            await this.DeleteLine(new ARInvoiceLineDeleteCommand()
            {
                calling_user_id = commandModel.calling_user_id,
                id = line.id
            });
        }

        var dto = await MapToDto(existingEntity);
        return new Response<ARInvoiceHeaderDto>(dto);
    }

    public async Task<Response<ARInvoiceLineDto>> DeleteLine(ARInvoiceLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<ARInvoiceLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<ARInvoiceLineDto>("AP Invoice Line not found", ResultCode.NotFound);

        // Soft Delete
        existingEntity = CommonDataHelper<ARInvoiceLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

        _Context.ARInvoiceLines.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToLineDto(existingEntity);
        return new Response<ARInvoiceLineDto>(dto);
    }

    public async Task<PagingResult<ARInvoiceHeaderListDto>> Find(PagingSortingParameters parameters, ARInvoiceHeaderFindCommand commandModel)
    {
        var response = new PagingResult<ARInvoiceHeaderListDto>();

        try
        {
            var query = _Context.ARInvoiceHeaders
                .Where(m => !m.is_deleted);


            if(commandModel.customer_id.HasValue)
            {
                query = query.Where(m => m.customer_id == commandModel.customer_id.Value);
            }

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                query = query.Where(m => m.invoice_number.ToString().Contains(commandModel.wildcard));
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<ARInvoiceHeaderListDto>();
            foreach (var item in pagedItems)
            {
                dtos.Add(await MapToListDto(item));
            }

            response.Data = dtos;
            response.TotalResultCount = totalCount;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(Find), ex);
            response.SetException(ex.Message, ResultCode.Error);
            response.TotalResultCount = 0;
        }

        return response;
    }

    public async Task<Response<List<ARInvoiceHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<ARInvoiceHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);

        throw new NotImplementedException();
    }

    public async Task<Response<List<vw_OrdersReadyForInvoicing>>> GetOrdersReadyForInvoicing(string? customer_guid)
    {
        Response<List<vw_OrdersReadyForInvoicing>> response = new Response<List<vw_OrdersReadyForInvoicing>>();

        try
        {
            if (!String.IsNullOrEmpty(customer_guid))
            {
                response.Data = await _Context.vw_OrdersReadyForInvoicing.Where(m => m.customer_guid == customer_guid).ToListAsync();
            }
            else
            {
                response.Data = await _Context.vw_OrdersReadyForInvoicing.ToListAsync();
            }
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetOrdersReadyForInvoicing), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }


        return response;
    }

    public async Task<Response<List<vw_PartialInvoices>>> GetPartialInvoices()
    {
        Response<List<vw_PartialInvoices>> response = new Response<List<vw_PartialInvoices>>();

        try
        {
            response.Data = await _Context.vw_PartialInvoices.ToListAsync();
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GetPartialInvoices), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }


        return response;
    }

    public async Task<ARInvoiceHeaderListDto> MapToListDto(ARInvoiceHeader databaseModel)
    {
        var dto = new ARInvoiceHeaderListDto()
        {
            
            customer_id = databaseModel.customer_id,
            invoice_number = databaseModel.invoice_number,
            invoice_date = databaseModel.invoice_date,
            invoice_due_date = databaseModel.invoice_due_date,
            invoice_total = databaseModel.invoice_total,
            is_paid = databaseModel.is_paid,
            paid_on = databaseModel.paid_on,
            guid = databaseModel.guid,
            payment_terms = databaseModel.payment_terms,
            order_header_id = databaseModel.order_header_id,
            tax_percentage = databaseModel.tax_percentage,
            is_taxable = databaseModel.is_taxable,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone
        };

        var payment_terms_val = await _KVMemoryService.GetKeyValue(databaseModel.payment_terms);
        var customer_val = await _CustomerMemoryService.GetDatabaseValue(databaseModel.customer_id);
        var order_val = await _OrderMemoryService.GetDatabaseValue(databaseModel.order_header_id);

        if (payment_terms_val != null)
            dto.payment_terms_name = payment_terms_val.value;

        if (customer_val != null)
            dto.customer_name = customer_val.customer_name;

        if (order_val != null)
            dto.order_number = order_val.order_number;

        return dto;
    }

    public async Task<ARInvoiceHeaderDto> MapToDto(ARInvoiceHeader databaseModel)
    {
        var dto = new ARInvoiceHeaderDto()
        {
            customer_id = databaseModel.customer_id,
            invoice_number = databaseModel.invoice_number,
            invoice_date = databaseModel.invoice_date,
            invoice_due_date = databaseModel.invoice_due_date,
            invoice_total = databaseModel.invoice_total,
            is_paid = databaseModel.is_paid,
            paid_on = databaseModel.paid_on,
            guid = databaseModel.guid,
            payment_terms = databaseModel.payment_terms,
            order_header_id = databaseModel.order_header_id,
            tax_percentage = databaseModel.tax_percentage,
            is_taxable = databaseModel.is_taxable,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var payment_terms_val = await _KVMemoryService.GetKeyValue(databaseModel.payment_terms);
        var customer_val = await _CustomerMemoryService.GetDatabaseValue(databaseModel.customer_id);
        var order_val = await _OrderMemoryService.GetDatabaseValue(databaseModel.order_header_id);

        if (payment_terms_val != null)
            dto.payment_terms_name = payment_terms_val.value;

        if (customer_val != null)
            dto.customer_name = customer_val.customer_name;

        if (order_val != null)
        {
            dto.order_number = order_val.order_number;
            dto.order_date = order_val.order_date;

            if (!String.IsNullOrEmpty(order_val.pay_method))
            {
                var pay_method = await _KVMemoryService.GetKeyValue(order_val.pay_method);
                if (pay_method != null)
                {
                    dto.pay_method = pay_method.value;
                }
                
            }
            
        }

        var lines = await _Context.ARInvoiceLines
            .Where(m => m.ar_invoice_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        foreach (var ar_line in lines)
            dto.ar_invoice_lines.Add(await MapToLineDto(ar_line));


        dto.tax_total = lines.Sum(m => m.line_tax);

        return dto;
    }

    public async Task<ARInvoiceLineDto> MapToLineDto(ARInvoiceLine databaseModel)
    {
        var dto = new ARInvoiceLineDto()
        {
            ar_invoice_header_id = databaseModel.ar_invoice_header_id,
            line_number = databaseModel.line_number,
            line_total = databaseModel.line_total,
            order_line_id = databaseModel.order_line_id,
            product_id = databaseModel.product_id,
            order_qty = databaseModel.order_qty,
            invoice_qty = databaseModel.invoice_qty,
            line_tax = databaseModel.line_tax,
            is_taxable = databaseModel.is_taxable,
            line_description = databaseModel.line_description,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone
        };

        var order_product = await (from ol in _Context.OrderLines
                                   join p in _Context.Products on ol.product_id equals p.id
                                   where ol.id == databaseModel.order_line_id
                                   select new { ol, p }).SingleOrDefaultAsync();

        if (order_product != null)
        {
            dto.identifier1 = order_product.p.identifier1;
            dto.unit_price = order_product.ol.unit_price;
            dto.units_ordered = order_product.ol.quantity;
        }

        return dto;
    }

    private async Task<List<PurchaseOrderReceiveLine>> GetPurchaseOrderReceiveData(int association_object_id)
    {
        List<PurchaseOrderReceiveLine> response = new List<PurchaseOrderReceiveLine>();

        var purchase_order_header = await _Context.PurchaseOrderHeaders
            .Where(m => m.id == association_object_id)
            .SingleOrDefaultAsync();

        if (purchase_order_header != null)
        {

        }


        return response;
    }

    public async Task<ARInvoiceLineListDto> MapToLineListDto(ARInvoiceLine databaseModel)
    {
        var dto = new ARInvoiceLineListDto()
        {
            ar_invoice_header_id = databaseModel.ar_invoice_header_id,
            line_number = databaseModel.line_number,
            line_total = databaseModel.line_total,
            order_line_id = databaseModel.order_line_id,
            product_id = databaseModel.product_id,
            order_qty = databaseModel.order_qty,
            invoice_qty = databaseModel.invoice_qty,
            line_tax = databaseModel.line_tax,
            is_taxable = databaseModel.is_taxable,
            line_description = databaseModel.line_description,
            guid = databaseModel.guid,
            id = databaseModel.id,
            created_on = databaseModel.created_on,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_by = databaseModel.created_by,
            updated_by = databaseModel.updated_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone
        };

        return dto;
    }

    public async Task<ARInvoiceHeader> MapToDatabaseModel(ARInvoiceHeaderCreateCommand createCommand)
    {
        decimal invoice_total = 0;
        foreach(var invoice_line in createCommand.ar_invoice_lines)
        {
            var product_price = await _Context.OrderLines.Where(m => m.id == invoice_line.order_line_id).Select(m => m.unit_price).SingleAsync();
            decimal line_total = (invoice_line.invoice_qty * product_price);

            if(invoice_line.is_taxable)
            {
                decimal tax = ((invoice_line.invoice_qty * product_price) * createCommand.tax_percentage);
                line_total = ((invoice_line.invoice_qty * product_price) + tax);
            }

            invoice_total = invoice_total + line_total;
        }

        return CommonDataHelper<ARInvoiceHeader>.FillCommonFields(new ARInvoiceHeader()
        {
            customer_id = createCommand.customer_id,
            order_header_id = createCommand.order_header_id,
            invoice_date = createCommand.invoice_date,
            invoice_due_date = createCommand.invoice_due_date,
            payment_terms = createCommand.payment_terms,
            is_taxable = createCommand.is_taxable,
            tax_percentage = createCommand.tax_percentage,
            invoice_total = invoice_total,
            guid = Guid.NewGuid().ToString(),
            created_on = DateTime.UtcNow,
            updated_on = DateTime.UtcNow,
            is_deleted = false
        }, createCommand.calling_user_id);
    }

    public async Task<ARInvoiceLine> MapToLineDatabaseModel(ARInvoiceLineCreateCommand createCommand, int ar_invoice_header_id, string calling_user_id)
    {
        var line = CommonDataHelper<ARInvoiceLine>.FillCommonFields(new ARInvoiceLine()
        {
            ar_invoice_header_id = ar_invoice_header_id,
            line_number = createCommand.line_number,
            order_line_id = createCommand.order_line_id,
            product_id = createCommand.product_id,
            line_description = createCommand.line_description,
            invoice_qty = createCommand.invoice_qty,
            is_taxable = createCommand.is_taxable,
            guid = Guid.NewGuid().ToString(),
            is_deleted = false,
        }, calling_user_id);


        var order_line = await _Context.OrderLines.Where(m => m.id == createCommand.order_line_id).FirstOrDefaultAsync();

        if (order_line == null)
            throw new Exception("Order line not found");

        var customer_tax = await (from c in _Context.Customers
                                  join o in _Context.OrderHeaders on c.id equals o.customer_id
                                  where o.id == order_line.order_header_id
                                  select new { c.is_taxable, c.tax_rate }).FirstOrDefaultAsync();

        if (customer_tax == null)
            throw new Exception("Customer tax information not found");


        // line_total
        line.line_total = order_line.unit_price * line.invoice_qty;

        // Order qty
        line.order_qty = order_line.quantity;

        // Tax
        if (line.is_taxable)
            line.line_tax = Math.Floor(((order_line.unit_price * customer_tax.tax_rate) * 100) * createCommand.invoice_qty) / 100;
        else
            line.line_tax = 0;

        return line;
    }

    private bool ARInvoiceLineExists(ARInvoiceLineCreateCommand createCommand)
    {
        return _Context.ARInvoiceLines.Any(m => m.line_number == createCommand.line_number);
    }

    public ARInvoiceHeader MapToDatabaseModel(ARInvoiceHeaderDto dtoModel)
    {
        throw new NotImplementedException();
    }

    private async Task<int> ManuallyGenerateAnInvoiceNumber()
    {
        var total_records = await _Context.ARInvoiceHeaders.CountAsync();
        int start = DatabaseStartNumbers.ARInvoices;

        return (total_records + start + 1);
    }

    private async Task PostToGeneralLedger(ARInvoiceHeader invoice, string callingUserId)
    {
        if (_FinancialTransactionModule == null || _ChartOfAccountModule == null)
        {
            await LogTrace("GL", 
                $"GL posting skipped for AR Invoice {invoice.id} - FinancialTransactionModule or ChartOfAccountModule not available");
            return;
        }

        try
        {
            // Get GL account numbers from KeyValueStore
            var accountsReceivableKV = await _Context.KeyValueStores
                .Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_accounts_receivable")
                .SingleOrDefaultAsync();
            
            var salesRevenueKV = await _Context.KeyValueStores
                .Where(m => m.module_id == KeyValueIds.GLAccounts && m.key == "gl_account_sales_revenue")
                .SingleOrDefaultAsync();

            if (accountsReceivableKV == null || salesRevenueKV == null)
            {
                await LogTrace("GL", 
                    $"GL posting skipped for AR Invoice {invoice.id} - GL account settings not found in KeyValueStore");
                return;
            }

            // Look up actual Chart of Account records by account number
            var accountsReceivable = await _ChartOfAccountModule.GetDtoByAccountNumber(accountsReceivableKV.value ?? "1100");
            var salesRevenue = await _ChartOfAccountModule.GetDtoByAccountNumber(salesRevenueKV.value ?? "4010");

            if (!accountsReceivable.Success || accountsReceivable.Data == null || 
                !salesRevenue.Success || salesRevenue.Data == null)
            {
                await LogTrace("GL", 
                    $"GL posting skipped for AR Invoice {invoice.id} - Chart of Account records not found");
                return;
            }

            var transactionDate = invoice.invoice_date.ToDateTime(TimeOnly.MinValue);
            var fiscalPeriod = transactionDate.ToString("yyyy-MM");
            var description = $"AR Invoice #{invoice.invoice_number} - Customer ID: {invoice.customer_id}";

            // Debit Accounts Receivable (increases asset)
            await _FinancialTransactionModule.RecordTransaction(
                accountsReceivable.Data.id,
                transactionDate,
                FinancialTransactionType.ARPost,
                "ARInvoice",
                invoice.id,
                invoice.guid,
                invoice.invoice_total,  // Debit amount
                0,                       // Credit amount
                description,
                fiscalPeriod,
                null,
                false,
                callingUserId
            );

            // Credit Sales Revenue (increases revenue)
            await _FinancialTransactionModule.RecordTransaction(
                salesRevenue.Data.id,
                transactionDate,
                FinancialTransactionType.ARPost,
                "ARInvoice",
                invoice.id,
                invoice.guid,
                0,                       // Debit amount
                invoice.invoice_total,   // Credit amount
                description,
                fiscalPeriod,
                null,
                false,
                callingUserId
            );

            // Update invoice posting status
            invoice.is_posted = true;
            invoice.posted_on = DateTime.UtcNow;
            invoice.posted_by = callingUserId;
            await _Context.SaveChangesAsync();

            await LogTrace("GL", 
                $"Successfully posted AR Invoice {invoice.id} to GL");
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(PostToGeneralLedger), ex);
            // Don't throw - invoice was created successfully, GL posting can be retried
        }
    }
}