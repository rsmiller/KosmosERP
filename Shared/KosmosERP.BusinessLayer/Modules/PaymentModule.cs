using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Payment.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Payment.Dto;
using Microsoft.AspNetCore.Hosting.Server;
using KosmosERP.BusinessLayer.Models.Module.Payment;
using KosmosERP.BusinessLayer.PaymentProviders.Models;


namespace KosmosERP.BusinessLayer.Modules;

public interface IPaymentModule : IERPModule<Payment, PaymentDto, PaymentListDto, PaymentCreateCommand, PaymentEditCommand, PaymentDeleteCommand, PaymentFindCommand>, IBaseERPModule
{
    Task<Response<PaymentProviderTransactionCreateDto>> GetStripePaymentIntentFromARInvoce(CreateStripePaymentIntentCommand commandModel);
    Task<Response<PaymentProviderTransactionGetDto>> GetStripePaymentIntentStatus(StripePaymentIntentStatusCommand commandModel);
    Task<Response<SavedPaymentMethodsDto>> GetSavedPaymentMethods(GetSavedPaymentMethodsCommand commandModel);
    Task<Response<PaymentProviderTransactionGetDto>> PayStripePaymentIntent(StripePaymentIntentStatusCommand commandModel);
    Task<Response<PaymentCardGetDto>> CreateNewCardIntent(CreateStripeNewCardIntentCommand commandModel);
    Task<Response<PaymentDto>> CreateInternal(PaymentCreateCommand commandModel);
}

public class PaymentModule : BaseERPModule, IPaymentModule
{
    public override Guid ModuleIdentifier => Guid.Parse("3eeb1eb4-2f3c-4cee-b47e-2bd15ed7477f");
    public override string ModuleName => "Payments";

    private IBaseERPContext _Context;
    private IPaymentProvider _PaymentProvider;
    private IServer _Server;

