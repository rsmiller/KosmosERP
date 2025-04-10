using Microsoft.EntityFrameworkCore;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Module;
using Prometheus.Models.Permissions;
using Prometheus.BusinessLayer.Interfaces;
using System.Text.Json;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Order.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Order.Dto;
using Prometheus.BusinessLayer.Models.Module.Transaction.Command.Create;
using Prometheus.BusinessLayer.Models.Module.Transaction.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.Transaction.Command.Delete;


namespace Prometheus.BusinessLayer.Modules;

public interface IOrderModule : IERPModule<OrderHeader, OrderHeaderDto, OrderHeaderListDto, OrderHeaderCreateCommand, OrderHeaderEditCommand, OrderHeaderDeleteCommand, OrderHeaderFindCommand>, IBaseERPModule
{
    Task<Response<OrderLineDto>> GetLineDto(int object_id);
    Task<Response<OrderLineDto>> CreateLine(OrderLineCreateCommand commandModel);
    Task<Response<OrderLineDto>> EditLine(OrderLineEditCommand commandModel);
    Task<Response<OrderLineDto>> DeleteLine(OrderLineDeleteCommand commandModel);
    Task<Response<OrderLineAttributeDto>> CreateAttribute(OrderLineAttributeCreateCommand commandModel);
    Task<Response<OrderLineAttributeDto>> EditAttribute(OrderLineAttributeEditCommand commandModel);
    Task<Response<OrderLineAttributeDto>> DeleteAttribute(OrderLineAttributeDeleteCommand commandModel);
}

public class OrderModule : BaseERPModule, IOrderModule
{
	public override Guid ModuleIdentifier => Guid.Parse("68c1862f-6043-4b09-8674-dd844a3a6fed");
	public override string ModuleName => "SalesOrder";

	private IBaseERPContext _Context;
    private IMessagePublisher _MessagePublisher;

