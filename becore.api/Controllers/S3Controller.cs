using System.Net;
using becore.api.Models;
using becore.api.Services;
using Microsoft.AspNetCore.Mvc;
using File = becore.api.Scheme.System.File;

namespace becore.api.Controllers;

[ApiController]
[Route("api/s3")]
public class S3Controller : ControllerBase
{
    private readonly FileS3Service _fileS3Service;
    private readonly ILogger<S3Controller> _logger;

    public S3Controller(FileS3Service fileS3Service, ILogger<S3Controller> logger)
    {
        _fileS3Service = fileS3Service;
        _logger = logger;
    }
    
    // Разрешенные типы файлов изображений
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tiff" };
    private readonly string[] _allowedMimeTypes = { 
        "image/jpeg", "image/jpg", "image/png", "image/gif", 
        "image/webp", "image/bmp", "image/tiff" 
    };
    
    // Максимальный размер файла (5MB)
    private const long MaxFileSize = 5 * 1024 * 1024;

    [HttpPost("image/{id}")]
    public async Task<IActionResult> UploadImage([FromRoute] Guid id, [FromForm(Name = "Data")] IFormFile file)
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

            // Создаем FileModel для работы с FileS3Service
            var fileModel = new FileModel
            {
                Entity = new File
                {
                    Id = id,
                    Type = file.ContentType
                },
                Data = file.OpenReadStream()
            };

            var result = await _fileS3Service.CreateAsync(fileModel);

            if (result != null)
            {
                _logger.LogInformation("Successfully uploaded image for ID: {Id}, Size: {Size} bytes", id, result.Entity.Size);
                return Ok(new {
                    success = true,
                    id = result.Entity.Id,
                    fileName = file.FileName,
                    fileSize = result.Entity.Size,
                    contentType = result.Entity.Type,
                    uploadedAt = DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogError("Upload failed for ID: {Id}", id);
                return BadRequest(new { error = "Upload failed" });
            }
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
            
            var fileModel = await _fileS3Service.GetAsync(id);

            if (fileModel == null)
            {
                _logger.LogWarning("Image not found for ID: {Id}", id);
                return NotFound(new { error = "Image not found", id });
            }

            _logger.LogInformation("Successfully retrieved image for ID: {Id}, ContentType: {ContentType}, Size: {Size}", 
                id, fileModel.Entity.Type, fileModel.Entity.Size);
                
            // Добавляем кэширование заголовки
            Response.Headers.CacheControl = "public, max-age=86400"; // 24 часа
            
            return File(fileModel.Data, fileModel.Entity.Type ?? "application/octet-stream");
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
            
            var success = await _fileS3Service.DeleteAsync(id);
            
            if (success)
            {
                _logger.LogInformation("Successfully deleted image for ID: {Id}", id);
                return NoContent();
            }
            else
            {
                _logger.LogWarning("Image not found for deletion, ID: {Id}", id);
                return NotFound(new { error = "Image not found", id });
            }
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
