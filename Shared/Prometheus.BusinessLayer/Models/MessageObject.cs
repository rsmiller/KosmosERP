using System.Text.Json;

namespace Prometheus.BusinessLayer.Models;

public class MessageObject
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public DateTime created_on { get; set; } = DateTime.Now;
    public string body { get; set; }


    public MessageObject()
    {
        body = "";
    }

    public MessageObject(object content)
    {
        body = JsonSerializer.Serialize(content);
    }
}
