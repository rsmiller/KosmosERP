using KosmosERP.Models;
using KosmosERP.Module;
using KosmosERP.Database;
using KosmosERP.Models.Helpers;
using KosmosERP.Database.Models;
using KosmosERP.Models.Interfaces;
using KosmosERP.Models.Permissions;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Dto;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.ARInvoice.Command.Find;
using System.Threading.Tasks;


namespace KosmosERP.BusinessLayer.Modules;

public interface IARInvoiceModule : IERPModule<ARInvoiceHeader, ARInvoiceHeaderDto, ARInvoiceHeaderListDto, ARInvoiceHeaderCreateCommand, ARInvoiceHeaderEditCommand, ARInvoiceHeaderDeleteCommand, ARInvoiceHeaderFindCommand>, IBaseERPModule
{
    Task<Response<ARInvoiceLineDto>> GetLineDto(int object_id);
    Task<Response<ARInvoiceLineDto>> CreateLine(ARInvoiceLineCreateCommand commandModel);
    Task<Response<ARInvoiceLineDto>> EditLine(ARInvoiceLineEditCommand commandModel);
    Task<Response<ARInvoiceLineDto>> DeleteLine(ARInvoiceLineDeleteCommand commandModel);
}

public class ARInvoiceModule : BaseERPModule, IARInvoiceModule
{
	public override Guid ModuleIdentifier => Guid.Parse("fab75a7f-af8c-4416-9e40-9c774aba1811");
	public override string ModuleName => "ARInvoice";

	private IBaseERPContext _Context;

	public ARInvoiceModule(IBaseERPContext context) : base(context)
    {
		_Context = context;
	}

	public override void SeedPermissions()
	{
        var role = _Context.Roles.Any(m => m.name == "AP Invoice Users");
        var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ARInvoicePermissions.Read);
        var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ARInvoicePermissions.Create);
        var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ARInvoicePermissions.Edit);
        var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == ARInvoicePermissions.Delete);

        if (role == false)
        {
            _Context.Roles.Add(CommonDataHelper<Role>.FillCommonFields(new Role()
            {
                name = "AR Invoice Users",
            }, 1));

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "AR Invoice Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read AR Invoice",
                internal_permission_name = ARInvoicePermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ARInvoicePermissions.Read).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = read_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (create_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Create AR Invoice",
                internal_permission_name = ARInvoicePermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ARInvoicePermissions.Create).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = create_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (edit_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Edit AR Invoice",
                internal_permission_name = ARInvoicePermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ARInvoicePermissions.Edit).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = edit_perm_id,
            }, 1));

            _Context.SaveChanges();
        }

        if (delete_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Delete AR Invoice",
                internal_permission_name = ARInvoicePermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ARInvoicePermissions.Delete).Select(m => m.id).Single();

            _Context.RolePermissions.Add(CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
            {
                role_id = role_id,
                module_permission_id = delete_perm_id,
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ARInvoicePermissions.Create, write: true);
        if (!permission_result)
            return new Response<ARInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

        try
        {
            var item = await MapToDatabaseModel(commandModel);

            await _Context.ARInvoiceHeaders.AddAsync(item);
            await _Context.SaveChangesAsync();

            // Check invoice number
            if(item.invoice_number == 0)
            {
                item.invoice_number = await this.ManuallyGenerateAnInvoiceNumber();

                _Context.ARInvoiceHeaders.Update(item);
                await _Context.SaveChangesAsync();
            }

            // Now do lines
            foreach (var ap_line in commandModel.ar_invoice_lines)
            {
                var db_line = await MapToLineDatabaseModel(ap_line, item.id, commandModel.calling_user_id);

                await _Context.ARInvoiceLines.AddAsync(db_line);
                await _Context.SaveChangesAsync();
            }


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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ARInvoicePermissions.Create, write: true);
        if (!permission_result)
            return new Response<ARInvoiceLineDto>("Invalid permission", ResultCode.InvalidPermission);

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ARInvoicePermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<ARInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

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

        if (commandModel.payment_terms.HasValue && existingEntity.payment_terms != commandModel.payment_terms)
            existingEntity.payment_terms = commandModel.payment_terms.Value;


        existingEntity.updated_on = DateTime.UtcNow;
        existingEntity.updated_by = commandModel.calling_user_id;
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ARInvoicePermissions.Edit, edit: true);
        if (!permission_result)
            return new Response<ARInvoiceLineDto>("Invalid permission", ResultCode.InvalidPermission);

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


        existingEntity.updated_on = DateTime.UtcNow;
        existingEntity.updated_by = commandModel.calling_user_id;

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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ARInvoicePermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<ARInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

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
                token = commandModel.token,
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

        var permission_result = await base.HasPermission(commandModel.calling_user_id, commandModel.token, ARInvoicePermissions.Delete, delete: true);
        if (!permission_result)
            return new Response<ARInvoiceLineDto>("Invalid permission", ResultCode.InvalidPermission);

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

            decimal parsed_num = 0;

            if (decimal.TryParse(commandModel.wildcard, out parsed_num))
            {
                var wild = commandModel.wildcard.ToLower();
                query = query.Where(m =>
                    m.invoice_number == parsed_num
                );
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

    public async Task<Response<List<ARInvoiceHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
    {
        throw new NotImplementedException();
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

        dto.customer_name = await _Context.Customers.Where(m => m.id == databaseModel.customer_id).Select(m => m.customer_name).SingleOrDefaultAsync();
        dto.order_number = await _Context.OrderHeaders.Where(m => m.id == databaseModel.order_header_id).Select(m => m.order_number).SingleOrDefaultAsync();
        
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
            deleted_on_timezone = databaseModel.deleted_on_timezone
        };

        dto.customer_name = await _Context.Customers.Where(m => m.id == databaseModel.customer_id).Select(m => m.customer_name).SingleOrDefaultAsync();
        dto.order_number = await _Context.OrderHeaders.Where(m => m.id == databaseModel.order_header_id).Select(m => m.order_number).SingleOrDefaultAsync();

        var lines = await _Context.ARInvoiceLines
            .Where(m => m.ar_invoice_header_id == databaseModel.id && !m.is_deleted)
            .ToListAsync();

        foreach (var ar_line in lines)
            dto.ar_invoice_lines.Add(await MapToLineDto(ar_line));


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

    public async Task<ARInvoiceLine> MapToLineDatabaseModel(ARInvoiceLineCreateCommand createCommand, int ar_invoice_header_id, int calling_user_id)
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


        var order_line = await _Context.OrderLines.Where(m => m.id == createCommand.order_line_id).SingleOrDefaultAsync();

        if (order_line == null)
            throw new Exception("Order line not found");

        var customer_tax = await (from c in _Context.Customers
                                  join o in _Context.OrderHeaders on c.id equals o.customer_id
                                  where c.id == order_line.order_header_id
                                  select new { c.is_taxable, c.tax_rate }).SingleOrDefaultAsync();

        if (customer_tax == null)
            throw new Exception("Customer tax information not found");


        // line_total
        line.line_total = order_line.unit_price * line.invoice_qty;

        // Order qty
        line.order_qty = order_line.quantity;

        // Tax
        if (line.is_taxable)
            line.line_tax = line.line_total * (customer_tax.tax_rate / 100);
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

    /// <summary>
    /// This method is used when the auto-gen field on Invoice Number fails. Some databases like memory databases don't auto-incremenet fields.
    /// </summary>
    /// <returns>Invoice Number</returns>
    private async Task<int> ManuallyGenerateAnInvoiceNumber()
    {
        var total_records = await _Context.ARInvoiceHeaders.CountAsync();
        return (total_records + 1);
    }
}