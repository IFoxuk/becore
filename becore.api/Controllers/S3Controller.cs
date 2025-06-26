using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using becore.api.S3;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace becore.api.Controllers;

[ApiController]
[Route("api/s3")]
public class S3Controller(IOptions<S3Options> options, IAmazonS3 s3Client, ILogger<S3Controller> logger) : ControllerBase
{
    private readonly S3Options _options = options.Value;
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly ILogger<S3Controller> _logger = logger;
    
    // Разрешенные типы файлов изображений
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tiff" };
    private readonly string[] _allowedMimeTypes = { 
        "image/jpeg", "image/jpg", "image/png", "image/gif", 
        "image/webp", "image/bmp", "image/tiff" 
    };
    
    // Максимальный размер файла (5MB)
    private const long MaxFileSize = 5 * 1024 * 1024;

    [HttpPost("image/{id}")]
    public async Task<ActionResult<PutObjectResponse>> UploadImage([FromRoute] Guid id,
        [FromForm(Name = "Data")] IFormFile file)
    {
        try
        {
            _logger.LogInformation("Starting image upload for ID: {Id}, FileName: {FileName}, ContentType: {ContentType}, Size: {Size}", 
                id, file.FileName, file.ContentType, file.Length);

            // Валидация файла
            var validationResult = ValidateImageFile(file);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("File validation failed for ID: {Id}. Errors: {Errors}", id, string.Join(", ", validationResult.Errors));
                return BadRequest(new { errors = validationResult.Errors });
            }

            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = id.ToString(),
                ContentType = file.ContentType,
                InputStream = file.OpenReadStream(),
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                Metadata = 
                {
                    ["original-name"] = file.FileName,
                    ["upload-date"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["file-size"] = file.Length.ToString()
                }
            };
            
            var response = await _s3Client.PutObjectAsync(request);
            
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully uploaded image for ID: {Id}, ETag: {ETag}", id, response.ETag);
                return Ok(new { 
                    success = true, 
                    id = id, 
                    etag = response.ETag,
                    uploadedAt = DateTime.UtcNow,
                    fileName = file.FileName,
                    fileSize = file.Length
                });
            }
            else
            {
                _logger.LogError("S3 upload failed for ID: {Id}, StatusCode: {StatusCode}", id, response.HttpStatusCode);
                return StatusCode(500, new { error = "Upload failed", statusCode = response.HttpStatusCode });
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 exception during upload for ID: {Id}. ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}", 
                id, ex.ErrorCode, ex.Message);
            return StatusCode(500, new { error = "S3 service error", details = ex.Message, errorCode = ex.ErrorCode });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument during upload for ID: {Id}", id);
            return BadRequest(new { error = "Invalid request parameters", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during upload for ID: {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = "An unexpected error occurred" });
        }
    }
    
    [HttpGet("image/{id}")]
    public async Task<IActionResult> GetImage([FromRoute] Guid id)
    {
        try
        {
            _logger.LogInformation("Retrieving image for ID: {Id}", id);
            
            var objectRequest = new GetObjectRequest
            {
                BucketName = _options.BucketName,
                Key = id.ToString()
            };

            var response = await _s3Client.GetObjectAsync(objectRequest);
            
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully retrieved image for ID: {Id}, ContentType: {ContentType}, Size: {Size}", 
                    id, response.Headers.ContentType, response.ContentLength);
                    
                // Добавляем кэширование заголовки
                Response.Headers.CacheControl = "public, max-age=86400"; // 24 часа
                Response.Headers.ETag = response.ETag;
                
                return File(response.ResponseStream, response.Headers.ContentType ?? "application/octet-stream");
            }
            
            _logger.LogWarning("Image not found for ID: {Id}, StatusCode: {StatusCode}", id, response.HttpStatusCode);
            return NotFound(new { error = "Image not found", id = id });
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogWarning("Image not found in S3 for ID: {Id}", id);
            return NotFound(new { error = "Image not found", id = id });
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 exception during image retrieval for ID: {Id}. ErrorCode: {ErrorCode}", id, ex.ErrorCode);
            return StatusCode(500, new { error = "S3 service error", details = ex.Message, errorCode = ex.ErrorCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during image retrieval for ID: {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = "An unexpected error occurred" });
        }
    }

    [HttpDelete("image/{id}")]
    public async Task<IActionResult> DeleteImage([FromRoute] Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting image for ID: {Id}", id);
            
            // Сначала проверяем, существует ли объект
            try
            {
                await _s3Client.GetObjectMetadataAsync(_options.BucketName, id.ToString());
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode == "NotFound" || ex.ErrorCode == "NoSuchKey")
            {
                _logger.LogWarning("Attempted to delete non-existent image for ID: {Id}", id);
                return NotFound(new { error = "Image not found", id = id });
            }
            
            var objectRequest = new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = id.ToString()
            };
            
            var response = await _s3Client.DeleteObjectAsync(objectRequest);
            
            if (response.HttpStatusCode == HttpStatusCode.NoContent)
            {
                _logger.LogInformation("Successfully deleted image for ID: {Id}", id);
                return NoContent();
            }
            
            _logger.LogError("Failed to delete image for ID: {Id}, StatusCode: {StatusCode}", id, response.HttpStatusCode);
            return StatusCode(500, new { error = "Delete failed", statusCode = response.HttpStatusCode });
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "S3 exception during image deletion for ID: {Id}. ErrorCode: {ErrorCode}", id, ex.ErrorCode);
            return StatusCode(500, new { error = "S3 service error", details = ex.Message, errorCode = ex.ErrorCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during image deletion for ID: {Id}", id);
            return StatusCode(500, new { error = "Internal server error", details = "An unexpected error occurred" });
        }
    }
    
    /// <summary>
    /// Валидирует загружаемый файл изображения
    /// </summary>
    /// <param name="file">Файл для валидации</param>
    /// <returns>Результат валидации</returns>
    private FileValidationResult ValidateImageFile(IFormFile file)
    {
        var errors = new List<string>();
        
        // Проверка на null
        if (file == null)
        {
            errors.Add("Файл не предоставлен");
            return new FileValidationResult { IsValid = false, Errors = errors };
        }
        
        // Проверка размера файла
        if (file.Length == 0)
        {
            errors.Add("Файл пустой");
        }
        else if (file.Length > MaxFileSize)
        {
            errors.Add($"Размер файла превышает максимально допустимый ({MaxFileSize / (1024 * 1024)} MB)");
        }
        
        // Проверка расширения файла
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
        {
            errors.Add($"Недопустимое расширение файла. Разрешены: {string.Join(", ", _allowedExtensions)}");
        }
        
        // Проверка MIME типа
        if (string.IsNullOrEmpty(file.ContentType) || !_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            errors.Add($"Недопустимый тип файла. Разрешены: {string.Join(", ", _allowedMimeTypes)}");
        }
        
        // Проверка имени файла
        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            errors.Add("Имя файла не указано");
        }
        else if (file.FileName.Length > 255)
        {
            errors.Add("Имя файла слишком длинное (максимум 255 символов)");
        }
        
        return new FileValidationResult 
        { 
            IsValid = errors.Count == 0, 
            Errors = errors 
        };
    }
}

/// <summary>
/// Результат валидации файла
/// </summary>
public record FileValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
}
