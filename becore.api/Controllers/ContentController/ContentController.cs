using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using becore.api.Scheme;
using becore.api.DTOs;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PageDto>>> GetPages([FromQuery] PageFilter filter)
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

        [HttpGet("{id}")]
        public async Task<ActionResult<PageDto>> GetPage(Guid id)
        {
            var page = await _contentService.GetPageByIdAsync(id);
            if (page == null)
                return NotFound();

            PageDto pageDto = page; // Implicit conversion
            return Ok(pageDto);
        }

        [HttpPost]
        public async Task<ActionResult<PageDto>> CreatePage(CreatePageDto createPageDto)
        {
            Page page = createPageDto; // Implicit conversion
            var createdPage = await _contentService.CreatePageAsync(page);
            PageDto createdPageDto = createdPage; // Implicit conversion
            return CreatedAtAction(nameof(GetPage), new { id = createdPage.Id }, createdPageDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePage(Guid id, UpdatePageDto updatePageDto)
        {
            var existingPage = await _contentService.GetPageByIdAsync(id);
            if (existingPage == null)
                return NotFound();

            updatePageDto.UpdatePage(existingPage);
            await _contentService.UpdatePageAsync(id, existingPage);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePage(Guid id)
        {
            var success = await _contentService.DeletePageAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
