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
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class VendorModuleTests : BaseTestModule<VendorModule>, IModuleTest
{
    private Address _Address;
    private AddressModule _AddressModule;

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        _AddressModule = new AddressModule(base._Context, logProviderFactory);
        _AddressModule.SeedPermissions();

        var the_module = new VendorModule(base._Context, _AddressModule, logProviderFactory);
        the_module.SeedPermissions();

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
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            read = true,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupCreatePermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            write = true,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();

        var another_role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _AddressModule.ModuleIdentifier.ToString(),
            role_id = role.id,
            write = true,
        }, 1);

        _Context.RolePermissions.Add(another_role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupEditPermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            edit = true,
        }, 1);

        _Context.RolePermissions.Add(role_module_permission);
        await _Context.SaveChangesAsync();
    }

    protected override async Task SetupDeletePermissions()
    {
        var role = await _Context.Roles.Where(m => m.name == "Module Admin").FirstAsync();

        var role_module_permission = CommonDataHelper<RolePermission>.FillCommonFields(new RolePermission()
        {
            module_id = _Module.ModuleIdentifier.ToString(),
            role_id = role.id,
            delete = true,
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
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var result = await _Module.Create(new VendorCreateCommand()
        {
            calling_user_id = _User.external_id,
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
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new VendorEditCommand()
        {
            calling_user_id = _User.external_id,
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
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new VendorDeleteCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id
        });

        Assert.That(delete_result.Success, Is.True);
        Assert.That(delete_result.Data, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_by, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_string, Is.Not.Null);
        Assert.That(delete_result.Data.deleted_on_timezone, Is.Not.Null);
    }

    [Test]
    public async Task Find()
    {
        var new_result = await _Module.Create(new VendorCreateCommand()
        {
            calling_user_id = _User.external_id,
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

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new VendorFindCommand() { calling_user_id = _User.external_id, wildcard = "vendor333" });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<VendorDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.vendor_description, Is.Not.Empty);
        Assert.That(result.Data.vendor_name, Is.Not.Empty);
        Assert.That(result.Data.fax, Is.Not.Empty);
        Assert.That(result.Data.phone, Is.Not.Empty);
        Assert.That(result.Data.general_email, Is.Not.Empty);
        Assert.That(result.Data.address_id, Is.Not.Zero);
        Assert.That(result.Data.category, Is.Not.Empty);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(VendorListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.vendor_description, Is.Not.Empty);
        Assert.That(result.vendor_name, Is.Not.Empty);
        Assert.That(result.fax, Is.Not.Empty);
        Assert.That(result.phone, Is.Not.Empty);
        Assert.That(result.general_email, Is.Not.Empty);
        Assert.That(result.address_id, Is.Not.Zero);
        Assert.That(result.category, Is.Not.Empty);
        Assert.That(result.created_by, Is.Not.Zero);
        Assert.That(result.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.created_on_string, Is.Not.Null);
        Assert.That(result.created_on_timezone, Is.Not.Null);
        Assert.That(result.updated_by, Is.Not.Null);
        Assert.That(result.updated_on, Is.Not.Null);
        Assert.That(result.updated_on_string, Is.Not.Null);
        Assert.That(result.updated_on_timezone, Is.Not.Null);
    }
}