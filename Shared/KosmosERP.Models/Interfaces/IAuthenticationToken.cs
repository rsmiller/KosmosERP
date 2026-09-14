namespace KosmosERP.Models.Interfaces;

public interface IAuthenticationToken
{
    public string access_token { get;  set;}
    public long expires_in { get;  set;}
}
