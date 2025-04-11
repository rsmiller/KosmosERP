using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Prometheus.Database;
using Prometheus.Database.Models;
using Prometheus.Models;
using Prometheus.Models.Helpers;
using Prometheus.BusinessLayer.Models.Module.Notification.Command.Find;
using Prometheus.BusinessLayer.Models.Module.Notification.Dto;

namespace Prometheus.BusinessLayer.Modules;

public interface INotificationModule
{
    Task StreamNotification();
    Task<Response<NotificationDto>> GetDto(int object_id);
    Task<PagingResult<NotificationDto>> GetNotifications(PagingSortingParameters parameters, NotificationFindCommand commandModel);
    Task<NotificationDto> MapToDto(Notification databaseModel);
}

public class NotificationModule : Hub, INotificationModule
{
    private IBaseERPContext _Context;
    private static Dictionary<string, CancellationTokenSource> _ConnectedUsers = new Dictionary<string, CancellationTokenSource>();

    public NotificationModule(IBaseERPContext context)
    {
        _Context = context;
    }

    public async Task StreamNotification()
    {
        var userId = Context.ConnectionId;
        var cts = new CancellationTokenSource();
        _ConnectedUsers[userId] = cts;

        _ = Task.Run(() => PollNotifications(userId, cts.Token));
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        if (_ConnectedUsers.TryGetValue(Context.ConnectionId, out var cts))
        {
            cts.Cancel();
            _ConnectedUsers.Remove(Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    private async Task PollNotifications(string connectionId, CancellationToken token)
    {
        DateTime lastChecked = DateTime.UtcNow;

        while (!token.IsCancellationRequested)
        {
            var results = await GetNewResults(lastChecked);
            if (results.Any())
            {
                lastChecked = DateTime.UtcNow;
                await Clients.Client(connectionId).SendAsync("ReceiveResults", results);
            }

            await Task.Delay(60000, token); // Every minute
        }
    }

    private async Task<List<NotificationDto>> GetNewResults(DateTime last_execution)
    {
        List<NotificationDto> results = new List<NotificationDto>();

        var query = await _Context.Notifications
            .Where(m => m.created_on >= last_execution && m.is_deleted == false)
            .OrderByDescending(m => m.created_on).ToListAsync();

        foreach (var note in query)
            results.Add(await this.MapToDto(note));

        return results;
    }


    public async Task<Response<NotificationDto>> GetDto(int object_id)
    {
        Response<NotificationDto> response = new Response<NotificationDto>();

        var result = await _Context.Notifications.SingleOrDefaultAsync(m => m.id == object_id);
        if (result == null)
        {
            response.SetException("Notification not found", ResultCode.NotFound);
            return response;
        }

        response.Data = await this.MapToDto(result);
        return response;
    }

    public async Task<PagingResult<NotificationDto>> GetNotifications(PagingSortingParameters parameters, NotificationFindCommand commandModel)
    {
        PagingResult<NotificationDto> response = new PagingResult<NotificationDto>();

        try
        {

            var query = _Context.Notifications.Where(m => m.user_id == commandModel.user_id
                                                            && m.notification_read == commandModel.show_read
                                                            && m.is_deleted == false);


            var totalCount = await query.CountAsync();
            var pagedItems = await query.SortAndPageBy(parameters).ToListAsync();

            List<NotificationDto> notificationDtos = new List<NotificationDto>();

            foreach (var note in pagedItems)
                notificationDtos.Add(await this.MapToDto(note));

            response.Data = notificationDtos;

            return response;
        }
        catch (Exception ex)
        {
            //await LogError(50, this.GetType().Name, nameof(Find), ex);
            response.SetException(ex.Message, ResultCode.Error);
            response.TotalResultCount = 0;
        }

        return response;
    }



    public async Task<NotificationDto> MapToDto(Notification databaseModel)
    {
        return new NotificationDto
        {
            id = databaseModel.id,
            user_id = databaseModel.user_id,
            object_name = databaseModel.object_name,
            object_id = databaseModel.object_id,
            alert_text = databaseModel.alert_text,
            notified = databaseModel.notified,
            notification_read = databaseModel.notification_read,
            created_by = databaseModel.created_by,
            created_on = databaseModel.created_on,
            guid = databaseModel.guid,
            is_deleted = databaseModel.is_deleted,
            updated_by = databaseModel.updated_by,
            updated_on = databaseModel.updated_on,
            created_on_string = databaseModel.created_on_string,
            created_on_timezone = databaseModel.created_on_timezone,
            updated_on_string = databaseModel.updated_on_string,
            updated_on_timezone = databaseModel.updated_on_timezone,
        };
    }
}
