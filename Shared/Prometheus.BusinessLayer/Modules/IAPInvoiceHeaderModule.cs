using Microsoft.EntityFrameworkCore;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Dto;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Create;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.APInvoiceHeader.Command.Find;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Helpers;
using Prometheus.Models.Interfaces;
using Prometheus.Module;

namespace Prometheus.Api.Modules
{
    public interface IAPInvoiceHeaderModule : IERPModule<APInvoiceHeader, APInvoiceHeaderDto, APInvoiceHeaderListDto, APInvoiceHeaderCreateCommand, APInvoiceHeaderEditCommand, APInvoiceHeaderDeleteCommand, APInvoiceHeaderFindCommand>, IBaseERPModule
    {
    }

    public class APInvoiceHeaderModule : BaseERPModule, IAPInvoiceHeaderModule
    {
        public override Guid ModuleIdentifier => Guid.Parse("ecf469ca-0a18-459b-954d-47de0bf23cf6");
        public override string ModuleName => "APInvoiceHeaders";

        private IBaseERPContext _Context;

        public APInvoiceHeaderModule(IBaseERPContext context) : base(context)
        {
            _Context = context;
        }

        public override void SeedPermissions()
        {
            var role = _Context.Roles.Any(m => m.name == "APInvoiceHeader Users");
            var read_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "read_apinvoiceheader");
            var create_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "create_apinvoiceheader");
            var edit_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "edit_apinvoiceheader");
            var delete_permission = _Context.ModulePermissions.Any(m => m.module_id == this.ModuleIdentifier.ToString() && m.internal_permission_name == "delete_apinvoiceheader");

            if (role == false)
            {
                _Context.Roles.Add(new Role()
                {
                    name = "APInvoiceHeader Users",
                    created_by = 1,
                    created_on = DateTime.Now,
                    updated_by = 1,
                    updated_on = DateTime.Now,
                });

                _Context.SaveChanges();
            }

            var role_id = _Context.Roles.Where(m => m.name == "APInvoiceHeader Users").Select(m => m.id).Single();

            if (read_permission == false)
            {
                _Context.ModulePermissions.Add(new ModulePermission()
                {
                    permission_name = "Read APInvoiceHeader",
                    internal_permission_name = "read_apinvoiceheader",
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    read = true,
                });

                _Context.SaveChanges();

                var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "read_apinvoiceheader").Select(m => m.id).Single();

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
                    permission_name = "Create APInvoiceHeader",
                    internal_permission_name = "create_apinvoiceheader",
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    write = true
                });

                _Context.SaveChanges();

                var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "create_apinvoiceheader").Select(m => m.id).Single();

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
                    permission_name = "Edit APInvoiceHeader",
                    internal_permission_name = "edit_apinvoiceheader",
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    edit = true
                });

                _Context.SaveChanges();

                var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "edit_apinvoiceheader").Select(m => m.id).Single();

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
                    permission_name = "Delete APInvoiceHeader",
                    internal_permission_name = "delete_apinvoiceheader",
                    module_id = this.ModuleIdentifier.ToString(),
                    module_name = this.ModuleName,
                    delete = true
                });

                _Context.SaveChanges();

                var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == "delete_apinvoiceheader").Select(m => m.id).Single();

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

        public APInvoiceHeader? Get(int object_id)
        {
            return _Context.APInvoiceHeaders.SingleOrDefault(m => m.id == object_id);
        }

        public async Task<APInvoiceHeader?> GetAsync(int object_id)
        {
            return await _Context.APInvoiceHeaders.SingleOrDefaultAsync(m => m.id == object_id);
        }

        public async Task<Response<APInvoiceHeaderDto>> GetDto(int object_id)
        {
            Response<APInvoiceHeaderDto> response = new Response<APInvoiceHeaderDto>();

            var result = await _Context.APInvoiceHeaders.SingleOrDefaultAsync(m => m.id == object_id);
            if (result == null)
            {
                response.SetException("APInvoiceHeader not found", ResultCode.NotFound);
                return response;
            }

            response.Data = await this.MapToDto(result);
            return response;
        }

        public async Task<Response<APInvoiceHeaderDto>> Create(APInvoiceHeaderCreateCommand commandModel)
        {
            if (commandModel == null)
                return new Response<APInvoiceHeaderDto>(ResultCode.NullItemInput);

            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<APInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

            var permission_result = await base.HasPermission(commandModel.calling_user_id, "create_apinvoiceheader", write: true);
            if (!permission_result)
                return new Response<APInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

            try
            {
                var alreadyExists = APInvoiceHeaderExists(commandModel);
                if (alreadyExists == true)
                    return new Response<APInvoiceHeaderDto>(ResultCode.AlreadyExists);

                var item = MapToDatabaseModel(commandModel);

                await _Context.APInvoiceHeaders.AddAsync(item);
                await _Context.SaveChangesAsync();

                var dto = await GetDto(item.id);

                return new Response<APInvoiceHeaderDto>(dto.Data);
            }
            catch (Exception ex)
            {
                return new Response<APInvoiceHeaderDto>(ex.Message, ResultCode.Error);
            }
        }

        public async Task<Response<APInvoiceHeaderDto>> Edit(APInvoiceHeaderEditCommand commandModel)
        {
            if (commandModel == null)
                return new Response<APInvoiceHeaderDto>(ResultCode.NullItemInput);

            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<APInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

            var permission_result = await base.HasPermission(commandModel.calling_user_id, "edit_apinvoiceheader", write: true);
            if (!permission_result)
                return new Response<APInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

            throw new NotImplementedException();
        }

        public async Task<Response<APInvoiceHeaderDto>> Delete(APInvoiceHeaderDeleteCommand commandModel)
        {
            var validationResult = ModelValidationHelper.ValidateModel(commandModel);
            if (!validationResult.Success)
                return new Response<APInvoiceHeaderDto>(validationResult.Exception, ResultCode.DataValidationError);

            var permission_result = await base.HasPermission(commandModel.calling_user_id, "delete_apinvoiceheader", delete: true);
            if (!permission_result)
                return new Response<APInvoiceHeaderDto>("Invalid permission", ResultCode.InvalidPermission);

            var existingEntity = await GetAsync(commandModel.id);
            if (existingEntity == null)
                return new Response<APInvoiceHeaderDto>("APInvoiceHeader not found", ResultCode.NotFound);

            existingEntity.is_deleted = true;
            existingEntity.deleted_on = DateTime.Now;
            existingEntity.deleted_by = commandModel.calling_user_id;

            _Context.APInvoiceHeaders.Update(existingEntity);
            await _Context.SaveChangesAsync();

            var dto = await MapToDto(existingEntity);
            return new Response<APInvoiceHeaderDto>(dto);
        }

        public async Task<PagingResult<APInvoiceHeaderListDto>> Find(PagingSortingParameters parameters, APInvoiceHeaderFindCommand commandModel)
        {
            throw new NotImplementedException();
        }

        public async Task<Response<List<APInvoiceHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
        {
            throw new NotImplementedException();
        }

        public async Task<APInvoiceHeaderListDto> MapToListDto(APInvoiceHeader databaseModel)
        {
            var dto = new APInvoiceHeaderListDto()
            {
                vendor_id = databaseModel.vendor_id,
                invoice_number = databaseModel.invoice_number,
                inv_date = databaseModel.inv_date,
                inv_due_date = databaseModel.inv_due_date,
                inv_received_date = databaseModel.inv_received_date,
                invoice_total = databaseModel.invoice_total,
                memo = databaseModel.memo,
                packing_list_is_required = databaseModel.packing_list_is_required,
                is_paid = databaseModel.is_paid,
                guid = databaseModel.guid,
                id = databaseModel.id,
                created_on = databaseModel.created_on,
                updated_on = databaseModel.updated_on,
                is_deleted = databaseModel.is_deleted
            };

            return dto;
        }

        public async Task<APInvoiceHeaderDto> MapToDto(APInvoiceHeader databaseModel)
        {
            var dto = new APInvoiceHeaderDto()
            {
                vendor_id = databaseModel.vendor_id,
                invoice_number = databaseModel.invoice_number,
                inv_date = databaseModel.inv_date,
                inv_due_date = databaseModel.inv_due_date,
                inv_received_date = databaseModel.inv_received_date,
                invoice_total = databaseModel.invoice_total,
                memo = databaseModel.memo,
                packing_list_is_required = databaseModel.packing_list_is_required,
                is_paid = databaseModel.is_paid,
                guid = databaseModel.guid,
                id = databaseModel.id,
                created_on = databaseModel.created_on,
                updated_on = databaseModel.updated_on,
                is_deleted = databaseModel.is_deleted
            };

            return dto;
        }

        public APInvoiceHeader MapToDatabaseModel(APInvoiceHeaderCreateCommand createCommand)
        {
            return new APInvoiceHeader()
            {
                vendor_id = createCommand.vendor_id,
                invoice_number = createCommand.invoice_number,
                inv_date = createCommand.inv_date,
                inv_due_date = createCommand.inv_due_date,
                inv_received_date = createCommand.inv_received_date,
                invoice_total = createCommand.invoice_total,
                memo = createCommand.memo,
                packing_list_is_required = createCommand.packing_list_is_required,
                is_paid = createCommand.is_paid,
                guid = createCommand.guid,
                created_on = DateTime.Now,
                updated_on = DateTime.Now,
                is_deleted = false
            };
        }

        private bool APInvoiceHeaderExists(APInvoiceHeaderCreateCommand createCommand)
        {
            return _Context.APInvoiceHeaders.Any(m => m.invoice_number == createCommand.invoice_number && m.vendor_id == createCommand.vendor_id);
        }

        public APInvoiceHeader MapToDatabaseModel(APInvoiceHeaderDto dtoModel)
        {
            throw new NotImplementedException();
        }
    }
}
