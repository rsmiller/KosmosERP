
namespace KosmosERP.Models;

public class PagingResult<T>
{
    public bool Success { get; set; } = true;
    public ResultCode ResultCode { get; set; } = ResultCode.Okay;
    public string Exception { get; private set; } = "";
    public List<T> Data { get; set; } = new List<T>();
    public int TotalResultCount { get; set; } = 0;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;

    public PagingResult() { } 
    public PagingResult(string exception, ResultCode code)
    {
        this.SetException(exception, code);
    }
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