    public OrderModule(IBaseERPContext context, IMessagePublisher messagePublisher) : base(context)
    {
        _Context = context;
        _MessagePublisher = messagePublisher;
    }

    
    public override void SeedPermissions()
	{
        var role = _Context.Roles.Any(m => m.name == "Sales Order Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == OrderPermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == OrderPermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == OrderPermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == OrderPermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(new Role()
            {
                name = "Sales Order Users",
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "Sales Order Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read Sales Order",
                internal_permission_name = OrderPermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == OrderPermissions.Read).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create Sales Order",
                internal_permission_name = OrderPermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == OrderPermissions.Create).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit Sales Order",
                internal_permission_name = OrderPermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == OrderPermissions.Edit).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete Sales Order",
                internal_permission_name = OrderPermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == OrderPermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, OrderPermissions.Create, write: true);
        if (!permission_result)
            return new Response<OrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            var alreadyExists = await this.OrderHeaderExists(commandModel);
            if (alreadyExists == true)
                return new Response<OrderHeaderDto>(ResultCode.AlreadyExists);

            var item = this.MapToDatabaseModel(commandModel, commandModel.calling_user_id);

            await _Context.OrderHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();


            // Now do lines
            foreach (var order_line in commandModel.order_lines)
            {
                var db_line = MapToLineDatabaseModel(order_line, item.id, commandModel.calling_user_id);

                await _Context.OrderLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();


                // Publish this data to a message queue to be processed for transactions
                await _MessagePublisher.PublishAsync(new Models.MessageObject()
                {
                    created_on = DateTime.Now,
                    object_type = "TransactionCreateCommand",
                    body = JsonSerializer.Serialize(new TransactionCreateCommand()
                    {
                        transaction_type = TransactionType.Reserved,
                        transaction_date = DateTime.Now,
                        object_reference_id = item.id,
                        object_sub_reference_id = item.id,
                        sold_unit_price = item.price,
                        units_sold = order_line.quantity,
                        product_id = order_line.product_id,
                        calling_user_id = commandModel.calling_user_id,
                    })
                }, RequiredMessageTopics.TransactionMovementTopic);
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,OrderPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<OrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OrderHeaderDto>("Order Header not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.OrderHeaders.Update(existingEntity);
        await _Context.SaveChangesAsync();


        await _MessagePublisher.PublishAsync(new Models.MessageObject()
        {
            created_on = DateTime.Now,
            object_type = "TransactionDeleteCommand",
            body = JsonSerializer.Serialize(new TransactionDeleteCommand()
            {
                object_reference_id = existingEntity.id,
                calling_user_id = commandModel.calling_user_id,
            })
        }, RequiredMessageTopics.TransactionMovementTopic);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,OrderPermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<OrderHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OrderHeaderDto>("Order Header not found", ResultCode.NotFound);

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
            if (commandModel.customer_id.HasValue && existingEntity.customer_id != commandModel.customer_id)
                existingEntity.customer_id = commandModel.customer_id.Value;
            if (commandModel.ship_to_address_id.HasValue && existingEntity.ship_to_address_id != commandModel.ship_to_address_id)
                existingEntity.customer_id = commandModel.ship_to_address_id.Value;
            if (commandModel.shipping_method_id.HasValue && existingEntity.shipping_method_id != commandModel.shipping_method_id)
                existingEntity.shipping_method_id = commandModel.shipping_method_id.Value;
            if (commandModel.pay_method_id.HasValue && existingEntity.pay_method_id != commandModel.pay_method_id)
                existingEntity.pay_method_id = commandModel.pay_method_id.Value;
            if (commandModel.opportunity_id.HasValue && existingEntity.opportunity_id != commandModel.opportunity_id)
                existingEntity.opportunity_id = commandModel.opportunity_id.Value;
            if(existingEntity.order_type != commandModel.order_type)
                existingEntity.order_type = commandModel.order_type;
            if(commandModel.order_date.HasValue && existingEntity.order_date != commandModel.order_date)
                existingEntity.order_date = commandModel.order_date.Value;
            if (commandModel.required_date.HasValue && existingEntity.required_date != commandModel.required_date)
                existingEntity.required_date = commandModel.required_date.Value;
            if (existingEntity.po_number != commandModel.po_number)
                existingEntity.po_number = commandModel.po_number;
            if (commandModel.tax.HasValue && existingEntity.tax != commandModel.tax)
                existingEntity.tax = commandModel.tax.Value;
            if (commandModel.shipping_cost.HasValue && existingEntity.shipping_cost != commandModel.shipping_cost)
                existingEntity.shipping_cost = commandModel.shipping_cost.Value;


            existingEntity.updated_on = DateTime.Now;
            existingEntity.updated_by = commandModel.calling_user_id;

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

                    // Publish this data to a message queue to be processed for transactions
                    await _MessagePublisher.PublishAsync(new Models.MessageObject()
                    {
                        created_on = DateTime.Now,
                        object_type = "TransactionCreateCommand",
                        body = JsonSerializer.Serialize(new TransactionCreateCommand()
                        {
                            transaction_type = TransactionType.Reserved,
                            transaction_date = DateTime.Now,
                            object_reference_id = add_line.order_id,
                            object_sub_reference_id = add_line.id,
                            sold_unit_price = add_line.unit_price,
                            units_sold = add_line.quantity,
                            product_id = add_line.product_id,
                            calling_user_id = commandModel.calling_user_id,
                        })
                    }, RequiredMessageTopics.TransactionMovementTopic);
                }
                else
                {
                    var edit_response = await this.EditLine(line);

                    if(!edit_response.Success)
                        return new Response<OrderHeaderDto>(edit_response.Exception, ResultCode.Error);


                    // Build attributes if there are any
                    if (line.attributes.Count > 0)
                    {
                        var attibute_response = await BuildAndEditLineAttributes(line, edit_response.Data.id, commandModel.calling_user_id);

                        if (!attibute_response.Success)
                            return new Response<OrderHeaderDto>(attibute_response.Exception, ResultCode.Error);
                    }
                }
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

    private async Task<Response<bool>> BuildAndEditLineAttributes(OrderLineEditCommand commandModel, int order_line_id, int calling_user_id)
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
            var query = _Context.OrderHeaders.Where(m => !m.is_deleted);

            if (!string.IsNullOrEmpty(commandModel.wildcard))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m => m.po_number.ToLower().Contains(wild));
            }

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m => m.order_number == parsed_num);
            }

            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

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

	public async Task<Response<List<OrderHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
	{
		throw new NotImplementedException();
	}

    public async Task<Response<OrderLineDto>> CreateLine(OrderLineCreateCommand commandModel)
    {
        if (commandModel == null)
            return new Response<OrderLineDto>(ResultCode.NullItemInput);

        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,OrderPermissions.Create, write: true);
        if (!permission_result)
            return new Response<OrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

        if (!commandModel.order_id.HasValue)
            return new Response<OrderLineDto>("Order Header os a required field", ResultCode.DataValidationError);

        try
        {
            var item = this.MapToLineDatabaseModel(commandModel, commandModel.order_id.Value, commandModel.calling_user_id);

            await _Context.OrderLines.AddAsync(item);
            await _Context.SaveChangesAsync();


            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.Now,
                object_type = "TransactionCreateCommand",
                body = JsonSerializer.Serialize(new TransactionCreateCommand()
                {
                    transaction_type = TransactionType.Reserved,
                    transaction_date = DateTime.Now,
                    object_reference_id = item.order_id,
                    object_sub_reference_id = item.id,
                    sold_unit_price = item.unit_price,
                    units_sold = item.quantity,
                    product_id = item.product_id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);


            var dto = await GetLineDto(item.id);

            return new Response<OrderLineDto>(dto.Data);
        }
        catch (Exception ex)
        {
            await LogError(80, this.GetType().Name, nameof(CreateLine), ex);
            return new Response<OrderLineDto>(ex.Message, ResultCode.Error);
        }
    }

    public async Task<Response<OrderLineDto>> EditLine(OrderLineEditCommand commandModel)
    {
        var validationResult = ModelValidationHelper.ValidateModel(commandModel);
        if (!validationResult.Success)
            return new Response<OrderLineDto>(validationResult.Exception, ResultCode.DataValidationError);

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,OrderPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<OrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

        if(!commandModel.id.HasValue)
            return new Response<OrderLineDto>("Order Line must have an id", ResultCode.DataValidationError);

        var existingEntity = await GetLineAsync(commandModel.id.Value);
        if (existingEntity == null)
            return new Response<OrderLineDto>("Order Line not found", ResultCode.NotFound);

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


            existingEntity.updated_on = DateTime.Now;
            existingEntity.updated_by = commandModel.calling_user_id;

            _Context.OrderLines.Update(existingEntity);
            await _Context.SaveChangesAsync();

            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.Now,
                object_type = "TransactionEditCommand",
                body = JsonSerializer.Serialize(new TransactionEditCommand()
                {
                    object_reference_id = existingEntity.order_id,
                    object_sub_reference_id = existingEntity.id,
                    sold_unit_price = existingEntity.unit_price,
                    units_sold = existingEntity.quantity,
                    product_id = existingEntity.product_id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);


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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, OrderPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<OrderLineDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetLineAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OrderLineDto>("Order Line not found", ResultCode.NotFound);

        try
        {
            existingEntity.is_deleted = true;
            existingEntity.deleted_on = DateTime.Now;
            existingEntity.deleted_by = commandModel.calling_user_id;

            _Context.OrderLines.Update(existingEntity);
            await _Context.SaveChangesAsync();


            // Corrolate the transacation data
            await _MessagePublisher.PublishAsync(new Models.MessageObject()
            {
                created_on = DateTime.Now,
                object_type = "TransactionDeleteCommand",
                body = JsonSerializer.Serialize(new TransactionDeleteCommand()
                {
                    object_reference_id = existingEntity.order_id,
                    object_sub_reference_id = existingEntity.id,
                    calling_user_id = commandModel.calling_user_id,
                })
            }, RequiredMessageTopics.TransactionMovementTopic);


            var dto = await MapToLineDto(existingEntity);
            return new Response<OrderLineDto>(dto);
        }
        catch(Exception ex)
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,OrderPermissions.Create, write: true);
        if (!permission_result)
            return new Response<OrderLineAttributeDto>("Invalid permission", ResultCode.InvalidPermission);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,OrderPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<OrderLineAttributeDto>("Invalid permission", ResultCode.InvalidPermission);

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

        existingEntity.updated_on = DateTime.Now;
        existingEntity.updated_by = commandModel.calling_user_id;

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token,OrderPermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<OrderLineAttributeDto>("Invalid permission", ResultCode.InvalidPermission);

        var existingEntity = await GetAttributeAsync(commandModel.id);
        if (existingEntity == null)
            return new Response<OrderLineAttributeDto>("Order Line not found", ResultCode.NotFound);

        existingEntity.is_deleted = true;
        existingEntity.deleted_on = DateTime.Now;
        existingEntity.deleted_by = commandModel.calling_user_id;

        _Context.OrderLineAttributes.Update(existingEntity);
        await _Context.SaveChangesAsync();

        var dto = await MapToAttributeDto(existingEntity);

        return new Response<OrderLineAttributeDto>(dto);
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

    public OrderHeader MapToDatabaseModel(OrderHeaderCreateCommand commandModel, int calling_user_id)
    {
        var now = DateTime.Now;

        return new OrderHeader()
        {
            customer_id = commandModel.customer_id,
            ship_to_address_id = commandModel.ship_to_address_id,
            shipping_method_id = commandModel.shipping_method_id,
            pay_method_id = commandModel.pay_method_id,
            opportunity_id = commandModel.opportunity_id,
            order_type = commandModel.order_type,
            revision_number = commandModel.revision_number,
            order_date = commandModel.order_date,
            required_date = commandModel.required_date,
            po_number = commandModel.po_number,
            tax = commandModel.tax,
            shipping_cost = commandModel.shipping_cost,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
    }

    public OrderLine MapToLineDatabaseModel(OrderLineCreateCommand commandModel, int order_id, int calling_user_id)
    {
        var now = DateTime.Now;

        return new OrderLine()
        {
            order_id = order_id,
            product_id = commandModel.product_id,
            line_number = commandModel.line_number,
            opportunity_line_id = commandModel.opportunity_line_id,
            line_description = commandModel.line_description,
            quantity = commandModel.quantity,
            unit_price = commandModel.unit_price,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
    }


    public async Task<OrderHeaderDto> MapToDto(OrderHeader databaseModel)
	{
        return new OrderHeaderDto
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
            ship_to_address_id = databaseModel.ship_to_address_id,
            shipping_method_id = databaseModel.shipping_method_id,
            pay_method_id = databaseModel.pay_method_id,
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
            canceled_by = databaseModel.canceled_by
        };
    }

	public async Task<OrderHeaderListDto> MapToListDto(OrderHeader databaseModel)
	{
        return new OrderHeaderListDto
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
            ship_to_address_id = databaseModel.ship_to_address_id,
            shipping_method_id = databaseModel.shipping_method_id,
            pay_method_id = databaseModel.pay_method_id,
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
            canceled_by = databaseModel.canceled_by
        };
    }

    private async Task<OrderLineDto> MapToLineDto(OrderLine databaseModel)
    {
        var dto = new OrderLineDto()
        {
            order_id = databaseModel.order_id,
            product_id = databaseModel.product_id,
            line_number = databaseModel.line_number,
            opportunity_line_id = databaseModel.opportunity_line_id,
            quantity = databaseModel.quantity,
            unit_price = databaseModel.unit_price,
            guid = databaseModel.guid
        };

        var attributes = await _Context.OrderLineAttributes.Where(m => m.order_line_id == databaseModel.id).ToListAsync();

        foreach (var attribute in attributes)
        {
            dto.attributes.Add(await this.MapToOrderLineAttribute(attribute));
        }

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
            is_deleted = databaseModel.is_deleted
        };
    }

    public OrderLine MapToLineDatabaseModel(OrderLineEditCommand commandModel, int order_id, int calling_user_id)
    {
        var now = DateTime.Now;

        return new OrderLine()
        {
            order_id = order_id,
            product_id = commandModel.product_id.Value,
            line_number = commandModel.line_number.Value,
            opportunity_line_id = commandModel.opportunity_line_id,
            line_description = commandModel.line_description,
            quantity = commandModel.quantity.Value,
            unit_price = commandModel.unit_price.Value,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
    }

    public OrderLineAttribute MapToAttributeDatabaseModel(OrderLineAttributeEditCommand commandModel, int order_line_id, int calling_user_id)
    {
        var now = DateTime.Now;
        return new OrderLineAttribute()
        {
            order_line_id = order_line_id,
            attribute_name = commandModel.attribute_name,
            attribute_value = commandModel.attribute_value,
            attribute_value2 = commandModel.attribute_value2,
            attribute_value3 = commandModel.attribute_value3,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
    }

    public OrderLineAttribute MapToAttributeDatabaseModel(OrderLineAttributeCreateCommand commandModel, int calling_user_id)
    {
        var now = DateTime.Now;
        return new OrderLineAttribute()
        {
            order_line_id = commandModel.order_line_id.Value,
            attribute_name = commandModel.attribute_name,
            attribute_value = commandModel.attribute_value,
            attribute_value2 = commandModel.attribute_value2,
            attribute_value3 = commandModel.attribute_value3,
            guid = Guid.NewGuid().ToString(),
            created_by = calling_user_id,
            created_on = now,
            updated_by = calling_user_id,
            updated_on = now,
        };
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
            updated_on = databaseModel.updated_on
        };
    }
}