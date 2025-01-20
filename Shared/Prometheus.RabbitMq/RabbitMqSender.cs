using Prometheus.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Prometheus.RabbitMq;

public partial class RabbitMqSender
{
    private IConnectionFactory _ConnectionFactory;
    private IConnection _Connection;
    private IModel _Channel;
    private IBasicProperties _MessageProperties;
    private IRabbitMqConnection _RabbitMqConnectionSettings;


    private string _ThisQueue = "";
    private string _ThisExchange = "";
    private string _ThisRoutingKey = "";

    public RabbitMqSender(IRabbitMqConnection connection, string exchange, string queue, string routing_key)
    {
        _ThisQueue = queue;
        _ThisExchange = exchange;
        _ThisRoutingKey = routing_key;

        _RabbitMqConnectionSettings = connection;
        _ConnectionFactory = new ConnectionFactory() { HostName = connection.HostName, UserName = connection.UserName, Password = connection.Password, VirtualHost = connection.VirtualHost };
        _Connection = _ConnectionFactory.CreateConnection();
        _Channel = _Connection.CreateModel();
        _Channel.QueueBind(queue, _ThisExchange, _ThisRoutingKey);
        _MessageProperties = _Channel.CreateBasicProperties();
        _MessageProperties.Persistent = true;
    }

    public void CloseConnection()
    {
        if (_Channel.IsOpen)
            _Channel.Close();

        if (_Connection.IsOpen)
            _Connection.Close();
    }

    public bool Send<T>(T obj)
    {
        try
        {
            var json = JsonConvert.SerializeObject(obj);
            var message = Encoding.UTF8.GetBytes(json);

            _Channel.BasicPublish(_ThisExchange, _ThisRoutingKey, _MessageProperties, message);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public bool Send<T>(T obj, string routingKey)
    {
        try
        {
            var json = JsonConvert.SerializeObject(obj);
            var message = Encoding.UTF8.GetBytes(json);

            _Channel.BasicPublish(_ThisExchange, routingKey, _MessageProperties, message);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}
