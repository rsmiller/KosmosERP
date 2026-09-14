using System.Text;

namespace KosmosERP.Models;

public class Response<T>
{
    public Response()
    {

    }

    public Response(T obj)
    {
        this.Data = obj;
    }

    public Response(ResultCode code)
    {
        Success = false;

        switch (code)
        {
            case ResultCode.AlreadyExists:
                Exception = "Already Exists";
                break;
            case ResultCode.DataValidationError:
                Exception = "Data Validation Error";
                break;
            case ResultCode.Error:
                Exception = "Error";
                break;
            case ResultCode.Invalid:
                Exception = "Invalid";
                break;
            case ResultCode.None:
                Exception = "None";
                break;
            case ResultCode.NotFound:
                Exception = "Not Found";
                break;
            case ResultCode.NullItemInput:
                Exception = "Null";
                break;
            case ResultCode.InvalidPermission:
                Exception = "Invalid Permission";
                break;
            default:
                Exception = "Gernal Error";
                break;
        }
    }

    public Response(string exception, ResultCode code)
    {
        this.SetException(exception, code);
    }

    public Response(List<string> exceptions, ResultCode code)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var ex in exceptions)
            sb.AppendLine(ex);

        this.SetException(sb.ToString(), code);
    }

    public bool Success { get; set; } = true;
    public ResultCode ResultCode { get; set; } = ResultCode.Okay;
    public string Exception { get; private set; } = "";
    public T? Data { get; set; }

    public void SetException(Exception e)
    {
        Exception = e.Message;
        Success = false;
    }

    public void SetException(string e, ResultCode code)
    {
        Exception = e;
        Success = false;
        this.ResultCode = code;
    }
}
