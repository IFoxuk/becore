using System.Net;
using System.Security.Cryptography;
using Amazon.S3;
using Amazon.S3.Model;
using becore.api.Models;
using becore.api.S3;
using becore.api.Scheme;
using becore.api.Services.Interfaces;
using Microsoft.Extensions.Options;
using File = becore.api.Scheme.System.File;

namespace becore.api.Services;

public class FileS3Service(IOptions<S3Options> options, IAmazonS3 s3Client, ApplicationContext context, ILogger<FileS3Service> logger) : IFileS3Service
{
    private readonly S3Options _options = options.Value;
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly ApplicationContext _context = context;
    private readonly ILogger<FileS3Service> _logger = logger;

    public async Task<FileModel?> CreateAsync(FileModel fileModel)
    {
        if (fileModel?.Entity == null || fileModel.Data == null)
        {
            _logger.LogError("FileModel or its properties are null");
            return null;
        }

        try
        {
            // Read the stream into memory to avoid stream positioning issues
            using var sourceStream = fileModel.Data;
            var memoryStream = new MemoryStream();
            
            await sourceStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            // Store file size
            fileModel.Entity.Size = memoryStream.Length;
            
            // Save to database first
            await _context.Files.AddAsync(fileModel.Entity);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("File entity saved to database with ID: {FileId}, Size: {Size} bytes", 
                fileModel.Entity.Id, fileModel.Entity.Size);

            // Reset stream position for S3 upload
            memoryStream.Position = 0;
            
            // Upload to S3 with proper configuration
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = GenerateS3Key(fileModel.Entity),
                ContentType = fileModel.Entity.Type ?? "application/octet-stream",
                InputStream = memoryStream,
                UseChunkEncoding = false,
                DisablePayloadSigning = false
            };
            
            var response = await _s3Client.PutObjectAsync(request);
            
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("File uploaded to S3 successfully with key: {S3Key}, ETag: {ETag}", 
                    request.Key, response.ETag);
                    
                // Return a new stream for output
                var outputStream = new MemoryStream(memoryStream.ToArray());
                outputStream.Position = 0;
                
                return new FileModel
                {
                    Entity = fileModel.Entity,
                    Data = outputStream
                };
            }
            
            _logger.LogError("Failed to upload file to S3. Status code: {StatusCode}", response.HttpStatusCode);
            
            // Rollback database changes if S3 upload failed
            _context.Remove(fileModel.Entity);
            await _context.SaveChangesAsync();
            
