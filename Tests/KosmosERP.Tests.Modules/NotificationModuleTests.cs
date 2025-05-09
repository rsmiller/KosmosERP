using System.Data;
using Microsoft.EntityFrameworkCore;
using KosmosERP.Models;
using KosmosERP.Tests.Modules.Shared;
using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Database.Models;
using KosmosERP.BusinessLayer.Models.Module.Notification.Command.Find;
using KosmosERP.BusinessLayer.Models.Module.Notification.Dto;
using KosmosERP.Database;

namespace KosmosERP.Tests.Modules;

public class NotificationModuleTests
{
    private Customer _Customer;
    private Notification _Sample;
    private NotificationModule _Module;
    private ERPDbContext _Context;
    public string _SessionId = Guid.NewGuid().ToString();

    [SetUp]
    public async Task SetupModule()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .Options;

        _Context = new ERPDbContext(options);

        _Module = new NotificationModule(_Context);

        var customer = CommonDataHelper<Customer>.FillCommonFields(new Customer()
        {
            category = "CAT1",
            fax = "123-123-1234",
            phone = "234-456-2312",
            general_email = "customer@customer.com",
            is_deleted = false,
            customer_name = "Some customer",
            website = "google.com",
        }, 1);

        _Context.Customers.Add(customer);
        await _Context.SaveChangesAsync();

        _Customer = customer;

        var notification = CommonDataHelper<Notification>.FillCommonFields(new Notification()
        {
            object_id = _Customer.id,
            object_name = "customer",
            alert_text = "A new customer",
            user_id = 1,
        }, 1);

        _Context.Notifications.Add(notification);
        await _Context.SaveChangesAsync();

        _Sample = notification;
    }

    [TearDown]
    public void Destroy()
    {
        _Context.Dispose();
        _Module.Dispose();
    }

    [Test]
    public async Task Get()
    {
        var response = await _Module.GetDto(_Sample.id);

        Assert.IsTrue(response.Success);
        Assert.IsNotNull(response.Data);

        ValidateMostDtoFields(response.Data);
    }

    [Test]
    public async Task GetNotifications()
    {
        var results = await _Module.GetNotifications(
                        new PagingSortingParameters() { ResultCount = 20, Start = 0 },
                        new NotificationFindCommand() { calling_user_id = 1, token = _SessionId, user_id = 1 });
        
        Assert.IsTrue(results.Success);
        Assert.IsNotNull(results.Data);
        Assert.NotZero(results.Data.Count());

        var first_result = results.Data.First();

        ValidateMostDtoFields(first_result);
    }

    private void ValidateMostDtoFields(NotificationDto result)
    {
        Assert.NotZero(result.id);
        Assert.IsNotEmpty(result.guid);
        Assert.IsNotEmpty(result.object_name);
        Assert.IsNotEmpty(result.alert_text);
        Assert.NotZero(result.object_id);
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