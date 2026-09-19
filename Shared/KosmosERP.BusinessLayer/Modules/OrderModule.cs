using Microsoft.EntityFrameworkCore;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using KosmosERP.Models;
using KosmosERP.Models.Helpers;
using KosmosERP.Models.Interfaces;
using KosmosERP.Module;
using System.Text.Json;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Order.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Order.Dto;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Transaction.Command.Delete;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.ProductionOrder.Command.Create;
using KosmosERP.BusinessLayer.Interfaces;


namespace KosmosERP.BusinessLayer.Modules;

public interface IOrderModule : IERPModule<OrderHeader, OrderHeaderDto, OrderHeaderListDto, OrderHeaderCreateCommand, OrderHeaderEditCommand, OrderHeaderDeleteCommand, OrderHeaderFindCommand>, IBaseERPModule
{
    Task<Response<OrderLineDto>> GetLineDto(int object_id);
    Task<Response<OrderLineDto>> CreateLine(OrderLineCreateCommand commandModel, bool skipReleaseCheck = false);
    Task<Response<OrderLineDto>> EditLine(OrderLineEditCommand commandModel, bool skipReleaseCheck = false);
    Task<Response<OrderLineDto>> DeleteLine(OrderLineDeleteCommand commandModel);
    Task<Response<OrderLineAttributeDto>> CreateAttribute(OrderLineAttributeCreateCommand commandModel);
    Task<Response<OrderLineAttributeDto>> EditAttribute(OrderLineAttributeEditCommand commandModel);
    Task<Response<OrderLineAttributeDto>> DeleteAttribute(OrderLineAttributeDeleteCommand commandModel);

    Task<OrderHeaderDto> MapToDto(OrderHeader databaseModel);
    Task<OrderLineDto> MapToLineDto(OrderLine databaseModel);

    Task<Response<OrderHeaderDto>> DuplicateOrder(int order_header_id, int customer_id, string calling_user_id);
}

public class OrderModule : BaseERPModule, IOrderModule
{
	public override Guid ModuleIdentifier => Guid.Parse("68c1862f-6043-4b09-8674-dd844a3a6fed");
	public override string ModuleName => "Sales Orders";

	private IBaseERPContext _Context;
    private IAddressModule _AddressModule;
    private IMessageFactory _MessageFactory;
    private IMemoryCacheService<KeyValueStore> _KVMemoryService;
    private IMemoryCacheService<Customer> _CustomerMemoryService;
    private IMessagePublisherSettings _MessagePublisherSettings;
    private IProductionOrderModule _ProductionOrderModule;
    private IPaymentProvider _PaymentProvider;
    private IPaymentProviderFactory _PaymentFactory;
    private ILogProviderFactory _LogProviderFactory;

    public OrderModule(IBaseERPContext context, ILogProviderFactory logProviderFactory) : base(context, logProviderFactory)
    {
        _Context = context;
        _LogProviderFactory = logProviderFactory;
    }

    public OrderModule(IBaseERPContext context, 
                        IMessageFactory messageFactory,
                        IMessagePublisherSettings messageSettings,
                        IMemoryCacheService<KeyValueStore> kvMService,
                        IMemoryCacheService<Customer> customerMemService,
                        IProductionOrderModule productionOrderModule,
                        IPaymentProviderFactory providerFactory, 
                        ILogProviderFactory logProviderFactory,
                        IAddressModule addressModule) : base(context, logProviderFactory)
    {
        _Context = context;
        _MessageFactory = messageFactory;
        _KVMemoryService = kvMService;
        _CustomerMemoryService = customerMemService;
        _MessagePublisherSettings = messageSettings;
        _ProductionOrderModule = productionOrderModule;

        _PaymentProvider = providerFactory.GetProvider();
        _PaymentFactory = providerFactory;
        _LogProviderFactory = logProviderFactory;

        _AddressModule = addressModule;
    }


