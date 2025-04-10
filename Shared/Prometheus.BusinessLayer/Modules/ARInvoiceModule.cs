using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Interfaces;
using Prometheus.Models.Permissions;
using Prometheus.Module;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Create;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Delete;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Edit;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Command.Find;
using Prometheus.BusinessLayer.Models.Module.ARInvoice.Dto;

namespace Prometheus.BusinessLayer.Modules;

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
            _Context.Roles.Add(new Role()
            {
                name = "AR Invoice Users",
                created_by = 1,
                created_on = DateTime.Now,
                updated_by = 1,
                updated_on = DateTime.Now,
            });

            _Context.SaveChanges();
        }

        var role_id = _Context.Roles.Where(m => m.name == "AR Invoice Users").Select(m => m.id).Single();

        if (read_permission == false)
        {
            _Context.ModulePermissions.Add(new ModulePermission()
            {
                permission_name = "Read AR Invoice",
                internal_permission_name = APInvoicePermissions.Read,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                read = true,
            });

            _Context.SaveChanges();

            var read_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ARInvoicePermissions.Read).Select(m => m.id).Single();

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
                permission_name = "Create AR Invoice",
                internal_permission_name = APInvoicePermissions.Create,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                write = true
            });

            _Context.SaveChanges();

            var create_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ARInvoicePermissions.Create).Select(m => m.id).Single();

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
                permission_name = "Edit AR Invoice",
                internal_permission_name = ARInvoicePermissions.Edit,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                edit = true
            });

            _Context.SaveChanges();

            var edit_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ARInvoicePermissions.Edit).Select(m => m.id).Single();

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
                permission_name = "Delete AR Invoice",
                internal_permission_name = ARInvoicePermissions.Delete,
                module_id = this.ModuleIdentifier.ToString(),
                module_name = this.ModuleName,
                delete = true
            });

            _Context.SaveChanges();

            var delete_perm_id = _Context.ModulePermissions.Where(m => m.internal_permission_name == ARInvoicePermissions.Delete).Select(m => m.id).Single();

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

    public ARInvoiceHeader? Get(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<ARInvoiceHeader?> GetAsync(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<ARInvoiceHeaderDto>> GetDto(int object_id)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<ARInvoiceLineDto>> GetLineDto(int object_id)
	{
        throw new NotImplementedException();
    }

    public async Task<Response<ARInvoiceHeaderDto>> Create(ARInvoiceHeaderCreateCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<ARInvoiceHeaderDto>> Delete(ARInvoiceHeaderDeleteCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<ARInvoiceHeaderDto>> Edit(ARInvoiceHeaderEditCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<PagingResult<ARInvoiceHeaderListDto>> Find(PagingSortingParameters parameters, ARInvoiceHeaderFindCommand commandModel)
	{
		throw new NotImplementedException();
	}

	public async Task<Response<List<ARInvoiceHeaderListDto>>> GlobalSearch(PagingSortingParameters parameters, string wildcard)
	{
		throw new NotImplementedException();
	}

    public async Task<Response<ARInvoiceLineDto>> CreateLine(ARInvoiceLineCreateCommand commandModel)
    {
        throw new NotImplementedException();
    }
    
	public async Task<Response<ARInvoiceLineDto>> EditLine(ARInvoiceLineEditCommand commandModel)
    {
        throw new NotImplementedException();
    }
    
	public async Task<Response<ARInvoiceLineDto>> DeleteLine(ARInvoiceLineDeleteCommand commandModel)
    {
        throw new NotImplementedException();
    }

    public ARInvoiceHeader MapToDatabaseModel(ARInvoiceHeaderDto dtoModel)
	{
		throw new NotImplementedException();
	}

	public Task<ARInvoiceHeaderDto> MapToDto(ARInvoiceHeader databaseModel)
	{
		throw new NotImplementedException();
	}

	public Task<ARInvoiceHeaderListDto> MapToListDto(ARInvoiceHeader databaseModel)
	{
		throw new NotImplementedException();
	}
}