            // Dispose the memory stream since we're not returning it
            memoryStream.Dispose();
            return null;
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "S3 error occurred while creating file: {ErrorCode}, Message: {Message}", 
                s3Ex.ErrorCode, s3Ex.Message);
            
            // Try to rollback database changes
            try
            {
                var existingEntity = await _context.Files.FindAsync(fileModel.Entity.Id);
                if (existingEntity != null)
                {
                    _context.Remove(existingEntity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to rollback database changes after S3 error");
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating file");
            return null;
        }
    }

    public async Task<FileModel?> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogError("Invalid file ID provided: {FileId}", id);
            return null;
        }

        try
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                _logger.LogWarning("File not found in database: {FileId}", id);
                return null;
            }

            var request = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = GenerateS3Key(file)
            };
            
            var response = await _s3Client.GetObjectAsync(request);
            
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("File retrieved successfully from S3: {FileId}", id);
                return new FileModel
                {
                    Entity = file,
                    Data = response.ResponseStream
                };
            }
            
            _logger.LogWarning("File not found in S3, removing from database: {FileId}", id);
            
            // Remove orphaned database record if file doesn't exist in S3
            _context.Remove(file);
            await _context.SaveChangesAsync();
            
            return null;
        }
        catch (AmazonS3Exception s3Ex) when (s3Ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File not found in S3: {FileId}, Error: {ErrorCode}", id, s3Ex.ErrorCode);
            
            // Remove orphaned database record
            try
            {
                var file = await _context.Files.FindAsync(id);
                if (file != null)
                {
                    _context.Remove(file);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to remove orphaned database record: {FileId}", id);
            }
            
            return null;
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "S3 error occurred while retrieving file: {FileId}, ErrorCode: {ErrorCode}", id, s3Ex.ErrorCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving file: {FileId}", id);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogError("Invalid file ID provided for deletion: {FileId}", id);
            return false;
        }

        try
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                _logger.LogWarning("File not found in database for deletion: {FileId}", id);
                return false;
            }

            // Try to delete from S3 first
            try
            {
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = GenerateS3Key(file)
                });
                
                _logger.LogInformation("File deleted from S3 successfully: {FileId}", id);
            }
            catch (AmazonS3Exception s3Ex) when (s3Ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File not found in S3 during deletion, continuing with database cleanup: {FileId}", id);
            }
            catch (AmazonS3Exception s3Ex)
            {
                _logger.LogError(s3Ex, "Failed to delete file from S3: {FileId}, ErrorCode: {ErrorCode}", id, s3Ex.ErrorCode);
                return false;
            }

            // Delete from database
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("File deleted successfully: {FileId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting file: {FileId}", id);
            return false;
        }
    }

    public async Task<FileModel?> UpdateAsync(FileModel fileModel)
    {
        if (fileModel?.Entity == null || fileModel.Data == null)
        {
            _logger.LogError("FileModel or its properties are null for update");
            return null;
        }

        if (fileModel.Entity.Id == Guid.Empty)
        {
            _logger.LogError("Invalid file ID provided for update: {FileId}", fileModel.Entity.Id);
            return null;
        }

        try
        {
            // Check if file exists
            var existingFile = await _context.Files.FindAsync(fileModel.Entity.Id);
            if (existingFile == null)
            {
                _logger.LogWarning("File not found for update: {FileId}", fileModel.Entity.Id);
                return null;
            }

            // Read the stream into memory to avoid stream positioning issues
            using var sourceStream = fileModel.Data;
            var memoryStream = new MemoryStream();
            
            await sourceStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            // Update file size
            fileModel.Entity.Size = memoryStream.Length;

            // Delete old file from S3
            try
            {
                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = GenerateS3Key(existingFile)
                });
            }
            catch (AmazonS3Exception s3Ex) when (s3Ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Old file not found in S3 during update, continuing: {FileId}", fileModel.Entity.Id);
            }

            // Upload new file to S3
            memoryStream.Position = 0;
            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = GenerateS3Key(fileModel.Entity),
                ContentType = fileModel.Entity.Type ?? "application/octet-stream",
                InputStream = memoryStream,
                UseChunkEncoding = false,
                DisablePayloadSigning = false
            };
            
            var response = await _s3Client.PutObjectAsync(request);
            
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                // Update database record
                _context.Entry(existingFile).CurrentValues.SetValues(fileModel.Entity);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("File updated successfully: {FileId}, Size: {Size} bytes", 
                    fileModel.Entity.Id, fileModel.Entity.Size);
                
                // Return a new stream for output
                var outputStream = new MemoryStream(memoryStream.ToArray());
                outputStream.Position = 0;
                
                return new FileModel
                {
                    Entity = fileModel.Entity,
                    Data = outputStream
                };
            }
            
            _logger.LogError("Failed to upload updated file to S3. Status code: {StatusCode}", response.HttpStatusCode);
            
            // Dispose the memory stream since we're not returning it
            memoryStream.Dispose();
            return null;
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "S3 error occurred while updating file: {FileId}, ErrorCode: {ErrorCode}", fileModel.Entity.Id, s3Ex.ErrorCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating file: {FileId}", fileModel.Entity.Id);
            return null;
        }
    }

    private string GenerateS3Key(File file)
    {
        // Generate a proper S3 key based on file ID and other properties
        return $"files/{file.Id:D}"; // Using standard GUID format
    }
}
