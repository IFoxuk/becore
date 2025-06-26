using becore.api.Models;
using becore.api.Scheme;
using becore.api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using File = becore.api.Scheme.System.File;

namespace becore.api.Controllers.SystemControllers;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly FileService _fileService;
    private readonly UserManager<ApplicationUser> _user;

    public FileController(FileService fileService, UserManager<ApplicationUser> user)
    {
        _fileService = fileService;
        _user = user;
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        var file = await _fileService.GetAsync(id);
        return file != null ? Ok(file) : NotFound();
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
        
        return file != null ? Ok(file) : BadRequest();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        if (await _fileService.GetAsync(id) == null)
            return NotFound();
        
        var deleteResult = _fileService.DeleteAsync(id);
        await Task.Run(() => deleteResult);
        return deleteResult.IsCompleted ? Ok() : BadRequest();
    }

    [HttpPut()]
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
        return newFile != null ? Ok(newFile) : BadRequest();
    }
    
}