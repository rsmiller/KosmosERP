using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Create;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Edit;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Delete;
using KosmosERP.BusinessLayer.Models.Module.Activity.Command.Find;
using KosmosERP.BusinessLayer;

namespace KosmosERP.Tests.Modules;

public class ActivityModuleTests : BaseTestModule<ActivityModule>, IModuleTest
{
    private User _Owner;
    private Customer _Customer;

    [SetUp]
    public async Task SetupModule()
    {
        var logProviderFactory = new LogProviderFactory(new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
        var the_module = new ActivityModule(base._Context, logProviderFactory);

        await base.SetupModule(the_module);
    }

    protected override async Task SetupTestData()
    {
        // Create test owner user
        _Owner = CommonDataHelper<User>.FillCommonFields(new User()
        {
            first_name = "Test",
            last_name = "Owner",
            email = "testowner@test.com",
            username = "testowner",
            password = "password123",
            password_salt = "salt123",
            employee_number = "10002",
            department = "1",
            guid = Guid.NewGuid().ToString(),
            external_id = Guid.NewGuid().ToString(),
            is_external_user = false,
            is_admin = false,
            is_management = false,
            is_guest = false,
            is_deleted = false,
        }, 1);

        _Context.Users.Add(_Owner);
        await _Context.SaveChangesAsync();

        // Create test customer
        _Customer = CommonDataHelper<Customer>.FillCommonFields(new Customer()
        {
            customer_name = "Test Customer",
            phone = "555-1234",
            category = "Business",
            payment_terms = "Net 30",
            is_taxable = true,
            tax_rate = 0.08m,
            is_deleted = false,
        }, 1);

        _Context.Customers.Add(_Customer);
        await _Context.SaveChangesAsync();
    }

    [Test]
    public async Task Get()
    {
        // First create an activity
        var createCommand = new ActivityCreateCommand()
        {
            subject = "Test Activity for Get",
            description = "This is a test activity for get",
            activity_type = "Meeting",
            status = "Planned",
            owner_id = _Owner.id,
            start_date = DateTime.UtcNow.AddDays(1),
            priority = 1,
            calling_user_id = _User.external_id,
        };

        var createResult = await _Module.Create(createCommand);
        Assert.That(createResult.Success, Is.True);

        // Now get it
        var getResult = await _Module.GetDto(createResult.Data.id);
        Assert.That(getResult.Success, Is.True);
        Assert.That(getResult.Data.subject, Is.EqualTo("Test Activity for Get"));
        Assert.That(getResult.Data.activity_type, Is.EqualTo("Meeting"));
    }

    [Test]
    public async Task Create()
    {
        var createCommand = new ActivityCreateCommand()
        {
            subject = "Test Activity",
            description = "This is a test activity",
            activity_type = "Task",
            status = "Planned",
            owner_id = _Owner.id,
            start_date = DateTime.UtcNow.AddDays(1),
            priority = 2,
            customer_id = _Customer.id,
            calling_user_id = _User.external_id,
        };

        var result = await _Module.Create(createCommand);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.subject, Is.EqualTo("Test Activity"));
        Assert.That(result.Data.activity_type, Is.EqualTo("Task"));
        Assert.That(result.Data.owner_id, Is.EqualTo(_Owner.id));
        Assert.That(result.Data.customer_id, Is.EqualTo(_Customer.id));
    }

    [Test]
    public async Task Edit()
    {
        // First create an activity
        var createCommand = new ActivityCreateCommand()
        {
            subject = "Test Activity for Edit",
            description = "This is a test activity for edit",
            activity_type = "Call",
            status = "Planned",
            owner_id = _Owner.id,
            start_date = DateTime.UtcNow.AddDays(1),
            priority = 2,
            calling_user_id = _User.external_id,
        };

        var createResult = await _Module.Create(createCommand);
        Assert.That(createResult.Success, Is.True);

        // Now edit it
        var editCommand = new ActivityEditCommand()
        {
            id = createResult.Data.id,
            subject = "Updated Activity Subject",
            description = "Updated description",
            activity_type = "Meeting",
            status = "Completed",
            priority = 1,
            calling_user_id = _User.external_id,
        };

        var editResult = await _Module.Edit(editCommand);
        Assert.That(editResult.Success, Is.True);
        Assert.That(editResult.Data.subject, Is.EqualTo("Updated Activity Subject"));
        Assert.That(editResult.Data.status, Is.EqualTo("Completed"));
    }

    [Test]
    public async Task Delete()
    {
        // First create an activity
        var createCommand = new ActivityCreateCommand()
        {
            subject = "Test Activity for Delete",
            description = "This is a test activity for delete",
            activity_type = "Task",
            status = "Planned",
            owner_id = _Owner.id,
            start_date = DateTime.UtcNow.AddDays(1),
            priority = 2,
            calling_user_id = _User.external_id,
        };

        var createResult = await _Module.Create(createCommand);
        Assert.That(createResult.Success, Is.True);

        // Now delete it
        var deleteCommand = new ActivityDeleteCommand()
        {
            id = createResult.Data.id,
            calling_user_id = _User.external_id,
        };

        var deleteResult = await _Module.Delete(deleteCommand);
        Assert.That(deleteResult.Success, Is.True);

        // Verify it's deleted
        var getResult = await _Module.GetDto(createResult.Data.id);
        Assert.That(getResult.Success, Is.True);
        Assert.That(getResult.Data.is_deleted, Is.True);
        
    }

    [Test]
    public async Task Find()
    {
        // Create multiple activities
        var createCommand1 = new ActivityCreateCommand()
        {
            subject = "Task Activity",
            description = "Task description",
            activity_type = "Task",
            status = "Planned",
            owner_id = _Owner.id,
            start_date = DateTime.UtcNow.AddDays(1),
            priority = 2,
            calling_user_id = _User.external_id,
        };

        var createCommand2 = new ActivityCreateCommand()
        {
            subject = "Meeting Activity",
            description = "Meeting description",
            activity_type = "Meeting",
            status = "Planned",
            owner_id = _Owner.id,
            start_date = DateTime.UtcNow.AddDays(2),
            priority = 3,
            calling_user_id = _User.external_id,
        };

        await _Module.Create(createCommand1);
        await _Module.Create(createCommand2);

        // Find only Task activities
        var findCommand = new ActivityFindCommand()
        {
            activity_type = "Task",
            calling_user_id = _User.external_id,
        };

        var findResult = await _Module.Find(new PagingSortingParameters(0, 10, "id"), findCommand);
        Assert.That(findResult.Success, Is.True);
        Assert.That(findResult.Data.All(a => a.activity_type == "Task"), Is.True);
    }

} 