    public override void SeedPermissions()
    {
        var role = _Context.Roles.Any(m => m.name == "Sales Order Administrators");

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "Sales Order Administrators",
            }, 1));

            _Context.SaveChanges();

            base.CreateFirstRunRolePermissions();
        }


        var cash_payment_method = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PayMethods && m.key == "payment_method_cash").SingleOrDefault();
        var check_payment_method = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PayMethods && m.key == "payment_method_check").SingleOrDefault();
        var card_payment_method = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PayMethods && m.key == "payment_method_card").SingleOrDefault();
        var po_payment_method = _Context.KeyValueStores.Where(m => m.module_id == KeyValueIds.PayMethods && m.key == "payment_method_po").SingleOrDefault();

        if (cash_payment_method == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "payment_method_cash",
                value = "Cash",
                module_id = KeyValueIds.PayMethods.ToString(),
                int_value = 1
            }, 1));

            _Context.SaveChanges();
        }

        if (check_payment_method == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "payment_method_check",
                value = "Check",
                module_id = KeyValueIds.PayMethods.ToString(),
                int_value = 2
            }, 1));

            _Context.SaveChanges();
        }

        if (card_payment_method == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "payment_method_card",
                value = "Card",
                module_id = KeyValueIds.PayMethods.ToString(),
                int_value = 3
            }, 1));

            _Context.SaveChanges();
        }
        
        if(po_payment_method == null)
        {
            _Context.KeyValueStores.Add(CommonDataHelper<KeyValueStore>.FillCommonFields(new KeyValueStore()
            {
                key = "payment_method_po",
                value = "PO",
                module_id = KeyValueIds.PayMethods.ToString()
            }, 1));

            _Context.SaveChanges();
        }

        // Seed ModulePermissions for Order module
        var existing_permissions = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString());
        if (!existing_permissions)
        {
            _Context.ModulePermissions.AddRange(new[]
            {
                CommonDataHelper<ModulePermission>.FillCommonFields(new ModulePermission()
                {
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    permission_name = "Read Orders",
                    internal_permission_name = "read_order",
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
                    permission_name = "Create Orders",
                    internal_permission_name = "create_order",
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
                    permission_name = "Edit Orders",
                    internal_permission_name = "edit_order",
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
                    permission_name = "Delete Orders",
                    internal_permission_name = "delete_order",
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

    public OrderHeader? Get(int object_id)
    {
        return _Context.OrderHeaders.SingleOrDefault(m => m.id == object_id);
    }

    public async Task<OrderHeader?> GetAsync(int object_id)
    {
        return await _Context.OrderHeaders.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<OrderLine?> GetLineAsync(int object_id)
    {
        return await _Context.OrderLines.SingleOrDefaultAsync(m => m.id == object_id);
    }

    public async Task<OrderLineAttribute?> GetAttributeAsync(int object_id)
    {
        return await _Context.OrderLineAttributes.SingleOrDefaultAsync(m => m.id == object_id);
    }

    
    public async Task<Response<OrderHeaderDto>> GetDto(int object_id)
    {
        Response<OrderHeaderDto> response = new Response<OrderHeaderDto>();

        var result = await _Context.OrderHeaders.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Order Header not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<Response<OrderHeaderDto>> GetDtoByGuid(string guid)
    {
        var entity = await _Context.OrderHeaders.FirstOrDefaultAsync(c => c.guid == guid && !c.is_deleted);
        if (entity == null)
            return new Response<OrderHeaderDto>("OrderHeader not found", ResultCode.NotFound);

        var dto = await MapToDto(entity);
        return new Response<OrderHeaderDto>(dto);
    }

    public async Task<Response<OrderLineDto>> GetLineDto(int object_id)
	{
        Response<OrderLineDto> response = new Response<OrderLineDto>();

        var result = await _Context.OrderLines.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Order Line not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToLineDto(result);
        return response;
    }

    public async Task<Response<OrderLineAttributeDto>> GetAttributeDto(int object_id)
    {
        Response<OrderLineAttributeDto> response = new Response<OrderLineAttributeDto>();

        var result = await _Context.OrderLineAttributes.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Order Line Attribute not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToAttributeDto(result);
        return response;
    }

    public async Task<Response<OrderHeaderDto>> Create(OrderHeaderCreateCommand commandModel)
	{
        if (commandModel == null)
            return new Response<OrderHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingCustomer = await _Context.Customers.SingleOrDefaultAsync(m => m.id == commandModel.customer_id);
        if (existingCustomer == null)
            return new Response<OrderHeaderDto>("Customer not found", ResultCode.NotFound);

        try
        {
            bool released = false;
            bool lines_include_manufactured_items = false;

            List<ProductionOrderLineCreateCommand> production_lines = new List<ProductionOrderLineCreateCommand>();


            var alreadyExists = await this.OrderHeaderExists(commandModel);
            if (alreadyExists == true)
                return new Response<OrderHeaderDto>(ResultCode.AlreadyExists);

            var item = this.MapToDatabaseModel(commandModel, commandModel.calling_user_id);

            item.revision_number = 1;
            item.price = commandModel.order_lines.Sum(m => (m.unit_price * m.quantity));

            // Tax stuff
            decimal tax = 0;

            if (existingCustomer.is_taxable)
            {
                var product_ids = commandModel.order_lines.Select(m => m.product_id).ToList();
                var all_taxable_products = await _Context.Products.Where(m => m.is_taxable == true && product_ids.Contains(m.id)).ToListAsync();

                foreach (var order_line in commandModel.order_lines)
                {
                    var product = all_taxable_products.SingleOrDefault(m => m.id == order_line.product_id);
                    if (product != null)
                    {
                        tax += commandModel.order_lines.Sum(m => ((m.unit_price * m.quantity) * existingCustomer.tax_rate) + (m.unit_price * m.quantity));
                    }
                }
            }

            item.tax = tax;

            item.order_number = await this.ManuallyGenerateAnOrderNumber();

            await _Context.OrderHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();

            if(item.order_type.ToLower() == "r")
            {
                released = true;
            }

            // Now do lines
            foreach (var order_line in commandModel.order_lines)
            {
                var db_line = MapToLineDatabaseModel(order_line, item.id, commandModel.calling_user_id);

                await _Context.OrderLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();

                // Attributes
                if (order_line.attributes.Count > 0)
                {
                    var attibute_response = await BuildLineAttributes(order_line, db_line.id, commandModel.calling_user_id);

                    if (!attibute_response.Success)
                    {
                        return new Response<OrderHeaderDto>(attibute_response.Exception, ResultCode.Error);
                    }
                }

                if(released == true)
                {
                    // Publish this data to a message queue to be processed for transactions
                    await _MessageFactory.GetPublisher().PublishAsync(new Models.MessageObject()
                    {
                        created_on = DateTime.UtcNow,
                        object_type = "TransactionCreateCommand",
                        body = JsonSerializer.Serialize(new TransactionCreateCommand()
                        {
                            transaction_type = TransactionType.Reserved,
                            transaction_date = DateTime.UtcNow,
                            object_reference_id = item.id,
                            object_sub_reference_id = item.id,
                            sold_unit_price = item.price,
                            units_sold = order_line.quantity,
                            product_id = order_line.product_id,
                            calling_user_id = commandModel.calling_user_id,
                        })
                    }, _MessagePublisherSettings.transaction_movement_topic);


                    var manufactured_product = await _Context.Products.SingleOrDefaultAsync(m => m.id == db_line.product_id && m.is_manufactured);

                    if(manufactured_product != null)
                    {
                        lines_include_manufactured_items = true;

                        production_lines.Add(new ProductionOrderLineCreateCommand()
                        {
                            order_line_id = db_line.id,
                            quantity = db_line.quantity,
                            status = ProductionOrderStatus.New,
                            production_lead_minutes = manufactured_product.manufacture_time_minutes,
                            calling_user_id = "1"
                        });
                    }
                }
            }


            // Production orders
            if(lines_include_manufactured_items == true)
            {
                var production_order_response = await _ProductionOrderModule.Create(new ProductionOrderHeaderCreateCommand()
                {
                    order_header_id = item.id,
                    status = ProductionOrderStatus.New,
                    calling_user_id = commandModel.calling_user_id,
                    production_order_lines = production_lines,
                });

                if(!production_order_response.Success)
                    await LogError(80, this.GetType().Name, nameof(CreateLine), new Exception($"Could not create production order header for order # {item.order_number}"));
                
            }
              


            var dto = await GetDto(item.id);

            return new Response<OrderHeaderDto>(dto.Data);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Create), ex);
            return new Response<OrderHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

	public async Task<Response<OrderHeaderDto>> Delete(OrderHeaderDeleteCommand commandModel)
	{
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OrderHeaderDto>("Order Header not found", ResultCode.NotFound);


        // Delete Record
        existingEntity = CommonDataHelper<OrderHeader>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


        _Context.OrderHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var lines = await _Context.OrderLines.Where(m => m.order_header_id == existingEntity.id && m.is_deleted == false).ToListAsync();

        foreach (var line in lines)
        {
            await this.DeleteLine(new OrderLineDeleteCommand()
            {
                calling_user_id = commandModel.calling_user_id,
                id = line.id,
            });
        }


        await _MessageFactory.GetPublisher().PublishAsync(new Models.MessageObject()
        {
            created_on = DateTime.UtcNow,
            object_type = "TransactionDeleteCommand",
            body = JsonSerializer.Serialize(new TransactionDeleteCommand()
            {
                object_reference_id = existingEntity.id,
                calling_user_id = commandModel.calling_user_id,
            })
        }, _MessagePublisherSettings.transaction_movement_topic);

        var dto = await MapToDto(existingEntity);
        return new Response<OrderHeaderDto>(dto);
    }

	public async Task<Response<OrderHeaderDto>> Edit(OrderHeaderEditCommand commandModel)
	{
        if (commandModel == null)
            return new Response<OrderHeaderDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OrderHeaderDto>("Order Header not found", ResultCode.NotFound);


        if(existingEntity.order_type.ToLower() == "r")
        {
            await LogTrace("OrderModule:Edit", $"Attempt to edit released order id: {existingEntity.id} by user id: {commandModel.calling_user_id}");
            return new Response<OrderHeaderDto>("Cannot edit a released order", ResultCode.Error);
        }

        // Check for validation issues with the lines
        foreach (var line in commandModel.order_lines)
        {
            var line_validation = ModelValidationHelper.ValidateModel(line);
            if (!line_validation.Success)
                return new Response<OrderHeaderDto>(line_validation.Exception, ResultCode.DataValidationError);

            var new_edited_lines = commandModel.order_lines.Where(m => !m.id.HasValue).ToList();
            foreach(var new_edit in new_edited_lines)
            {
                // These fields must be set to be considered a new line
                if(!new_edit.product_id.HasValue
                    && !new_edit.line_number.HasValue
                    && !new_edit.quantity.HasValue
                    && !new_edit.unit_price.HasValue)
                {
                    return new Response<OrderHeaderDto>("Required field not set on new line", ResultCode.DataValidationError);
                }
            }
        }


        try
        {
            bool released = false;
            bool lines_include_manufactured_items = false;

            // To Be used if order is being released
            List<ProductionOrderLineCreateCommand> production_lines = new List<ProductionOrderLineCreateCommand>();

            if (commandModel.customer_id.HasValue && existingEntity.customer_id != commandModel.customer_id)
                existingEntity.customer_id = commandModel.customer_id.Value;
            if (commandModel.ship_to_address_id.HasValue && existingEntity.ship_to_address_id != commandModel.ship_to_address_id)
                existingEntity.customer_id = commandModel.ship_to_address_id.Value;
            if (commandModel.billing_address_id.HasValue && existingEntity.billing_address_id != commandModel.billing_address_id)
                existingEntity.billing_address_id = commandModel.billing_address_id.Value;

            if (!String.IsNullOrEmpty(commandModel.shipping_method) && existingEntity.shipping_method != commandModel.shipping_method)
                existingEntity.shipping_method = commandModel.shipping_method;
            if (!String.IsNullOrEmpty(commandModel.pay_method) && existingEntity.pay_method != commandModel.pay_method)
                existingEntity.pay_method = commandModel.pay_method;

            if (commandModel.opportunity_id.HasValue && existingEntity.opportunity_id != commandModel.opportunity_id)
                existingEntity.opportunity_id = commandModel.opportunity_id.Value;

            if(existingEntity.order_type != commandModel.order_type)
            {
                existingEntity.order_type = commandModel.order_type;

                if(commandModel.order_type.ToLower() == "r")
                {
                    released = true;
                }
            }
            
            if (commandModel.required_date.HasValue && existingEntity.required_date != commandModel.required_date)
                existingEntity.required_date = commandModel.required_date.Value;
            if (existingEntity.po_number != commandModel.po_number)
                existingEntity.po_number = commandModel.po_number;

            if (commandModel.shipping_cost.HasValue && existingEntity.shipping_cost != commandModel.shipping_cost)
                existingEntity.shipping_cost = commandModel.shipping_cost.Value;

            if (commandModel.order_date.HasValue && existingEntity.order_date != commandModel.order_date)
                existingEntity.order_date = commandModel.order_date.Value;

            existingEntity.revision_number += 1;

            existingEntity = CommonDataHelper<OrderHeader>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            _Context.OrderHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            // Create or update lines
            foreach(var line in commandModel.order_lines)
            {
                // Edit lines
                if(!line.id.HasValue)
                {
                    var add_line = this.MapToLineDatabaseModel(line, existingEntity.id, commandModel.calling_user_id);
                    await _Context.OrderLines.AddAsync(add_line);
                    await _Context.SaveChangesAsync();


                    // Do attributes if available
                    if (line.attributes.Count > 0)
                    {
                        var attibute_response = await BuildAndEditLineAttributes(line, add_line.id, commandModel.calling_user_id);
                    
                        if (!attibute_response.Success)
                        {
                            return new Response<OrderHeaderDto>(attibute_response.Exception, ResultCode.Error);
                        }   
                    }
                    
                    if (released == true)
                    {
                        // Publish this data to a message queue to be processed for transactions
                        await _MessageFactory.GetPublisher().PublishAsync(new Models.MessageObject()
                        {
                            created_on = DateTime.UtcNow,
                            object_type = "TransactionCreateCommand",
                            body = JsonSerializer.Serialize(new TransactionCreateCommand()
                            {
                                transaction_type = TransactionType.Reserved,
                                transaction_date = DateTime.UtcNow,
                                object_reference_id = add_line.order_header_id,
                                object_sub_reference_id = add_line.id,
                                sold_unit_price = add_line.unit_price,
                                units_sold = add_line.quantity,
                                product_id = add_line.product_id,
                                calling_user_id = commandModel.calling_user_id,
                            })
                        }, _MessagePublisherSettings.transaction_movement_topic);

                        var manufactured_product = await _Context.Products.SingleOrDefaultAsync(m => m.id == line.product_id && m.is_manufactured);

                        if(manufactured_product != null)
                        {
                            lines_include_manufactured_items = true;

                            production_lines.Add(new ProductionOrderLineCreateCommand()
                            {
                                order_line_id = add_line.id,
                                quantity = add_line.quantity,
                                status = ProductionOrderStatus.New,
                                production_lead_minutes = manufactured_product.manufacture_time_minutes,
                                calling_user_id = "1"
                            });
                        }
                    }
                }
                else
                {
                    var edit_response = await this.EditLine(line, true);

                    if(!edit_response.Success)
                        return new Response<OrderHeaderDto>(edit_response.Exception, ResultCode.Error);


                    if (released == true)
                    {
                        // Publish this data to a message queue to be processed for transactions
                        await _MessageFactory.GetPublisher().PublishAsync(new Models.MessageObject()
                        {
                            created_on = DateTime.UtcNow,
                            object_type = "TransactionCreateCommand",
                            body = JsonSerializer.Serialize(new TransactionCreateCommand()
                            {
                                transaction_type = TransactionType.Reserved,
                                transaction_date = DateTime.UtcNow,
                                object_reference_id = edit_response.Data.order_header_id,
                                object_sub_reference_id = edit_response.Data.id,
                                sold_unit_price = edit_response.Data.unit_price,
                                units_sold = edit_response.Data.quantity,
                                product_id = edit_response.Data.product_id,
                                calling_user_id = commandModel.calling_user_id,
                            })
                        }, _MessagePublisherSettings.transaction_movement_topic);

                        var manufactured_product = await _Context.Products.SingleOrDefaultAsync(m => m.id == line.product_id && m.is_manufactured);

                        if(manufactured_product != null)
                        {
                            lines_include_manufactured_items = true;

                            production_lines.Add(new ProductionOrderLineCreateCommand()
                            {
                                order_line_id = edit_response.Data.id,
                                quantity = edit_response.Data.quantity,
                                status = ProductionOrderStatus.New,
                                production_lead_minutes = manufactured_product.manufacture_time_minutes,
                                calling_user_id = "1",
                            });
                        }
                    }

                    // Build attributes if there are any
                    if (line.attributes.Count > 0)
                    {
                        var attibute_response = await BuildAndEditLineAttributes(line, edit_response.Data.id, commandModel.calling_user_id);

                        if (!attibute_response.Success)
                            return new Response<OrderHeaderDto>(attibute_response.Exception, ResultCode.Error);
                    }
                }
            }

            

            existingEntity.price = await _Context.OrderLines.Where(m => m.order_header_id == existingEntity.id && m.is_deleted == false).SumAsync(m => (m.unit_price * m.quantity));
            _Context.OrderHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();


            // Production orders
            if(lines_include_manufactured_items == true)
            {
                var production_order_response = await _ProductionOrderModule.Create(new ProductionOrderHeaderCreateCommand()
                {
                    order_header_id = existingEntity.id,
                    status = ProductionOrderStatus.New,
                    calling_user_id = commandModel.calling_user_id,
                    production_order_lines = production_lines,
                });

                if(!production_order_response.Success)
                    await LogError(80, this.GetType().Name, nameof(CreateLine), new Exception($"Could not create production order header for order # {existingEntity.order_number}"));
            }
               

            var dto = await MapToDto(existingEntity);

            return new Response<OrderHeaderDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(CreateLine), ex);
            return new Response<OrderHeaderDto>(ex.Message, ResultCode.Error);
        }
    }

    private async Task<Response<bool>> BuildLineAttributes(OrderLineCreateCommand commandModel, int order_line_id, string calling_user_id)
    {
        foreach (var attribute in commandModel.attributes)
        {
            if (attribute.calling_user_id != calling_user_id)
                attribute.calling_user_id = calling_user_id;

            if (!attribute.order_line_id.HasValue)
                attribute.order_line_id = order_line_id;


            var edit_response = await this.CreateAttribute(attribute);

            if (!edit_response.Success)
            {
                return new Response<bool>(edit_response.Exception, ResultCode.Error);
            }
        }

        return new Response<bool>(true);
    }

    private async Task<Response<bool>> BuildAndEditLineAttributes(OrderLineEditCommand commandModel, int order_line_id, string calling_user_id)
    {
        foreach (var attribute in commandModel.attributes)
        {
            // New Attribute
            if (!attribute.id.HasValue)
            {
                var attribute_model = this.MapToAttributeDatabaseModel(attribute, order_line_id, commandModel.calling_user_id);
                await _Context.OrderLineAttributes.AddAsync(attribute_model);
                await _Context.SaveChangesAsync();
            }
            else
            {
                var edit_response = await this.EditAttribute(attribute);

                if (!edit_response.Success)
                {
                    return new Response<bool>(edit_response.Exception, ResultCode.Error);
                }
            }
        }

        return new Response<bool>(true);
    }

    public async Task<PagingResult<OrderHeaderListDto>> Find(PagingSortingParameters parameters, OrderHeaderFindCommand commandModel)
	{
        var response = new PagingResult<OrderHeaderListDto>();

        try
        {
            var filter = PredicateBuilder.True<OrderHeader>();
            filter = filter.And(m => m.is_deleted == false);

            if (commandModel.customer_id.HasValue)
            {
                filter = filter.And(m => m.customer_id == commandModel.customer_id);
            }

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                filter = filter.And(m => m.po_number.ToLower().Contains(wild));
            }

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                filter = filter.Or(m => m.order_number.ToString().Contains(commandModel.wildcard));
            }


            var totalCount = await _Context.OrderHeaders.CountAsync(filter);
            var results = _Context.OrderHeaders.Where(filter);
            var pagedItems = await results.SortAndPageBy(parameters).ToListAsync();

            var dtos = new List<OrderHeaderListDto>();
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

	public async Task<Response<List<OrderHeaderListDto>>> GlobalSearch(GlobalSearchFindCommand commandModel)
	{
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<List<OrderHeaderListDto>>(validationResult.Exception, ResultCode.DataValidationError);


        var response = new Response<List<OrderHeaderListDto>>();

        try
        {
            var filter = PredicateBuilder.True<OrderHeader>();
            filter = filter.And(m => m.is_deleted == false);

            int parsed_num = 0;

            if (int.TryParse(commandModel.wildcard, out parsed_num))
            {
                filter = filter.And(m => m.order_number == parsed_num);
            }
            else
            {
                var wild = commandModel.wildcard.ToLower();
                filter = filter.And(m => m.po_number.ToLower().Contains(wild));
            }

            var results = _Context.OrderHeaders.Where(filter);
            var pagedItems = await results.SortAndPageBy(commandModel.parameters).ToListAsync();

            var dtos = new List<OrderHeaderListDto>();
            foreach (var item in pagedItems)
                dtos.Add(await MapToListDto(item));

            response.Data = dtos;
        }
        catch (Exception ex)
        {
            await LogError(50, this.GetType().Name, nameof(GlobalSearch), ex);
            response.SetException(ex.Message, ResultCode.Error);
        }

        return response;
	}

    public async Task<Response<OrderLineDto>> CreateLine(OrderLineCreateCommand commandModel, bool skipReleaseCheck = false)
    {
        if (commandModel == null)
            return new Response<OrderLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        if (!commandModel.order_header_id.HasValue)
            return new Response<OrderLineDto>("Order Header os a required field", ResultCode.DataValidationError);

        var existingHeader = await GetAsync(commandModel.order_header_id.Value);
        if (existingHeader == null)
            return new Response<OrderLineDto>("Order Header not found", ResultCode.NotFound);

        if(skipReleaseCheck == false && existingHeader.order_type.ToLower() == "r")
        {
            await LogTrace("OrderModule:CreateLine", $"Attempt to edit released order id: {existingHeader.id} by user id: {commandModel.calling_user_id}");
            return new Response<OrderLineDto>("Cannot edit a released order", ResultCode.Error);
        }

        try
        {
            var item = this.MapToLineDatabaseModel(commandModel, commandModel.order_header_id.Value, commandModel.calling_user_id);

            await _Context.OrderLines.AddAsync(item);
            await _Context.SaveChangesAsync();


            decimal new_price = 0;
            var lines = await _Context.OrderLines.Where(m => m.is_deleted == false).Select(m => new { m.unit_price, m.quantity }).ToListAsync();
            foreach (var line in lines)
                new_price += (line.quantity * line.unit_price);

            existingHeader.price = new_price; 


            _Context.OrderHeaders.Update(existingHeader);
            await _Context.SaveChangesAsync();



            // Attributes
            if (commandModel.attributes.Count > 0)
            {
                var attibute_response = await BuildLineAttributes(commandModel, item.id, commandModel.calling_user_id);

                if (!attibute_response.Success)
                {
                    return new Response<OrderLineDto>(attibute_response.Exception, ResultCode.Error);
                }
            }

            await _MessageFactory.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionCreateCommand",
                body = JsonSerializer.Serialize(new TransactionCreateCommand()
                {
                    transaction_type = TransactionType.Reserved,
                    transaction_date = DateTime.UtcNow,
                    object_reference_id = item.order_header_id,
                    object_sub_reference_id = item.id,
                    sold_unit_price = item.unit_price,
                    units_sold = item.quantity,
                    product_id = item.product_id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings.transaction_movement_topic);


            var dto = await GetLineDto(item.id);

            return new Response<OrderLineDto>(dto.Data);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(CreateLine), ex);
            return new Response<OrderLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<OrderLineDto>> EditLine(OrderLineEditCommand commandModel, bool skipReleaseCheck = false)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        if(!commandModel.id.HasValue)
            return new Response<OrderLineDto>("Order Line must have an id", ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<OrderLineDto>("Order Line not found", ResultCode.NotFound);

        var existingHeader = await GetAsync(existingEntity.order_header_id);
        if (existingHeader == null)
            return new Response<OrderLineDto>("Order Header not found", ResultCode.NotFound);

        if(skipReleaseCheck == false && existingHeader.order_type.ToLower() == "r")
        {
            await LogTrace("OrderModule:CreateLine", $"Attempt to edit released order id: {existingHeader.id} by user id: {commandModel.calling_user_id}");
            return new Response<OrderLineDto>("Cannot edit a released order", ResultCode.Error);
        }

        try
        {

            if (commandModel.product_id.HasValue && existingEntity.product_id != commandModel.product_id)
                existingEntity.product_id = commandModel.product_id.Value;
            if (commandModel.line_number.HasValue && existingEntity.line_number != commandModel.line_number)
                existingEntity.line_number = commandModel.line_number.Value;
            if (existingEntity.opportunity_line_id != commandModel.opportunity_line_id)
                existingEntity.opportunity_line_id = commandModel.opportunity_line_id;
            if (existingEntity.line_description != commandModel.line_description)
                existingEntity.line_description = commandModel.line_description;
            if (commandModel.quantity.HasValue && existingEntity.quantity != commandModel.quantity)
                existingEntity.quantity = commandModel.quantity.Value;
            if (commandModel.unit_price.HasValue && existingEntity.unit_price != commandModel.unit_price)
                existingEntity.unit_price = commandModel.unit_price.Value;


            existingEntity = CommonDataHelper<OrderLine>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


            _Context.OrderLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            existingHeader.price = await _Context.PurchaseOrderLines.Where(m => m.is_deleted == false).SumAsync(m => (m.unit_price * m.quantity));
            _Context.OrderHeaders.Update(existingHeader);
            await _Context.SaveChangesAsync();


            await _MessageFactory.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionEditCommand",
                body = JsonSerializer.Serialize(new TransactionEditCommand()
                {
                    object_reference_id = existingEntity.order_header_id,
                    object_sub_reference_id = existingEntity.id,
                    sold_unit_price = existingEntity.unit_price,
                    units_sold = existingEntity.quantity,
                    product_id = existingEntity.product_id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings.transaction_movement_topic);


            var dto = await MapToLineDto(existingEntity);

            return new Response<OrderLineDto>(dto);

        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(EditLine), ex);
            return new Response<OrderLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<OrderLineDto>> DeleteLine(OrderLineDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OrderLineDto>("Order Line not found", ResultCode.NotFound);

        var existingHeader = await GetAsync(existingEntity.order_header_id);
        if (existingHeader == null)
            return new Response<OrderLineDto>("Order Header not found", ResultCode.NotFound);

        if(existingHeader.order_type.ToLower() == "r")
        {
            await LogTrace("OrderModule:DeleteLine", $"Attempt to edit released order id: {existingHeader.id} by user id: {commandModel.calling_user_id}");
            return new Response<OrderLineDto>("Cannot edit a released order", ResultCode.Error);
        }

        try
        {
            // Delete
            existingEntity = CommonDataHelper<OrderLine>.FillDeleteFields(existingEntity, commandModel.calling_user_id);

            _Context.OrderLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            decimal new_price = 0;
            var lines = await _Context.OrderLines.Where(m => m.is_deleted == false).Select(m => new { m.unit_price, m.quantity }).ToListAsync();
            foreach (var line in lines)
                new_price += (line.quantity * line.unit_price);

            existingHeader.price = new_price; 


            _Context.OrderHeaders.Update(existingHeader);
            await _Context.SaveChangesAsync();
            

            // Corrolate the transacation data
            await _MessageFactory.GetPublisher().PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.UtcNow,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.order_header_id,
                    object_sub_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, _MessagePublisherSettings.transaction_movement_topic);


            var dto = await MapToLineDto(existingEntity);
            return new Response<OrderLineDto>(dto);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(Create), ex);
            return new Response<OrderLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<OrderLineAttributeDto>> CreateAttribute(OrderLineAttributeCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<OrderLineAttributeDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderLineAttributeDto>(validationResult.Exception, ResultCode.DataValidationError);

        if (!commandModel.order_line_id.HasValue)
            return new Response<OrderLineAttributeDto>("Order Line ID a required field", ResultCode.DataValidationError);

        try
        {
            var item = this.MapToAttributeDatabaseModel(commandModel, commandModel.calling_user_id);

            await _Context.OrderLineAttributes.AddAsync(item);
            await _Context.SaveChangesAsync();

            var dto = await GetAttributeDto(item.id);

            return new Response<OrderLineAttributeDto>(dto.Data);
        }
        catch (Exception ex)
        {
            return new Response<OrderLineAttributeDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<OrderLineAttributeDto>> EditAttribute(OrderLineAttributeEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderLineAttributeDto>(validationResult.Exception, ResultCode.DataValidationError);

        if (!commandModel.id.HasValue)
            return new Response<OrderLineAttributeDto>("Attribute must have an id", ResultCode.DataValidationError);

        var existingEntity = await GetAttributeAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<OrderLineAttributeDto>("Attribute not found", ResultCode.NotFound);


        if (commandModel.order_line_id.HasValue && existingEntity.order_line_id != commandModel.order_line_id)
            existingEntity.order_line_id = commandModel.order_line_id.Value;
        if (existingEntity.attribute_name != commandModel.attribute_name)
            existingEntity.attribute_name = commandModel.attribute_name;
        if (existingEntity.attribute_value != commandModel.attribute_value)
            existingEntity.attribute_value = commandModel.attribute_value;
        if (existingEntity.attribute_value2 != commandModel.attribute_value2)
            existingEntity.attribute_value2 = commandModel.attribute_value2;
        if (existingEntity.attribute_value3 != commandModel.attribute_value3)
            existingEntity.attribute_value3 = commandModel.attribute_value3;


        existingEntity = CommonDataHelper<OrderLineAttribute>.FillUpdateFields(existingEntity, commandModel.calling_user_id);


        _Context.OrderLineAttributes.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToAttributeDto(existingEntity);

        return new Response<OrderLineAttributeDto>(dto);
    }

    public async Task<Response<OrderLineAttributeDto>> DeleteAttribute(OrderLineAttributeDeleteCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderLineAttributeDto>(validationResult.Exception, ResultCode.DataValidationError);

        var existingEntity = await GetAttributeAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OrderLineAttributeDto>("Attribute not found", ResultCode.NotFound);


        // Nuke
        existingEntity = CommonDataHelper<OrderLineAttribute>.FillDeleteFields(existingEntity, commandModel.calling_user_id);


        _Context.OrderLineAttributes.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToAttributeDto(existingEntity);

        return new Response<OrderLineAttributeDto>(dto);
    }

    public async Task<Response<OrderHeaderDto>> DuplicateOrder(int order_header_id, int customer_id, string calling_user_id)
    {
        Response<OrderHeaderDto> response = new Response<OrderHeaderDto>();

        try
        {
            var existingEntity = await _Context.OrderHeaders.SingleOrDefaultAsync(m => m.id == order_header_id);
            var existingCustomer = await _Context.Customers.SingleOrDefaultAsync(m => m.id == customer_id);

            if (existingEntity == null)
                return new Response<OrderHeaderDto>("Order not found", ResultCode.NotFound);

            if (existingCustomer == null)
                return new Response<OrderHeaderDto>("Customer not found", ResultCode.NotFound);

            var existingLines = await _Context.OrderLines.Where(m => m.order_header_id == order_header_id).ToListAsync();
            
            var now = DateOnly.FromDateTime(DateTime.Now);
            var lines_total = existingLines.Sum(m => m.unit_price * m.quantity);
            decimal lines_tax = 0;

            if(existingCustomer.is_taxable)
            {
                lines_tax = existingLines.Sum(m => (m.unit_price * (existingCustomer.tax_rate / 100)) * m.quantity);
            }


            var new_header = CommonDataHelper<OrderHeader>.FillCommonFields(new OrderHeader()
            {
                customer_id = customer_id,
                ship_to_address_id = existingEntity.ship_to_address_id,
                shipping_method = existingEntity.shipping_method,
                pay_method = existingEntity.pay_method,
                opportunity_id = existingEntity.opportunity_id,
                order_type = existingEntity.order_type,
                order_date = now,
                required_date = existingEntity.required_date,
                po_number = existingEntity.po_number,
                shipping_cost = existingEntity.shipping_cost,
                guid = Guid.NewGuid().ToString(),
                price = lines_total,
                tax = lines_tax
            }, calling_user_id);

            await _Context.OrderHeaders.AddAsync(new_header);
            await _Context.SaveChangesAsync();


            foreach (var existingLine in existingLines)
            {
                var line_attributes = await _Context.OrderLineAttributes.Where(m => m.order_line_id == existingLine.id).ToListAsync();

                var new_line = CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine()
                {
                    order_header_id = new_header.id,
                    product_id = existingLine.product_id,
                    line_number = existingLine.line_number,
                    opportunity_line_id = existingLine.opportunity_line_id,
                    quantity = existingLine.quantity,
                    unit_price = existingLine.unit_price,
                    line_description = existingLine.line_description,
                    guid = Guid.NewGuid().ToString(),
                }, calling_user_id);

                await _Context.OrderLines.AddAsync(new_line);
                await _Context.SaveChangesAsync();


                foreach (var line_attribute in line_attributes)
                {
                    var new_attribute = CommonDataHelper<OrderLineAttribute>.FillCommonFields(new OrderLineAttribute()
                    {
                        order_line_id = new_line.id,
                        attribute_name = line_attribute.attribute_name,
                        attribute_value = line_attribute.attribute_value,
                        attribute_value2 = line_attribute.attribute_value2,
                        attribute_value3 = line_attribute.attribute_value3,
                        guid = Guid.NewGuid().ToString(),
                    }, calling_user_id);

                    await _Context.OrderLineAttributes.AddAsync(new_attribute);
                    await _Context.SaveChangesAsync();
                }
            }

            response.Data = await this.MapToDto(new_header);
        }
        catch (Exception ex)
        {
            return new Response<OrderHeaderDto>(ex.Message, ResultCode.Error);
        }

        return response;
    }

    public async Task<bool> OrderHeaderExists(OrderHeaderCreateCommand commandModel)
    {
        var existingEntity = await _Context.OrderHeaders
            .Where(m => m.customer_id == commandModel.customer_id 
                    && m.po_number == commandModel.po_number 
                    && m.order_date == commandModel.order_date
                    && !m.is_deleted)
            .FirstOrDefaultAsync();

        if (existingEntity == null)
            return false;
        else
            return true;
    }

    public OrderHeader MapToDatabaseModel(OrderHeaderDto dtoModel)
	{
		throw new NotImplementedException();
	}

    public OrderHeader MapToDatabaseModel(OrderHeaderCreateCommand commandModel, string calling_user_id)
    {
        var rightThisSecond = DateOnly.FromDateTime(DateTime.Now);
        
        return CommonDataHelper<OrderHeader>.FillCommonFields(new OrderHeader()
        {
            customer_id = commandModel.customer_id,
            ship_to_address_id = commandModel.ship_to_address_id,
            shipping_method = commandModel.shipping_method,
            pay_method = commandModel.pay_method,
            opportunity_id = commandModel.opportunity_id,
            order_type = commandModel.order_type,
            order_date = rightThisSecond,
            required_date = commandModel.required_date,
            po_number = commandModel.po_number,
            shipping_cost = commandModel.shipping_cost,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public OrderLine MapToLineDatabaseModel(OrderLineCreateCommand commandModel, int order_id, string calling_user_id)
    {
        return CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine()
        {
            order_header_id = order_id,
            product_id = commandModel.product_id,
            line_number = commandModel.line_number,
            opportunity_line_id = commandModel.opportunity_line_id,
            line_description = commandModel.line_description,
            quantity = commandModel.quantity,
            unit_price = commandModel.unit_price,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public static async Task<OrderHeaderDto> StaticMapToDto(IBaseERPContext context, OrderHeader databaseModel)
    {
        var dto = new OrderHeaderDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            order_number = databaseModel.order_number,
            customer_id = databaseModel.customer_id,
            billing_address_id = databaseModel.billing_address_id,
            ship_to_address_id = databaseModel.ship_to_address_id,
            shipping_method = databaseModel.shipping_method,
            pay_method = databaseModel.pay_method,
            opportunity_id = databaseModel.opportunity_id,
            order_type = databaseModel.order_type,
            revision_number = databaseModel.revision_number,
            order_date = databaseModel.order_date,
            required_date = databaseModel.required_date,
            po_number = databaseModel.po_number,
            price = databaseModel.price,
            tax = databaseModel.tax,
            shipping_cost = databaseModel.shipping_cost,
            guid = databaseModel.guid,
            deleted_reason = databaseModel.deleted_reason,
            canceled_reason = databaseModel.canceled_reason,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        dto.customer_name = await context.Customers.Where(m => m.id == databaseModel.customer_id).Select(m => m.customer_name).SingleOrDefaultAsync();
        dto.created_by_name = await context.Users.Where(m => m.external_id == databaseModel.created_by).Select(m => m.first_name + " " + m.last_name).SingleOrDefaultAsync();


        return dto;
    }
    
    public async Task<OrderHeaderDto> MapToDto(OrderHeader databaseModel)
	{
        var dto = new OrderHeaderDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            order_number = databaseModel.order_number,
            customer_id = databaseModel.customer_id,
            billing_address_id = databaseModel.billing_address_id,
            ship_to_address_id = databaseModel.ship_to_address_id,
            shipping_method = databaseModel.shipping_method,
            pay_method = databaseModel.pay_method,
            opportunity_id = databaseModel.opportunity_id,
            order_type = databaseModel.order_type,
            revision_number = databaseModel.revision_number,
            order_date = databaseModel.order_date,
            required_date = databaseModel.required_date,
            po_number = databaseModel.po_number,
            price = databaseModel.price,
            tax = databaseModel.tax,
            shipping_cost = databaseModel.shipping_cost,
            guid = databaseModel.guid,
            deleted_reason = databaseModel.deleted_reason,
            canceled_reason = databaseModel.canceled_reason,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var payment_methods_val = await _KVMemoryService.GetKeyValue(databaseModel.pay_method);
        var shipment_methods_val = await _KVMemoryService.GetKeyValue(databaseModel.shipping_method);
        var customer_val = await _CustomerMemoryService.GetDatabaseValue(databaseModel.customer_id);

        if (payment_methods_val != null)
            dto.pay_method_name = payment_methods_val.value;

        if (customer_val != null)
            dto.customer_name = customer_val.customer_name;

        if (shipment_methods_val != null)
            dto.shipping_method_name = shipment_methods_val.value;

        var order_lines = await _Context.OrderLines.Where(m => m.order_header_id == databaseModel.id && m.is_deleted == false).ToListAsync();

        foreach (var line in order_lines)
            dto.order_lines.Add(await this.MapToLineDto(line));


        var ar_lines = await (from ah in _Context.ARInvoiceHeaders
                              join al in _Context.ARInvoiceLines on ah.id equals al.ar_invoice_header_id
                              where ah.order_header_id == databaseModel.id
                              && al.is_deleted == false
                              && ah.is_deleted == false
                              select al).ToListAsync();

        var ar_module = new ARInvoiceModule(_Context, _LogProviderFactory);
        foreach (var line in ar_lines)
            dto.ar_lines.Add(await ar_module.MapToLineDto(line));

        var shipping_address = await (from a in _Context.Addresses
                                        join ca in _Context.CustomerAddresses on a.id equals ca.address_id
                                        where ca.customer_id == databaseModel.customer_id 
                                        && ca.is_deleted == false
                                        && ca.address_type_id == CustomerAddressType.ShipTo
                                        select a).FirstOrDefaultAsync();

        var physical_address = await (from a in _Context.Addresses
                                        join ca in _Context.CustomerAddresses on a.id equals ca.address_id
                                        where ca.customer_id == databaseModel.customer_id 
                                        && ca.is_deleted == false
                                        && ca.address_type_id == CustomerAddressType.Physical
                                        select a).FirstOrDefaultAsync();

        if (shipping_address != null)
            dto.ship_to_address = await _AddressModule.MapToDto(shipping_address);


        return dto;
    }

    public async Task<OrderHeaderListDto> MapToListDto(OrderHeader databaseModel)
    {
        var dto = new OrderHeaderListDto
        {
            id = databaseModel.id,
            is_deleted = databaseModel.is_deleted,
            created_on = databaseModel.created_on,
            created_by = databaseModel.created_by,
            updated_on = databaseModel.updated_on,
            updated_by = databaseModel.updated_by,
            deleted_on = databaseModel.deleted_on,
            deleted_by = databaseModel.deleted_by,
            order_number = databaseModel.order_number,
            customer_id = databaseModel.customer_id,
            billing_address_id = databaseModel.billing_address_id,
            ship_to_address_id = databaseModel.ship_to_address_id,
            shipping_method = databaseModel.shipping_method,
            pay_method = databaseModel.pay_method,
            opportunity_id = databaseModel.opportunity_id,
            order_type = databaseModel.order_type,
            revision_number = databaseModel.revision_number,
            order_date = databaseModel.order_date,
            required_date = databaseModel.required_date,
            po_number = databaseModel.po_number,
            price = databaseModel.price,
            tax = databaseModel.tax,
            shipping_cost = databaseModel.shipping_cost,
            guid = databaseModel.guid,
            deleted_reason = databaseModel.deleted_reason,
            canceled_reason = databaseModel.canceled_reason,
            is_complete = databaseModel.is_complete,
            is_canceled = databaseModel.is_canceled,
            canceled_on = databaseModel.canceled_on,
            canceled_by = databaseModel.canceled_by,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };

        var customer_val = await _CustomerMemoryService.GetDatabaseValue(databaseModel.customer_id);

        if (customer_val != null)
            dto.customer_name = customer_val.customer_name;

        return dto;
    }

    public async Task<OrderLineDto> MapToLineDto(OrderLine databaseModel)
    {
        var dto = new OrderLineDto()
        {
            id = databaseModel.id,
            order_header_id = databaseModel.order_header_id,
            product_id = databaseModel.product_id,
            line_number = databaseModel.line_number,
            opportunity_line_id = databaseModel.opportunity_line_id,
            quantity = databaseModel.quantity,
            unit_price = databaseModel.unit_price,
            line_description = databaseModel.line_description,
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

        var product = await _Context.Products.Where(m => m.id == databaseModel.product_id).SingleOrDefaultAsync();

        if (product != null)
        {
            dto.product_name = product.product_name;
            dto.identifier1 = product.identifier1;
        }
        

        var attributes = await _Context.OrderLineAttributes.Where(m => m.order_line_id == databaseModel.id).ToListAsync();

        foreach (var attribute in attributes)
        {
            dto.attributes.Add(await this.MapToOrderLineAttribute(attribute));
        }

        dto.shipped_qty = await _Context.ShipmentLines.Where(m => m.order_line_id == databaseModel.id).SumAsync(m => m.units_shipped);

        return dto;
    }

    public async Task<OrderLineAttributeDto> MapToOrderLineAttribute(OrderLineAttribute databaseModel)
    {
        return new OrderLineAttributeDto()
        {
            id = databaseModel.id,
            order_line_id = databaseModel.order_line_id,
            attribute_name = databaseModel.attribute_name,
            attribute_value = databaseModel.attribute_value,
            attribute_value2 = databaseModel.attribute_value2,
            attribute_value3 = databaseModel.attribute_value3,
            guid = databaseModel.guid,
            created_by = databaseModel.created_by,
            created_on = databaseModel.created_on,
            deleted_by = databaseModel.deleted_by,
            deleted_on = databaseModel.deleted_on,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            is_deleted = databaseModel.is_deleted,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };
    }

    public OrderLine MapToLineDatabaseModel(OrderLineEditCommand commandModel, int order_id, string calling_user_id)
    {
        return CommonDataHelper<OrderLine>.FillCommonFields(new OrderLine()
        {
            order_header_id = order_id,
            product_id = commandModel.product_id.Value,
            line_number = commandModel.line_number.Value,
            opportunity_line_id = commandModel.opportunity_line_id,
            line_description = commandModel.line_description,
            quantity = commandModel.quantity.Value,
            unit_price = commandModel.unit_price.Value,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public OrderLineAttribute MapToAttributeDatabaseModel(OrderLineAttributeEditCommand commandModel, int order_line_id, string calling_user_id)
    {
        return CommonDataHelper<OrderLineAttribute>.FillCommonFields(new OrderLineAttribute()
        {
            order_line_id = order_line_id,
            attribute_name = commandModel.attribute_name,
            attribute_value = commandModel.attribute_value,
            attribute_value2 = commandModel.attribute_value2,
            attribute_value3 = commandModel.attribute_value3,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public OrderLineAttribute MapToAttributeDatabaseModel(OrderLineAttributeCreateCommand commandModel, string calling_user_id)
    {
        return CommonDataHelper<OrderLineAttribute>.FillCommonFields(new OrderLineAttribute()
        {
            order_line_id = commandModel.order_line_id.Value,
            attribute_name = commandModel.attribute_name,
            attribute_value = commandModel.attribute_value,
            attribute_value2 = commandModel.attribute_value2,
            attribute_value3 = commandModel.attribute_value3,
            guid = Guid.NewGuid().ToString(),
        }, calling_user_id);
    }

    public async Task<OrderLineAttributeDto> MapToAttributeDto(OrderLineAttribute databaseModel)
    {
        return new OrderLineAttributeDto()
        {
            id = databaseModel.id,
            order_line_id = databaseModel.order_line_id,
            attribute_name = databaseModel.attribute_name,
            attribute_value = databaseModel.attribute_value,
            attribute_value2 = databaseModel.attribute_value2,
            attribute_value3 = databaseModel.attribute_value3,
            guid = databaseModel.guid,
            created_by = databaseModel.created_by,
            created_on = databaseModel.created_on,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
            deleted_on_string = databaseModel.deleted_on_string,
            deleted_on_timezone = databaseModel.deleted_on_timezone,
        };
    }

    private async Task<int> ManuallyGenerateAnOrderNumber()
    {
        var total_records = await _Context.OrderHeaders.CountAsync();
        int start = DatabaseStartNumbers.Orders;

        return (total_records + start + 1);
    }
}