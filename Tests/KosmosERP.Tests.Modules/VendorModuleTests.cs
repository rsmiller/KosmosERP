using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Vendor.Dto;
using KosmosERP.BusinessLayer.Models.Module.Address.Command.Create;

namespace KosmosERP.Tests.Modules;

public class VendorModuleTests : BaseTestModule<VendorModule>, IModuleTest
{
    private Address _Address;
    private AddressModule _AddressModule;

    [SetUp]
    public async Task SetupModule()
    {
        _AddressModule = new AddressModule(base._Context);
        _AddressModule.SeedPermissions();

        var the_module = new VendorModule(base._Context, _AddressModule);

        await base.SetupModule(the_module);
    }

    [TearDown]
    public void TearDown()
    {
        _AddressModule.Dispose();
    }


    protected override async Task SetupRoles()
    {
        var admin_role = CommonDataHelper<Role>.FillCommonFields(new Role()
        {
            name = "Module Admin",
        }, 1);

        _Context.Roles.Add(admin_role);
        await _Context.SaveChangesAsync();

        var user_role = CommonDataHelper<UserRole>.FillCommonFields(new UserRole()
        {
            role_id = admin_role.id,
            user_id = _User.id,
        }, 1);

        _Context.UserRoles.Add(user_role);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupReadPermissions()
    {
        var read_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == VendorPermissions.Read);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = read_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupCreatePermissions()
    {
        var create_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == VendorPermissions.Create);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = create_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();


        var create_address_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _AddressModule.ModuleIdentifier.ToString() && m.internal_permission_name == AddressPermissions.Create);

