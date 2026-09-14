using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Models.Permissions;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Customer.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Customer.Dto;
using KosmosERP.BusinessLayer;
using Microsoft.Extensions.Caching.Memory;

namespace KosmosERP.Tests.Modules;

public class CustomerModuleTests : BaseTestModule<CustomerModule>, IModuleTest
{
    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var mem_cache = new MemoryCacheService<KeyValueStore>(new MemoryCache(new MemoryCacheOptions()), base._Context);
        var customer_cache = new MemoryCacheService<Customer>(new MemoryCache(new MemoryCacheOptions()), base._Context);
        
        var the_module = new CustomerModule(base._Context, mem_cache, customer_cache, logProviderFactory);

        await base.SetupModule(the_module);
    }

    [Test]
    public async Task Get()
    {
        var new_result = await _Module.Create(new CustomerCreateCommand()
        {
            calling_user_id = _User.external_id,
            customer_name = "Cool customer",
            customer_description = "Super cool customer guy",
            phone = "567-554-1234",
            fax = "777-555-2233",
            general_email = "TheEmail@email.com",
            website = "https://chickennuggets.com",
            accounting_email = "email@email.com",
            category = "Category1",
            is_taxable = false,
            tax_rate = 98,
            payment_terms = "payment_terms_net_15",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var result = await _Module.GetDto(new_result.Data.id);

        ValidateMostDtoFields(result);
    }

    [Test]
    public async Task Create()
    {
        var create_command = new CustomerCreateCommand()
        {
            calling_user_id = _User.external_id,
            customer_name = "Cool customer123123",
            customer_description = "Super cool customer guy123123",
            phone = "999-554-1234",
            fax = "111-555-2233",
            general_email = "TheEmail@email.com",
            website = "https://chickennuggets22.com",
            accounting_email = "email@email.com",
            category = "Category231",
            is_taxable = false,
            tax_rate = 91,
            payment_terms = "payment_terms_net_15",
        };

        var result = await _Module.Create(create_command);

        ValidateMostDtoFields(result);
        Assert.That(result.Data.customer_number, Is.Not.Zero);
        Assert.That(result.Data.customer_name == create_command.customer_name);
        Assert.That(result.Data.customer_description == create_command.customer_description);
        Assert.That(result.Data.phone == create_command.phone);
        Assert.That(result.Data.fax == create_command.fax);
        Assert.That(result.Data.general_email == create_command.general_email);
        Assert.That(result.Data.website == create_command.website);
        Assert.That(result.Data.category == create_command.category);
        Assert.That(result.Data.is_taxable == create_command.is_taxable);
        Assert.That(result.Data.tax_rate == create_command.tax_rate);
    }

    [Test]
    public async Task Edit()
    {
        var new_result = await _Module.Create(new CustomerCreateCommand()
        {
            calling_user_id = _User.external_id,
            customer_name = "Cool customer1777",
            customer_description = "Super cool customer guy1545",
            phone = "999-666-5655",
            fax = "111-234-333",
            general_email = "TheEmai1l@email.com",
            website = "https://chickennuggets29898.com",
            accounting_email = "email@email.com",
            category = "Category9",
            is_taxable = false,
            tax_rate = 42,
            payment_terms = "payment_terms_net_15",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var edit_command = new CustomerEditCommand()
        {
            calling_user_id = _User.external_id,
            id = new_result.Data.id,
            customer_name = "Cool customer000",
            customer_description = "Super cool customer guy000",
            phone = "999-666-0000",
            fax = "111-000-333",
            general_email = "TheEmail99@email.com",
            website = "https://chickennuggets20909.com",
            accounting_email = "email@email.com",
            category = "Category12",
            is_taxable = true,
            tax_rate = 11,
            payment_terms = "payment_terms_net_30",
        };

        var edit_result = await _Module.Edit(edit_command);

        ValidateMostDtoFields(edit_result);

        Assert.That(edit_result.Data.customer_name == edit_command.customer_name);
        Assert.That(edit_result.Data.customer_description == edit_command.customer_description);
        Assert.That(edit_result.Data.phone == edit_command.phone);
        Assert.That(edit_result.Data.fax == edit_command.fax);
        Assert.That(edit_result.Data.general_email == edit_command.general_email);
        Assert.That(edit_result.Data.website == edit_command.website);
        Assert.That(edit_result.Data.category == edit_command.category);
        Assert.That(edit_result.Data.is_taxable == edit_command.is_taxable);
        Assert.That(edit_result.Data.tax_rate == edit_command.tax_rate);
        Assert.That(edit_result.Data.payment_terms == edit_command.payment_terms);
    }

    [Test]
    public async Task Delete()
    {
        var new_result = await _Module.Create(new CustomerCreateCommand()
        {
            calling_user_id = _User.external_id,
            customer_name = "Cool customer000",
            customer_description = "Super cool customer guy000",
            phone = "999-666-0000",
            fax = "111-000-333",
            general_email = "TheEmail99@email.com",
            website = "https://chickennuggets20909.com",
            accounting_email = "email@email.com",
            category = "Category12",
            is_taxable = true,
            tax_rate = 11,
            payment_terms = "payment_terms_net_15",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var delete_result = await _Module.Delete(new CustomerDeleteCommand()
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
        var new_result = await _Module.Create(new CustomerCreateCommand()
        {
            calling_user_id = _User.external_id,
            customer_name = "Cool customer4322",
            customer_description = "Super cool customer guy3231",
            phone = "544-889-9778",
            fax = "234-555-333",
            general_email = "TheEmail99@email.com",
            website = "https://chickennuggets289.com",
            accounting_email = "email@email.com",
            category = "Category2",
            is_taxable = true,
            tax_rate = 23,
            payment_terms = "payment_terms_net_15",
        });

        Assert.That(new_result.Success, Is.True);
        Assert.That(new_result.Data, Is.Not.Null);

        var results = await _Module.Find(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new CustomerFindCommand() { calling_user_id = _User.external_id, wildcard = "customer4322" });
        
        Assert.That(results.Success, Is.True);
        Assert.That(results.Data, Is.Not.Null);
        Assert.That(results.Data.Count(), Is.Not.Zero);

        var first_result = results.Data.First();

        ValidateMostListFields(first_result);
    }

    private void ValidateMostDtoFields(Response<CustomerDto> result)
    {
        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.id, Is.Not.Zero);
        Assert.That(result.Data.guid, Is.Not.Empty);
        Assert.That(result.Data.customer_number, Is.Not.Zero);
        Assert.That(result.Data.customer_name, Is.Not.Empty);
        Assert.That(result.Data.customer_description, Is.Not.Empty);
        Assert.That(result.Data.phone, Is.Not.Empty);
        Assert.That(result.Data.fax, Is.Not.Empty);
        Assert.That(result.Data.general_email, Is.Not.Empty);
        Assert.That(result.Data.website, Is.Not.Empty);
        Assert.That(result.Data.category, Is.Not.Empty);
        Assert.That(result.Data.payment_terms, Is.Not.Empty);
        Assert.That(result.Data.created_by, Is.Not.Zero);
        Assert.That(result.Data.created_on, Is.GreaterThan(DateTime.MinValue));
        Assert.That(result.Data.created_on_string, Is.Not.Null);
        Assert.That(result.Data.created_on_timezone, Is.Not.Null);
        Assert.That(result.Data.updated_by, Is.Not.Null);
        Assert.That(result.Data.updated_on, Is.Not.Null);
        Assert.That(result.Data.updated_on_string, Is.Not.Null);
        Assert.That(result.Data.updated_on_timezone, Is.Not.Null);
    }

    private void ValidateMostListFields(CustomerListDto result)
    {
        Assert.That(result.id, Is.Not.Zero);
        Assert.That(result.guid, Is.Not.Empty);
        Assert.That(result.customer_number, Is.Not.Zero);
        Assert.That(result.customer_name, Is.Not.Empty);
        Assert.That(result.customer_description, Is.Not.Empty);
        Assert.That(result.phone, Is.Not.Empty);
        Assert.That(result.fax, Is.Not.Empty);
        Assert.That(result.general_email, Is.Not.Empty);
        Assert.That(result.payment_terms, Is.Not.Empty);
        Assert.That(result.website, Is.Not.Empty);
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