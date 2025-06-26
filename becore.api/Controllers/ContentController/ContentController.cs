using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using becore.api.Scheme;
using becore.shared.DTOs;

namespace becore.api.Controllers.ContentController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly ContentService _contentService;

        public ContentController(ContentService contentService)
        {
            _contentService = contentService;
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
            var createdPage = await _contentService.CreatePageAsync(page);
            PageDto createdPageDto = createdPage; // Implicit conversion
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

        [HttpDelete("pages/{id}")]
        public async Task<IActionResult> DeletePage(Guid id)
        {
            var success = await _contentService.DeletePageAsync(id);
            if (!success)
                return NotFound();

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
