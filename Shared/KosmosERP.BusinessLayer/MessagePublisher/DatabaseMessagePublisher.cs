using KosmosERP.BusinessLayer.Helpers;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.BusinessLayer.Models;
using KosmosERP.Database;
using KosmosERP.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KosmosERP.BusinessLayer.MessagePublisher;

public class DatabaseMessagePublisher : IMessagePublisher, IAsyncDisposable
{

    private IBaseERPContext _Context;

    public DatabaseMessagePublisher(IBaseERPContext context)
    {
        _Context = context;
    }

    public async Task<bool> PublishAsync(MessageObject message, string topic_or_queue)
    {
        if(_Context == null)
            throw new Exception("Database context is null in DatabaseMessagePublisher");

        try
        {
            var entry = CommonDataHelper<MessageQueue>.FillCommonFields(new MessageQueue
            {
                queue = topic_or_queue,
                body = JsonSerializer.Serialize(message),
            }, 1);

            await _Context.MessageQueues.AddAsync(entry);
            await _Context.SaveChangesAsync();

            return true;
        }
        catch(Exception ex)
        {
            throw new Exception("Error publishing message to database", ex);
        }
    }

    public async Task<string?> GetNextMessage(string topic_or_queue)
    {
        var entry = await _Context.MessageQueues.Where(m => m.read == false).OrderBy(m => m.created_on).FirstOrDefaultAsync();
        
        if(entry != null)
        {
            entry.read = true;
            entry.read_on = DateTime.UtcNow;

            _Context.MessageQueues.Update(entry);
            await _Context.SaveChangesAsync();

            return entry.body;
        }
        else
        {
            return null;
        }
    }

    public async Task CloseConnection()
    {
        
    }

    public async ValueTask DisposeAsync()
    {
        
    }
}
