using becore.api.Scheme;
using Microsoft.EntityFrameworkCore;

public class ContentService
{
    private readonly ApplicationContext _context;

    public ContentService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Page>> GetPagesAsync(PageFilter? filter = null)
    {
        var query = _context.Pages
            .Include(p => p.PageTags)
            .AsQueryable();

        if (filter != null)
        {
            // Фильтрация по имени
            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(page =>
                    page.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
            }

            // Фильтрация по тегу
            if (!string.IsNullOrEmpty(filter.Tag))
            {
                query = query.Where(page =>
                    page.PageTags.Any(pt => pt.TagName.Contains(filter.Tag, StringComparison.OrdinalIgnoreCase)));
            }
        }

        return await query.ToListAsync();
    }

    public async Task<Page?> GetPageByIdAsync(Guid id)
    {
        return await _context.Pages
            .Include(p => p.PageTags)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Page> CreatePageAsync(Page page)
    {
        // Id уже установлен в implicit operator CreatePageDto
        // Но если он пустой, устанавливаем новый
        if (page.Id == Guid.Empty)
        {
            page.Id = Guid.NewGuid();
            
            // Обновляем PageId для всех тегов
            foreach (var pageTag in page.PageTags)
            {
                pageTag.PageId = page.Id;
            }
        }
        
        _context.Pages.Add(page);
        await _context.SaveChangesAsync();
        
        // Загружаем созданную страницу с тегами
        return await GetPageByIdAsync(page.Id) ?? page;
    }

    public async Task<Page?> UpdatePageAsync(Guid id, Page updatedPage)
    {
        var existingPage = await _context.Pages
            .Include(p => p.PageTags)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (existingPage == null)
            return null;

        existingPage.Name = updatedPage.Name;
        existingPage.Description = updatedPage.Description;
        existingPage.QuadIcon = updatedPage.QuadIcon;
        existingPage.WideIcon = updatedPage.WideIcon;
        
        // Обновляем теги
        // Удаляем старые теги
        _context.PageTags.RemoveRange(existingPage.PageTags);
        
        // Добавляем новые теги
        existingPage.PageTags.Clear();
        foreach (var tag in updatedPage.Tags)
        {
            existingPage.PageTags.Add(new PageTag 
            { 
                TagName = tag, 
                PageId = id 
            });
        }

        await _context.SaveChangesAsync();
        return existingPage;
    }

    public async Task<bool> DeletePageAsync(Guid id)
    {
        var page = await _context.Pages
            .Include(p => p.PageTags)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (page == null)
            return false;

        // PageTags будут удалены автоматически благодаря каскадному удалению
        _context.Pages.Remove(page);
        await _context.SaveChangesAsync();
        return true;
    }
}

// Класс фильтра для model binding
public class PageFilter
{
    public string? Name { get; set; }
    public string? Tag { get; set; }

    public PageFilter()
    {
    }

    public PageFilter(string? name = null, string? tag = null)
    {
        Name = name;
        Tag = tag;
    }
}
