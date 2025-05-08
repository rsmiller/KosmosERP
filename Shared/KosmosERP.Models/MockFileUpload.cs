using Microsoft.AspNetCore.Http;

namespace KosmosERP.Models;

public class MockFileUpload : IFormFile
{
    public string ContentType { get; set; }

    public string ContentDisposition { get; set; }

    public IHeaderDictionary Headers { get; set; }

    public long Length { get; set; }

    public string Name { get; set; }

    public string FileName {  get; set; }

    public void CopyTo(Stream target)
    {

    }

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Stream OpenReadStream()
    {
        return new MemoryStream();
    }
}