        var another_role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = create_address_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(another_role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupEditPermissions()
    {
        var edit_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == VendorPermissions.Edit);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = edit_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupDeletePermissions()
    {
        var delete_permission = await _Context.ModulePermissions.FirstAsync(m => m.module_id == _Module.ModuleIdentifier.ToString() && m.internal_permission_name == VendorPermissions.Delete);
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_permission_id = delete_permission.id,
            role_id = role.id,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupData()
    {
        var address = CommonDataHelper<Address>.FillCommonFields(new Address
        {
            street_address1 = "11005 Chicken Nugget Lane",
            street_address2 = "Unit 12",
            city = "Temple",
            state = "TX",
            postal_code = "76251",
            country = "USA",
            is_deleted = false,
        }, 1);

        _Context.Addresses.Add(address);
        await _Context.SaveChangesAsync();

        _Address = address;
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new VendorCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            vendor_name = "Cool vendor",
            vendor_description = "The best vendor in the world",
            category = "General",
            fax = "123-456-7891",
            phone = "567-432-1234",
            general_email = "asd@email.com",
            website = "https://www.google.com",
            is_critial_vendor = true,
            address = new AddressCreateCommand()
            {
                street_address1 = "11005 Chicken Nugget Lane",
                street_address2 = "Unit 12",
                city = "Temple",
                state = "TX",
                postal_code = "76251",
                country = "USA",
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new VendorCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            vendor_name = "Cool vendor",
            vendor_description = "The best vendor in the world",
            category = "General",
            fax = "123-456-7891",
            phone = "567-432-1234",
            general_email = "asd@email.com",
            website = "https://www.google.com",
            is_critial_vendor = true,
            address = new AddressCreateCommand()
            {
                street_address1 = "11005 Chicken Nugget Lane",
                street_address2 = "Unit 12",
                city = "Temple",
                state = "TX",
                postal_code = "76251",
                country = "USA",
            }
        });

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new VendorCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            vendor_name = "Cool vendor",
            vendor_description = "The best vendor in the world",
            category = "General",
            fax = "123-456-7891",
            phone = "567-432-1234",
            general_email = "asd@email.com",
            website = "https://www.google.com",
            is_critial_vendor = true,
            address = new AddressCreateCommand()
            {
                street_address1 = "11005 Chicken Nugget Lane",
                street_address2 = "Unit 12",
                city = "Temple",
                state = "TX",
                postal_code = "76251",
                country = "USA",
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var edit_command = new VendorEditCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id,
            vendor_name = "Cool vendor21333",
            vendor_description = "The besdfsdfsdf",
            category = "General222",
            fax = "333-456-7891",
            phone = "445-432-1234",
            general_email = "asd@email.com",
            website = "https://www.googlesds.com",
            is_critial_vendor = false,
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.vendor_name == edit_command.vendor_name);
        Assert.That(edit_result.Data.vendor_description == edit_command.vendor_description);
        Assert.That(edit_result.Data.category == edit_command.category);
        Assert.That(edit_result.Data.fax == edit_command.fax);
        Assert.That(edit_result.Data.phone == edit_command.phone);
        Assert.That(edit_result.Data.general_email == edit_command.general_email);
        Assert.That(edit_result.Data.website == edit_command.website);
        Assert.That(edit_result.Data.is_critial_vendor == edit_command.is_critial_vendor);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new VendorCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            vendor_name = "Cool vendor",
            vendor_description = "The best vendor in the world",
            category = "General",
            fax = "123-456-7891",
            phone = "567-432-1234",
            general_email = "asd@email.com",
            website = "https://www.google.com",
            is_critial_vendor = true,
            address = new AddressCreateCommand()
            {
                street_address1 = "11005 Chicken Nugget Lane",
                street_address2 = "Unit 12",
                city = "Temple",
                state = "TX",
                postal_code = "76251",
                country = "USA",
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var delete_result = await _Module.Delete(new VendorDeleteCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            id = new_result.Data.id
        });

        Assert.IsTrue(delete_result.Success);
        Assert.IsNotNull(delete_result.Data);
        Assert.NotNull(delete_result.Data.deleted_by);
        Assert.NotNull(delete_result.Data.deleted_on);
        Assert.NotNull(delete_result.Data.deleted_on_string);
        Assert.NotNull(delete_result.Data.deleted_on_timezone);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new VendorCreateCommand()
        {
            calling_user_id = 1,
            token = _SessionId,
            vendor_name = "Cool vendor333",
            vendor_description = "The best vendor in the world",
            category = "General",
            fax = "123-456-7891",
            phone = "567-432-1234",
            general_email = "asd@email.com",
            website = "https://www.google.com",
            is_critial_vendor = true,
            address = new AddressCreateCommand()
            {
                street_address1 = "11005 Chicken Nugget Lane",
                street_address2 = "Unit 12",
                city = "Temple",
                state = "TX",
                postal_code = "76251",
                country = "USA",
            }
        });

        Assert.IsTrue(new_result.Success);
        Assert.IsNotNull(new_result.Data);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new VendorFindCommand() { calling_user_id = 1, token = _SessionId, wildcard = "vendor333" });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<VendorDto> result)
    {
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.NotZero(result.Data.id);
        Assert.IsNotEmpty(result.Data.guid);
        Assert.IsNotEmpty(result.Data.vendor_description);
        Assert.IsNotEmpty(result.Data.vendor_name);
        Assert.IsNotEmpty(result.Data.fax);
        Assert.IsNotEmpty(result.Data.phone);
        Assert.IsNotEmpty(result.Data.general_email);
        Assert.NotZero(result.Data.address_id);
        Assert.IsNotEmpty(result.Data.category);
        Assert.NotZero(result.Data.created_by);
        Assert.NotNull(result.Data.created_on);
        Assert.NotNull(result.Data.created_on_string);
        Assert.NotNull(result.Data.created_on_timezone);
        Assert.NotNull(result.Data.updated_by);
        Assert.NotNull(result.Data.updated_on);
        Assert.NotNull(result.Data.updated_on_string);
        Assert.NotNull(result.Data.updated_on_timezone);
    }

    private void ValidateMostListFields(VendorListDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.IsNotEmpty(result.vendor_description);
        Assert.IsNotEmpty(result.vendor_name);
        Assert.IsNotEmpty(result.fax);
        Assert.IsNotEmpty(result.phone);
        Assert.IsNotEmpty(result.general_email);
        Assert.NotZero(result.address_id);
        Assert.IsNotEmpty(result.category);
        Assert.NotZero(result.created_by);
        Assert.NotNull(result.created_on);
        Assert.NotNull(result.created_on_string);
        Assert.NotNull(result.created_on_timezone);
        Assert.NotNull(result.updated_by);
        Assert.NotNull(result.updated_on);
        Assert.NotNull(result.updated_on_string);
        Assert.NotNull(result.updated_on_timezone);
    }
}