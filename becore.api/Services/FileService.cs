using Amazon.S3;
using Amazon.S3.Model;
using becore.api.Models;
using becore.api.S3;
using becore.api.Scheme;
using Microsoft.Extensions.Options;

namespace becore.api.Services;

public class FileService(IOptions<S3Options> options, IAmazonS3 s3Client, ApplicationContext context)
{
    private FileModel _file = null!;
    private readonly S3Options _options = options.Value;
    private IAmazonS3 _s3Client = s3Client;
    private ApplicationContext _context = context;

    public Task<FileModel?> CreateAsync(FileModel fileModel)
    {
        _file = fileModel;
        return CreateAsync();
    }
    
    private async Task<FileModel?> CreateAsync()
    {
        await _context.Files.AddAsync(_file.Entity);
        var saveResult = _context.SaveChangesAsync();
        await Task.Run(() => saveResult);
        if (!saveResult.IsCompleted) return null;

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = $"{_file}",
            ContentType = _file.Entity.Type,
            InputStream = _file.Data
        };
        var response = await _s3Client.PutObjectAsync(request); //TODO: Нэ бачит
        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK) return _file;
        
        _context.Remove(_file.Entity);
        await _context.SaveChangesAsync();
        return null;
    }

    public async Task<FileModel?> GetAsync(Guid id)
    {
        var file = await _context.Files.FindAsync(id);
        if (file == null) return null;

        var request = new GetObjectRequest
        {
            BucketName = _options.BucketName,
            Key = $"{file as DbEntity}"
        };
        var response = await _s3Client.GetObjectAsync(request); //TODO: Нэ бачит
        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            _file.Entity = file;
            _file.Data = response.ResponseStream;
            return _file;
        }
        
        _context.Remove(_file.Entity);
        await _context.SaveChangesAsync();
        return null;
    }

    public async Task DeleteAsync(Guid id)
    {
        var file = await _context.Files.FindAsync(id);
        if (file == null) return;

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest //TODO: Нэ бачит
        {
            BucketName = _options.BucketName,
            Key = $"{file as DbEntity}"
        });
        
        _context.Files.Remove(file);
        await _context.SaveChangesAsync();
    }

    public Task<FileModel?> UpdateAsync(FileModel fileModel)
    {
        _file = fileModel;
        return UpdateAsync();
    }
    
    private async Task<FileModel?> UpdateAsync()
    {
        await DeleteAsync(_file.Entity.Id);
        return await CreateAsync();
    }
}