    public PaymentModule(IServer server, IBaseERPContext context, 
                            IPaymentProviderFactory providerFactory, 
                            ILogProviderFactory logProviderFactory) : base(logProviderFactory)
    {
        _Server = server;
        _Context = context;
        _PaymentProvider = providerFactory.GetProvider();
    }

    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Payment Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Payment Administrators",
            }, 1));

            _Context.SaveChanges();
        }


        var existing_permissions = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString());
        if (!existing_permissions)
        {
            _Context.ModulePermissions.AddRange(new[]
            {
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Read Payments",
                    internal_permission_name = "read_payment",
                    read = true,
                    write = false,
                    edit = false,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Create Payments",
                    internal_permission_name = "create_payment",
                    read = false,
                    write = true,
                    edit = false,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Edit Payments",
                    internal_permission_name = "edit_payment",
                    read = false,
                    write = false,
                    edit = true,
                    delete = false,
                    is_active = true
                }, 1),
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Delete Payments",
                    internal_permission_name = "delete_payment",
                    read = false,
                    write = false,
                    edit = false,
                    delete = true,
                    is_active = true
                }, 1)
            });

            _Context.SaveChanges();
        }
    }

    public async Task<Response<PaymentCardGetDto>> CreateNewCardIntent(CreateStripeNewCardIntentCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PaymentCardGetDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PaymentCardGetDto>(validationResult.Exception, ResultCode.DataValidationError);
        
        var ar_header = await _Context.ARInvoiceHeaders.SingleOrDefaultAsync(m => m.guid == commandModel.ar_header_guid);
        if (ar_header == null)
            return new Response<PaymentCardGetDto>("AR Invoice Header not found", ResultCode.NotFound);

            
        var customer = await _Context.Customers.SingleOrDefaultAsync(m => m.id == ar_header.customer_id);
        if(customer == null)
            return new Response<PaymentCardGetDto>("Customer not found", ResultCode.NotFound);  
        

        Response<PaymentCardGetDto> response = new Response<PaymentCardGetDto>();

        try
        {
            var provider_response = await _PaymentProvider.CreateCard(customer);

            if (!provider_response.Success)
            {
                response.SetException(provider_response.Exception, ResultCode.Error);
                return response;
            }

            return new Response<PaymentCardGetDto>(provider_response.Data);
        }
        catch (Exception ex)
        {
            await this.LogError(99, this.GetType().Name, nameof(CreateNewCardIntent), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<Response<SavedPaymentMethodsDto>> GetSavedPaymentMethods(GetSavedPaymentMethodsCommand commandModel)
    {
        if (commandModel == null)
            return new Response<SavedPaymentMethodsDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<SavedPaymentMethodsDto>(validationResult.Exception, ResultCode.DataValidationError);
        

        Response<SavedPaymentMethodsDto> response = new Response<SavedPaymentMethodsDto>();

        try
        {
            string external_customer_payment_id = "";

            if(commandModel.customer_id.HasValue)
            {
                var customer = await _Context.Customers.SingleOrDefaultAsync(m => m.id == commandModel.customer_id);
                if (customer == null || string.IsNullOrEmpty(customer.payment_external_id))
                {
                    response.SetException("Customer not found or does not have a payment external ID", ResultCode.NotFound);
                    return response;
                }

                external_customer_payment_id = customer.payment_external_id;
            }
            else if(!String.IsNullOrEmpty(commandModel.ar_invoice_header_guid))
            {
                var ar_header = await _Context.ARInvoiceHeaders.SingleOrDefaultAsync(m => m.guid == commandModel.ar_invoice_header_guid);
                if (ar_header == null)
                {
                    response.SetException("AR Invoice Header not found", ResultCode.NotFound);
                    return response;
                }

                var customer = await _Context.Customers.SingleOrDefaultAsync(m => m.id == ar_header.customer_id);
                if (customer == null || string.IsNullOrEmpty(customer.payment_external_id))
                {
                    var customer_address = await _Context.CustomerAddresses.Where(m => m.customer_id == customer.id && m.address_type_id == CustomerAddressType.Physical && !m.is_deleted).FirstOrDefaultAsync();

                    if(customer_address != null)
                    {
                        var billing_address = await _Context.Addresses.Where(m => m.id == customer_address.address_id && !m.is_deleted).SingleAsync();

                        var customer_response = await _PaymentProvider.CreateCustomer(customer, billing_address);

                        if (!customer_response.Success)
                            await LogError(90, this.GetType().Name, "Create - Create Customer", new Exception(customer_response.Exception));

                        customer.payment_external_id = customer_response.Data?.identifier;

                        _Context.Customers.Update(customer);
                        await _Context.SaveChangesAsync();
                    }
                }
                
                external_customer_payment_id = customer.payment_external_id;
            }
            else
            {
                response.SetException("Either customer Id or ar invoice header id must be provided", ResultCode.DataValidationError);
                return response;
            }


            var provider_response = await _PaymentProvider.GetSavedPaymentMethods(external_customer_payment_id);

            if (!provider_response.Success)
            {
                response.SetException(provider_response.Exception, ResultCode.Error);
                return response;
            }

            response.Data = provider_response.Data;
        }
        catch (Exception ex)
        {
            await this.LogError(99, this.GetType().Name, nameof(GetSavedPaymentMethods), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
    }


    public async Task<Response<PaymentProviderTransactionGetDto>> PayStripePaymentIntent(StripePaymentIntentStatusCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PaymentProviderTransactionGetDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PaymentProviderTransactionGetDto>(validationResult.Exception, ResultCode.DataValidationError);

        try
        {
            var payment_transaction = await _PaymentProvider.GetTransaction(commandModel.payment_intent_id);

            if(payment_transaction.Success && payment_transaction.Data != null)
            {
                var service = new Stripe.PaymentIntentService();
                var confirm_response = await service.ConfirmAsync(commandModel.payment_intent_id);

                if(!confirm_response.Status.Equals("succeeded", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Payment Intent could not be confirmed");
                }

                return await this.GetStripePaymentIntentStatus(commandModel);
            }
            else
            {
                throw new Exception("Could not retrieve Payment Intent");
            }
            
        }
        catch(Exception ex)
        {
            await this.LogError(99, this.GetType().Name, nameof(PayStripePaymentIntent), ex);
            return new Response<PaymentProviderTransactionGetDto>(ex.Message, ResultCode.Error);
        }
    }


    public async Task<Response<PaymentProviderTransactionGetDto>> GetStripePaymentIntentStatus(StripePaymentIntentStatusCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PaymentProviderTransactionGetDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PaymentProviderTransactionGetDto>(validationResult.Exception, ResultCode.DataValidationError);

        try
        {
            var payment_transaction = await _PaymentProvider.GetTransaction(commandModel.payment_intent_id);

            if(payment_transaction.Success && payment_transaction.Data != null)
            {

                if(payment_transaction.Data.status == "succeeded")
                {
                    // Payment succeeded, update local records if necessary
                    var existingPayment = await _Context.Payments.SingleOrDefaultAsync(m => m.transaction_id == commandModel.payment_intent_id);
                    if(existingPayment != null && existingPayment.transaction_status != "succeeded")
                    {
                        existingPayment.transaction_status = "succeeded";
                        existingPayment.transaction_method = payment_transaction.Data.transaction_method;
                        existingPayment.transaction_date = DateTime.UtcNow;
                        existingPayment.confirmation_code = payment_transaction.Data.confirmation_code;
                        existingPayment.payment_receipt = payment_transaction.Data.payment_receipt;

                        _Context.Payments.Update(existingPayment);
                        await _Context.SaveChangesAsync();

                        var existingARHeader = await _Context.ARInvoiceHeaders.SingleOrDefaultAsync(m => m.id == existingPayment.ar_header_id);
                        if (existingARHeader != null)
                        {
                            existingARHeader.paid_on = DateOnly.FromDateTime(DateTime.UtcNow);
                            existingARHeader.is_paid = true;

                            if(!String.IsNullOrEmpty(existingARHeader.payment_external_id))
                            {
                                await _PaymentProvider.CompleteInvoice(existingARHeader.payment_external_id, existingPayment.transaction_id);
                            }

                            var ar_header = CommonDataHelper<ARInvoiceHeader>.FillUpdateFields(existingARHeader, commandModel.calling_user_id);

                            _Context.ARInvoiceHeaders.Update(ar_header);
                            await _Context.SaveChangesAsync();
                        }
                    }

                }

                return new Response<PaymentProviderTransactionGetDto>(payment_transaction.Data);
            }
            else
            {
                return new Response<PaymentProviderTransactionGetDto>("Could not retrieve Payment Intent", ResultCode.Error);
            }
            
        }
        catch(Exception ex)
        {
            await this.LogError(99, this.GetType().Name, nameof(GetStripePaymentIntentStatus), ex);
            return new Response<PaymentProviderTransactionGetDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PaymentProviderTransactionCreateDto>> GetStripePaymentIntentFromARInvoce(CreateStripePaymentIntentCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PaymentProviderTransactionCreateDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PaymentProviderTransactionCreateDto>(validationResult.Exception, ResultCode.DataValidationError);

        try
        {
            var existingARHeader = await _Context.ARInvoiceHeaders.SingleOrDefaultAsync(m => m.guid == commandModel.ar_header_guid);
            if (existingARHeader == null)
            {
                await this.LogError(99, this.GetType().Name, nameof(GetStripePaymentIntentFromARInvoce), new Exception("AR Header not found"));
                throw new Exception("AR Header not found");
            }
                
            if (existingARHeader.is_paid)
            {
                await this.LogError(90, this.GetType().Name, nameof(GetStripePaymentIntentFromARInvoce), new Exception("AR Invoice already paid"));
                throw new Exception("AR Invoice already paid");
            }

            var customer = await _Context.Customers.SingleOrDefaultAsync(m => m.id == existingARHeader.customer_id);
            if (customer == null)
            {
                await this.LogError(90, this.GetType().Name, nameof(GetStripePaymentIntentFromARInvoce), new Exception("Customer not found"));
                throw new Exception("Customer not found");
            }

            var ar_invoice_lines = await _Context.ARInvoiceLines.Include("order_line").Where(m => m.ar_invoice_header_id == existingARHeader.id && !m.is_deleted).ToListAsync();

            decimal total_amount = ar_invoice_lines.Sum(m => m.line_total);
            decimal rounded_price = Math.Round(total_amount, 2);
            

            var payment_response = await this.Create(new PaymentCreateCommand()
            {
                ar_invoice_header_id = existingARHeader.id,
                order_header_id = existingARHeader.order_header_id,
                calling_user_id = commandModel.calling_user_id,
                payment_amount = rounded_price,
                transaction_method = "stripe",
                transaction_status = "created",
                transaction_id = "",
                confirmation_code = "",
                payment_processor = "stripe",
                payment_receipt = "",
            });

            if (!payment_response.Success)
                throw new Exception("Failed to create payment in database: " + payment_response.Exception);

            var existingOrder = await _Context.OrderHeaders.SingleOrDefaultAsync(m => m.id == existingARHeader.order_header_id);
            var payment = await _Context.Payments.SingleOrDefaultAsync(m => m.id == payment_response.Data.id);
            
            Dictionary<string, string>? metadata = new Dictionary<string, string>();
            

            if(commandModel.payment_method_id != null && commandModel.payment_method_id != "" && commandModel.payment_method_id != "undefined")
                metadata.Add("payment_method_id", commandModel.payment_method_id);

            var provider_response = await _PaymentProvider.CreateTransaction(existingARHeader, payment, customer, existingOrder, metadata);

            if (!provider_response.Success)
                throw new Exception("Failed to create payment in Stripe: " + provider_response.Exception);

            
            payment.transaction_id = provider_response.Data.id;
            _Context.Payments.Update(payment);
            await _Context.SaveChangesAsync();

            return new Response<PaymentProviderTransactionCreateDto>(provider_response.Data);
        }
        catch(Exception ex)
        {
            await this.LogError(99, this.GetType().Name, nameof(GetStripePaymentIntentFromARInvoce), ex);
            return new Response<PaymentProviderTransactionCreateDto>(ex.Message, ResultCode.Error);
        }
    }

    public Payment? Get(int object_id)
    {
        return _Context.Payments.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<Payment?> GetAsync(int object_id)
    {
        return await _Context.Payments.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<Response<PaymentDto>> GetDto(int object_id)
    {
        Response<PaymentDto> response = new Response<PaymentDto>();

        var result = await _Context.Payments.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Payment not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<PaymentDto>> GetDtoByGuid(string guid)
    {
        Response<PaymentDto> response = new Response<PaymentDto>();

        var result = await _Context.Payments.SingleOrDefaultAsync(m => m.guid == guid);
        if (result == null)
        {
            response.SetException("Payment not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<PaymentDto>> CreateInternal(PaymentCreateCommand commandModel)
    {
        var existingOrder = await _Context.OrderHeaders.SingleOrDefaultAsync(m => m.id == commandModel.order_header_id);
        if (existingOrder == null)
            return new Response<PaymentDto>("Order not found", ResultCode.NotFound);

        var existingARHeader = await _Context.ARInvoiceHeaders.SingleOrDefaultAsync(m => m.id == commandModel.ar_invoice_header_id);
        if (existingARHeader == null)
            return new Response<PaymentDto>("Order not found", ResultCode.NotFound);

        try
        {
            // Does this order have a payment customer?
            var customer = await _Context.Customers.Where(m => m.id == existingOrder.customer_id).SingleOrDefaultAsync();

            if(customer != null && customer.external_id != null && customer.external_id != "")
            {
                try
                {
                    var customer_address = await _Context.CustomerAddresses.Where(m => m.customer_id == customer.id && m.address_type_id == CustomerAddressType.Physical && !m.is_deleted).FirstOrDefaultAsync();

                    if(customer_address != null)
                    {
                        var billing_address = await _Context.Addresses.Where(m => m.id == customer_address.address_id && !m.is_deleted).SingleAsync();

                        var provider_response = await _PaymentProvider.CreateCustomer(customer, billing_address);

                        if (!provider_response.Success)
                            await LogError(90, this.GetType().Name, "Create - Create Customer", new Exception(provider_response.Exception));

                        customer.payment_external_id = provider_response.Data.identifier;

                        _Context.Customers.Update(customer);
                        await _Context.SaveChangesAsync();
                    }
                    else
                    {
                        await LogError(90, this.GetType().Name, "Create - Create Customer", new Exception($"Billing address not found for customer {customer.guid}"));
                    }
                }
                catch (Exception e)
                {
                    await LogError(90, this.GetType().Name, "Create - Create Customer", e);
                }
            }

            var item = this.MapToDatabaseModel(commandModel, commandModel.calling_user_id);
            
            item.payment_number = await this.ManuallyGenerateAnPaymentNumber();


            await _Context.Payments.AddAsync(item);
            await _Context.SaveChangesAsync();


            var dto = await GetDto(item.id);

            return new Response<PaymentDto>(dto.Data);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Create), ex);
            return new Response<PaymentDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<PaymentDto>> Create(PaymentCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PaymentDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PaymentDto>(validationResult.Exception, ResultCode.DataValidationError);

        return await CreateInternal(commandModel);
    }

    public async Task<Response<PaymentDto>> Edit(PaymentEditCommand commandModel)
    {
        if (commandModel == null)
            return new Response<PaymentDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PaymentDto>(validationResult.Exception, ResultCode.DataValidationError);

        return new Response<PaymentDto>("You can not update a payment once it has been created", ResultCode.NotFound);
    }

    public async Task<Response<PaymentDto>> Delete(PaymentDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<PaymentDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<PaymentDto>("Payment not found", ResultCode.NotFound);


        // Delete Record
        existingEntity = CommonDataHelper<Payment>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


        _Context.Payments.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToDto(existingEntity);
        return new Response<PaymentDto>(dto);
    }

    public async Task<PagingResult<PaymentListDto>> Find(PagingSortingParameters parameters, PaymentFindCommand commandModel)
    {
        var response = new PagingResult<PaymentListDto>();

        try
        {
            var filter = PredicateBuilder.True<Payment>();
            filter = filter.And(m => m.is_deleted == false);

            if (commandModel.order_header_id.HasValue)
            {
                filter = filter.And(m => m.order_header_id == commandModel.order_header_id);
            }

            if (!string.IsNullOrEmpty(commandModel.wildcard))
                {
                    var wild = commandModel.wildcard.ToLower();
                    filter = filter.Or(m => m.transaction_id.ToLower().Contains(wild));
                }


            var totalCount = await _Context.Payments.CountAsync(filter);
            var results = _Context.Payments.Where(filter);
            var pagedItems = await results.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<PaymentListDto>();
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

    public async Task<Response<List<PaymentListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<PaymentListDto>>(validationResult.Exception, ResultCode.DataValidationError);


        throw new NotImplementedException();
    }

    public async Task<PaymentListDto> MapToListDto(Payment databaseModel)
    {
        var dto = new PaymentListDto()
        {
            id = databaseModel.id,
            payment_number = databaseModel.payment_number,
            order_header_id = databaseModel.order_header_id,
            ar_header_id = databaseModel.ar_header_id,
            payment_amount = databaseModel.payment_amount,
            transaction_method = databaseModel.transaction_method,
            transaction_id = databaseModel.transaction_id,
            transaction_date = databaseModel.transaction_date,
            transaction_status = databaseModel.transaction_status,
            confirmation_code = databaseModel.confirmation_code,
            payment_processor = databaseModel.payment_processor,
            payment_receipt = databaseModel.payment_receipt,
            guid = databaseModel.guid,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
        };

        return dto;
    }

    public async Task<PaymentDto> MapToDto(Payment databaseModel)
    {
        var dto = new PaymentDto()
        {
            id = databaseModel.id,
            payment_number = databaseModel.payment_number,
            order_header_id = databaseModel.order_header_id,
            ar_header_id = databaseModel.ar_header_id,
            payment_amount = databaseModel.payment_amount,
            transaction_method = databaseModel.transaction_method,
            transaction_id = databaseModel.transaction_id,
            transaction_date = databaseModel.transaction_date,
            transaction_status = databaseModel.transaction_status,
            confirmation_code = databaseModel.confirmation_code,
            payment_processor = databaseModel.payment_processor,
            payment_receipt = databaseModel.payment_receipt,
            guid = databaseModel.guid,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
        };

        try
        {
            var provider_response = await _PaymentProvider.GetTransaction(databaseModel.transaction_id);

            if (!provider_response.Success)
                await LogError(90, this.GetType().Name, "MapToDto", new Exception(provider_response.Exception));
            else
                dto.payment_transaction = provider_response.Data;
        }
        catch (Exception e)
        {
            await LogError(90, this.GetType().Name, "MapToDto", e);
        }

        return dto;
    }

    public Payment MapToDatabaseModel(PaymentDto dtoModel)
    {
        throw new NotImplementedException();
    }

    public Payment MapToDatabaseModel(PaymentCreateCommand commandModel, string calling_user_id)
    {
        return CommonDataHelper<Payment>.FillCommonFields(new Payment()
        {
            order_header_id = commandModel.order_header_id,
            ar_header_id = commandModel.ar_invoice_header_id,
            payment_amount = commandModel.payment_amount,
            transaction_method = commandModel.transaction_method,
            transaction_id = commandModel.transaction_id,
            transaction_date = commandModel.transaction_date,
            transaction_status = commandModel.transaction_status,
            confirmation_code = commandModel.confirmation_code,
            payment_processor = commandModel.payment_processor,
            payment_receipt = commandModel.payment_receipt,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    private async Task<int> ManuallyGenerateAnPaymentNumber()
    {
        var total_records = await _Context.Payments.CountAsync();
        int start = DatabaseStartNumbers.Payments;

        return (total_records + start + 1);
    }
}