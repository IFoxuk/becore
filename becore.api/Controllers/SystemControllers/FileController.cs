using becore.api.Models;
using becore.api.Scheme;
using becore.api.Services;
using becore.api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using File = becore.api.Scheme.System.File;

namespace becore.api.Controllers.SystemControllers;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly IFileS3Service _fileService;
    private readonly UserManager<ApplicationUser> _user;

    public FileController(IFileS3Service fileService, UserManager<ApplicationUser> user)
    {
        _fileService = fileService;
        _user = user;
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        var file = await _fileService.GetAsync(id);
        if (file == null) return NotFound();
        
        return File(file.Data, 
            file.Entity.Type == string.Empty ? "application/octet-stream" : file.Entity.Type, 
            $"file_{id}");
    }
    
    [HttpGet("{id:guid}/info")]
    public async Task<IActionResult> GetFileInfo(Guid id)
    {
        var file = await _fileService.GetAsync(id);
        if (file == null) return NotFound();
        
        // Return only file metadata without the stream
        return Ok(new {
            Id = file.Entity.Id,
            Type = file.Entity.Type,
            Size = file.Entity.Size,
            UserId = file.Entity.User?.Id
        });
    }

    [HttpPost("{userId}")]
    public async Task<IActionResult> CreateFile(string userId, IFormFile data)
    {
        var file = await _fileService.CreateAsync(new FileModel
        {
            Entity = new File
            {
                Type = data.ContentType,
                User = await _user.FindByIdAsync(userId)
            },
            Data = data.OpenReadStream()
        });
        
        if (file == null) return BadRequest();
        
        // Return only file metadata
        return Ok(new {
            Id = file.Entity.Id,
            Type = file.Entity.Type,
            Size = file.Entity.Size,
            UserId = file.Entity.User?.Id
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        if (await _fileService.GetAsync(id) == null)
            return NotFound();
        
        var deleteResult = await _fileService.DeleteAsync(id);
        return deleteResult ? Ok() : BadRequest();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateFile(File file, IFormFile data)
    {
        if (await _fileService.GetAsync(file.Id) == null)
            return NotFound();
        
        file.Type = data.ContentType;
        var newFile = await _fileService.UpdateAsync(new FileModel
        {
            Entity = file,
            Data = data.OpenReadStream()
        });
        
        if (newFile == null) return BadRequest();
        
        // Return only file metadata
        return Ok(new {
            Id = newFile.Entity.Id,
            Type = newFile.Entity.Type,
            Size = newFile.Entity.Size,
            UserId = newFile.Entity.User?.Id
        });
    }
    
}