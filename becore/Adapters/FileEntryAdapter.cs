using Microsoft.AspNetCore.Http;
using Blazorise;

namespace becore.Adapters;

/// <summary>
/// Адаптер для конвертации Blazorise IFileEntry в IFormFile
/// </summary>
public class FileEntryAdapter : IFormFile
{
    private readonly IFileEntry _fileEntry;

    public FileEntryAdapter(IFileEntry fileEntry)
    {
        _fileEntry = fileEntry ?? throw new ArgumentNullException(nameof(fileEntry));
    }

    public string ContentType => _fileEntry.Type ?? "application/octet-stream";

    public string ContentDisposition => $"form-data; name=\"{Name}\"; filename=\"{FileName}\"";

    public IHeaderDictionary Headers => new HeaderDictionary();

    public long Length => _fileEntry.Size;

    public string Name => _fileEntry.Name;

    public string FileName => _fileEntry.Name;

    public void CopyTo(Stream target)
    {
        using var stream = _fileEntry.OpenReadStream();
        stream.CopyTo(target);
    }

    public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        using var stream = _fileEntry.OpenReadStream();
        await stream.CopyToAsync(target, cancellationToken);
    }

    public Stream OpenReadStream()
    {
        return _fileEntry.OpenReadStream();
    }
}
