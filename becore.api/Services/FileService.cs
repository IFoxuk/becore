using Amazon.S3;
using Amazon.S3.Model;
using becore.api.Models;
using becore.api.S3;
using becore.api.Scheme;
using Microsoft.Extensions.Options;

namespace becore.api.Services;

public class FileService(FileModel fileModel, IOptions<S3Options> options, IAmazonS3 s3Client, ApplicationContext context)
{
    private readonly FileModel _file = fileModel;
    private readonly S3Options _options = options.Value;
    private IAmazonS3 _s3Client = s3Client;
    private ApplicationContext _context = context; 

    public async Task<FileModel?> CreateAsync()
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
        var response = await _s3Client.PutObjectAsync(request);
        if (response.HttpStatusCode == System.Net.HttpStatusCode.OK) return _file;
        
        _context.Remove(_file.Entity);
        await _context.SaveChangesAsync();
        return null;
    }

    public async Task<FileModel?> GetAsync()
    {
        var file = await _context.Files.FindAsync(_file.Entity.Id);
        if (file == null) return null;

        var request = new GetObjectRequest
        {
            BucketName = _options.BucketName,
            Key = $"{file as DbEntity}"
        };
        var response = await _s3Client.GetObjectAsync(request);
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

    public async Task DeleteAsync()
    {
        var file = await _context.Files.FindAsync(_file.Entity.Id);
        if (file == null) return;

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = $"{file as DbEntity}"
        });
        
        _context.Files.Remove(file);
        await _context.SaveChangesAsync();
    }

    public async Task<FileModel?> UpdateAsync()
    {
        await DeleteAsync();
        return await CreateAsync();
    }
}