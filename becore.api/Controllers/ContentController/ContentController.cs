using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using becore.api.Scheme;
using becore.shared.DTOs;
using becore.api.Services;
using becore.api.Services.Interfaces;
using becore.api.Models;
using File = becore.api.Scheme.System.File;

namespace becore.api.Controllers.ContentController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly ContentService _contentService;
        private readonly IFileS3Service _fileS3Service;
        private readonly UserManager<ApplicationUser> _user;

        public ContentController(ContentService contentService, IFileS3Service fileS3Service, UserManager<ApplicationUser> user)
        {
            _contentService = contentService;
            _fileS3Service = fileS3Service;
            _user = user;
        }

        [HttpGet("pages")]
        public async Task<ActionResult<IEnumerable<PageDto>>> GetPages([FromQuery] PageFilterDto filter)
        {
            var pages = await _contentService.GetPagesAsync(filter);
            var pageDtos = pages.Select(page => (PageDto)page);
            return Ok(pageDtos);
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<PageDto>>> GetAllPages()
        {
            var pages = await _contentService.GetPagesAsync();
            var pageDtos = pages.Select(page => (PageDto)page);
            return Ok(pageDtos);
        }

        [HttpGet("pages/{id}")]
        public async Task<ActionResult<PageDto>> GetPage(Guid id)
        {
            var page = await _contentService.GetPageByIdAsync(id);
            if (page == null)
                return NotFound();

            PageDto pageDto = page; // Implicit conversion
            return Ok(pageDto);
        }

        [HttpPost("pages")]
        public async Task<ActionResult<PageDto>> CreatePage(CreatePageDto createPageDto)
        {
            Page page = createPageDto; // Implicit conversion

            var file = await _fileS3Service.CreateAsync(new FileModel
            {
                Entity = new File
                {
                    Type = createPageDto.File.ContentType,
                    User = await _user.GetUserAsync(User)
                },
                Data = createPageDto.File.OpenReadStream()
            });

            var createdPage = await _contentService.CreatePageAsync(page);
            PageDto createdPageDto = createdPage; // Implicit conversion
            return CreatedAtAction(nameof(GetPage), new { id = createdPage.Id }, createdPageDto);
        }

        [HttpPost("pages/with-icons")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<PageDto>> CreatePageWithIcons([FromForm] CreatePageWithIconsDto dto)
        {
            // Parse tags from comma-separated string
            var tagList = dto.GetTagsList();

            var createPageDto = new CreatePageDto
            {
                Name = dto.Name,
                Description = dto.Description,
                Tags = tagList
            };

            // Create page first
            Page page = createPageDto;
            var createdPage = await _contentService.CreatePageAsync(page);

            // Upload icons if provided
            if (dto.QuadIcon != null)
            {
                var quadFileModel = new FileModel
                {
                    Entity = new File { Type = dto.QuadIcon.ContentType, User = await _user.FindByIdAsync(dto.userId.ToString()) },
                    Data = dto.QuadIcon.OpenReadStream()
                };
                var uploadedQuadIcon = await _fileS3Service.CreateAsync(quadFileModel);
                if (uploadedQuadIcon != null)
                {
                    createdPage.QuadIcon = uploadedQuadIcon.Entity.Id;
                }
            }

            if (dto.WideIcon != null)
            {
                var wideFileModel = new FileModel
                {
                    Entity = new File { Type = dto.WideIcon.ContentType },
                    Data = dto.WideIcon.OpenReadStream()
                };
                var uploadedWideIcon = await _fileS3Service.CreateAsync(wideFileModel);
                if (uploadedWideIcon != null)
                {
                    createdPage.WideIcon = uploadedWideIcon.Entity.Id;
                }
            }

            if (dto.File != null)
            {
                var FileModel = new FileModel
                {
                    Entity = new File { Type = dto.File.ContentType },
                    Data = dto.File.OpenReadStream()
                };
                var uploadedFile = await _fileS3Service.CreateAsync(FileModel);
                if (uploadedFile != null)
                {
                    createdPage.File = uploadedFile.Entity.Id;
                }
            }

            // Update page if icons were uploaded
            if (createdPage.QuadIcon != null || createdPage.WideIcon != null || createdPage.File != null)
            {
                await _contentService.UpdatePageAsync(createdPage.Id, createdPage);
            }

            PageDto createdPageDto = createdPage;
            return CreatedAtAction(nameof(GetPage), new { id = createdPage.Id }, createdPageDto);
        }

        [HttpPut("pages/{id}")]
        public async Task<IActionResult> UpdatePage(Guid id, UpdatePageDto updatePageDto)
        {
            var existingPage = await _contentService.GetPageByIdAsync(id);
            if (existingPage == null)
                return NotFound();

            existingPage.UpdateFromDto(updatePageDto);
            await _contentService.UpdatePageAsync(id, existingPage);
            return NoContent();
        }

        [HttpPut("pages/{id}/with-icons")]
        public async Task<IActionResult> UpdatePageWithIcons(Guid id, [FromForm] UpdatePageWithIconsDto dto)
        {
            var existingPage = await _contentService.GetPageByIdAsync(id);
            if (existingPage == null)
                return NotFound();

            // Parse tags from comma-separated string
            var tagList = dto.GetTagsList();

            // Update basic properties
            existingPage.Name = dto.Name;
            existingPage.Description = dto.Description;
            existingPage.Content = dto.Content;
            existingPage.Tags = tagList;

            // Handle QuadIcon replacement
            if (dto.ReplaceQuadIcon)
            {
                // Delete old icon if exists
                if (existingPage.QuadIcon.HasValue)
                {
                    await _fileS3Service.DeleteAsync(existingPage.QuadIcon.Value);
                    existingPage.QuadIcon = null;
                }

                // Upload new icon if provided
                if (dto.QuadIcon != null)
                {
                    var quadFileModel = new FileModel
                    {
                        Entity = new File { Type = dto.QuadIcon.ContentType },
                        Data = dto.QuadIcon.OpenReadStream()
                    };
                    var uploadedQuadIcon = await _fileS3Service.CreateAsync(quadFileModel);
                    if (uploadedQuadIcon != null)
                    {
                        existingPage.QuadIcon = uploadedQuadIcon.Entity.Id;
                    }
                }
            }

            // Handle WideIcon replacement
            if (dto.ReplaceWideIcon)
            {
                // Delete old icon if exists
                if (existingPage.WideIcon.HasValue)
                {
                    await _fileS3Service.DeleteAsync(existingPage.WideIcon.Value);
                    existingPage.WideIcon = null;
                }

                // Upload new icon if provided
                if (dto.WideIcon != null)
                {
                    var wideFileModel = new FileModel
                    {
                        Entity = new File { Type = dto.WideIcon.ContentType },
                        Data = dto.WideIcon.OpenReadStream()
                    };
                    var uploadedWideIcon = await _fileS3Service.CreateAsync(wideFileModel);
                    if (uploadedWideIcon != null)
                    {
                        existingPage.WideIcon = uploadedWideIcon.Entity.Id;
                    }
                }
            }

            await _contentService.UpdatePageAsync(id, existingPage);
            return NoContent();
        }

        [HttpDelete("pages/{id}")]
        public async Task<IActionResult> DeletePage(Guid id)
        {
            var page = await _contentService.GetPageByIdAsync(id);
            if (page == null)
                return NotFound();

            // Delete associated icons before deleting the page
            if (page.QuadIcon.HasValue)
            {
                await _fileS3Service.DeleteAsync(page.QuadIcon.Value);
            }
            if (page.WideIcon.HasValue)
            {
                await _fileS3Service.DeleteAsync(page.WideIcon.Value);
            }

            var success = await _contentService.DeletePageAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("pages/{id}/icons/{iconType}")]
        public async Task<IActionResult> DeleteIcon(Guid id, string iconType)
        {
            var page = await _contentService.GetPageByIdAsync(id);
            if (page == null) return NotFound();

            switch (iconType.ToLower())
            {
                case "quad":
                    if (page.QuadIcon.HasValue)
                    {
                        await _fileS3Service.DeleteAsync(page.QuadIcon.Value);
                        page.QuadIcon = null;
                        await _contentService.UpdatePageAsync(id, page);
                    }
                    break;
                case "wide":
                    if (page.WideIcon.HasValue)
                    {
                        await _fileS3Service.DeleteAsync(page.WideIcon.Value);
                        page.WideIcon = null;
                        await _contentService.UpdatePageAsync(id, page);
                    }
                    break;
                default:
                    return BadRequest("Invalid icon type. Use 'quad' or 'wide'.");
            }

            return NoContent();
        }

        [HttpPost("pages/{id}/upload-icons")]
        public async Task<IActionResult> UploadIcons(Guid id, [FromForm] UploadIconsDto dto)
        {
            var page = await _contentService.GetPageByIdAsync(id);
            if (page == null) return NotFound();

            if (!dto.HasFilesToUpload)
                return BadRequest("No files provided for upload.");

            // Upload QuadIcon
            if (dto.QuadIcon != null)
            {
                var quadFileModel = new FileModel
                {
                    Entity = new File { Type = dto.QuadIcon.ContentType },
                    Data = dto.QuadIcon.OpenReadStream()
                };
                var uploadedQuadIcon = await _fileS3Service.CreateAsync(quadFileModel);
                if (uploadedQuadIcon == null) return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload QuadIcon");
                page.QuadIcon = uploadedQuadIcon.Entity.Id;
            }

            // Upload WideIcon
            if (dto.WideIcon != null)
            {
                var wideFileModel = new FileModel
                {
                    Entity = new File { Type = dto.WideIcon.ContentType },
                    Data = dto.WideIcon.OpenReadStream()
                };
                var uploadedWideIcon = await _fileS3Service.CreateAsync(wideFileModel);
                if (uploadedWideIcon == null) return StatusCode(StatusCodes.Status500InternalServerError, "Failed to upload WideIcon");
                page.WideIcon = uploadedWideIcon.Entity.Id;
            }

            await _contentService.UpdatePageAsync(id, page);
            return NoContent();
        }

        [HttpGet("tags")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllTags()
        {
            var tags = await _contentService.GetAllTagsAsync();
            return Ok(tags);
        }

    }
}
