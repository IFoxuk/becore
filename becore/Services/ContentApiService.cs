using System.Text.Json;
using becore.shared.DTOs;

namespace becore.Services;

public class ContentApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ContentApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<IEnumerable<PageDto>> SearchPagesAsync(string? name = null, string? tag = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(name))
                queryParams.Add($"name={Uri.EscapeDataString(name)}");
            
            if (!string.IsNullOrEmpty(tag))
                queryParams.Add($"tag={Uri.EscapeDataString(tag)}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"api/content/pages{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var pages = JsonSerializer.Deserialize<IEnumerable<PageDto>>(json, _jsonOptions);
                return pages ?? Enumerable.Empty<PageDto>();
            }
            
            return Enumerable.Empty<PageDto>();
        }
        catch (Exception ex)
        {
            // В реальном приложении здесь должно быть логирование
            Console.WriteLine($"Ошибка при поиске страниц: {ex.Message}");
            return Enumerable.Empty<PageDto>();
        }
    }

    public async Task<PageDto?> GetPageByIdAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/content/pages/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PageDto>(json, _jsonOptions);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении страницы: {ex.Message}");
            return null;
        }
    }
    
    public async Task<PageDto?> GetPageByIdAsync(int id)
    {
        try
        {
            // Получаем все страницы и ищем по hash code ID
            var pages = await SearchPagesAsync();
            return pages.FirstOrDefault(p => p.Id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении страницы по ID: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<string>> GetAllTagsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/content/tags");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tags = JsonSerializer.Deserialize<IEnumerable<string>>(json, _jsonOptions);
                return tags ?? Enumerable.Empty<string>();
            }
            
            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении тегов: {ex.Message}");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<SearchResultDto?> SearchContentAsync(string query, int page = 1, int pageSize = 12)
    {
        try
        {
            // Парсим query для извлечения имени и тегов
            var name = "";
            var tag = "";
            
            if (!string.IsNullOrEmpty(query))
            {
                // Ищем теги в формате "tag:tagname"
                var tagMatch = System.Text.RegularExpressions.Regex.Match(query, @"tag:([^\s]+)");
                if (tagMatch.Success)
                {
                    tag = tagMatch.Groups[1].Value;
                    // Удаляем тег из основного запроса
                    query = System.Text.RegularExpressions.Regex.Replace(query, @"tag:[^\s]+", "").Trim();
                }
                name = query.Trim();
            }
            
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(name))
                queryParams.Add($"name={Uri.EscapeDataString(name)}");
                
            if (!string.IsNullOrEmpty(tag))
                queryParams.Add($"tag={Uri.EscapeDataString(tag)}");
            
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"api/content/pages{queryString}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var pages = JsonSerializer.Deserialize<IEnumerable<PageDto>>(json, _jsonOptions) ?? new List<PageDto>();
                
                // Симулируем пагинацию на клиенте (так как API пока не поддерживает)
                var totalCount = pages.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var pagedItems = pages.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                
                return new SearchResultDto
                {
                    Items = pagedItems,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при поиске контента: {ex.Message}");
            return null;
        }
    }

    #region Methods for working with pages and icons

    /// <summary>
    /// Создает новую страницу с иконками
    /// </summary>
    public async Task<PageDto?> CreatePageWithIconsAsync(CreatePageWithIconsDto dto)
    {
        try
        {
            using var formData = new MultipartFormDataContent();

            // Add text fields
            formData.Add(new StringContent(dto.userId.ToString()), "userId");

            // Add text fields
            formData.Add(new StringContent(dto.Name), "Name");
            
            if (!string.IsNullOrEmpty(dto.Description))
                formData.Add(new StringContent(dto.Description), "Description");
            
            if (!string.IsNullOrEmpty(dto.Content))
                formData.Add(new StringContent(dto.Content), "Content");
            
            if (!string.IsNullOrEmpty(dto.Tags))
                formData.Add(new StringContent(dto.Tags), "Tags");
            
            // Add file fields
            if (dto.QuadIcon != null)
            {
                var quadIconContent = new StreamContent(dto.QuadIcon.OpenReadStream());
                quadIconContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.QuadIcon.ContentType);
                formData.Add(quadIconContent, "QuadIcon", dto.QuadIcon.FileName);
            }
            
            if (dto.WideIcon != null)
            {
                var wideIconContent = new StreamContent(dto.WideIcon.OpenReadStream());
                wideIconContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.WideIcon.ContentType);
                formData.Add(wideIconContent, "WideIcon", dto.WideIcon.FileName);
            }
            
            var response = await _httpClient.PostAsync("api/content/pages/with-icons", formData);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PageDto>(json, _jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Ошибка создания страницы: {response.StatusCode}, {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании страницы с иконками: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Обновляет страницу с иконками
    /// </summary>
    public async Task<bool> UpdatePageWithIconsAsync(Guid pageId, UpdatePageWithIconsDto dto)
    {
        try
        {
            using var formData = new MultipartFormDataContent();
            
            // Add text fields
            formData.Add(new StringContent(dto.Name), "Name");
            
            if (!string.IsNullOrEmpty(dto.Description))
                formData.Add(new StringContent(dto.Description), "Description");
            
            if (!string.IsNullOrEmpty(dto.Content))
                formData.Add(new StringContent(dto.Content), "Content");
            
            if (!string.IsNullOrEmpty(dto.Tags))
                formData.Add(new StringContent(dto.Tags), "Tags");
            
            // Add boolean flags
            formData.Add(new StringContent(dto.ReplaceQuadIcon.ToString().ToLower()), "ReplaceQuadIcon");
            formData.Add(new StringContent(dto.ReplaceWideIcon.ToString().ToLower()), "ReplaceWideIcon");
            
            // Add file fields
            if (dto.QuadIcon != null)
            {
                var quadIconContent = new StreamContent(dto.QuadIcon.OpenReadStream());
                quadIconContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.QuadIcon.ContentType);
                formData.Add(quadIconContent, "QuadIcon", dto.QuadIcon.FileName);
            }
            
            if (dto.WideIcon != null)
            {
                var wideIconContent = new StreamContent(dto.WideIcon.OpenReadStream());
                wideIconContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.WideIcon.ContentType);
                formData.Add(wideIconContent, "WideIcon", dto.WideIcon.FileName);
            }
            
            var response = await _httpClient.PutAsync($"api/content/pages/{pageId}/with-icons", formData);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка обновления страницы: {response.StatusCode}, {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении страницы с иконками: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Загружает иконки для существующей страницы
    /// </summary>
    public async Task<bool> UploadIconsAsync(Guid pageId, UploadIconsDto dto)
    {
        try
        {
            if (!dto.HasFilesToUpload)
            {
                Console.WriteLine("Нет файлов для загрузки");
                return false;
            }
            
            using var formData = new MultipartFormDataContent();
            
            // Add file fields
            if (dto.QuadIcon != null)
            {
                var quadIconContent = new StreamContent(dto.QuadIcon.OpenReadStream());
                quadIconContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.QuadIcon.ContentType);
                formData.Add(quadIconContent, "QuadIcon", dto.QuadIcon.FileName);
            }
            
            if (dto.WideIcon != null)
            {
                var wideIconContent = new StreamContent(dto.WideIcon.OpenReadStream());
                wideIconContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.WideIcon.ContentType);
                formData.Add(wideIconContent, "WideIcon", dto.WideIcon.FileName);
            }
            
            var response = await _httpClient.PostAsync($"api/content/pages/{pageId}/upload-icons", formData);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка загрузки иконок: {response.StatusCode}, {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке иконок: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Удаляет конкретную иконку страницы
    /// </summary>
    public async Task<bool> DeleteIconAsync(Guid pageId, string iconType)
    {
        try
        {
            if (iconType != "quad" && iconType != "wide")
            {
                Console.WriteLine("Неверный тип иконки. Используйте 'quad' или 'wide'");
                return false;
            }
            
            var response = await _httpClient.DeleteAsync($"api/content/pages/{pageId}/icons/{iconType}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка удаления иконки: {response.StatusCode}, {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении иконки: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Удаляет страницу
    /// </summary>
    public async Task<bool> DeletePageAsync(Guid pageId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/content/pages/{pageId}");
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка удаления страницы: {response.StatusCode}, {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при удалении страницы: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Получает URL файла для отображения иконки
    /// </summary>
    public string GetFileUrl(Guid fileId)
    {
        return $"{_httpClient.BaseAddress?.ToString().TrimEnd('/')}/api/file/{fileId}";
    }

    /// <summary>
    /// Получает информацию о файле без загрузки самого файла
    /// </summary>
    public async Task<FileInfoDto?> GetFileInfoAsync(Guid fileId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/file/{fileId}/info");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                
                return new FileInfoDto
                {
                    Id = root.GetProperty("id").GetGuid(),
                    Type = root.TryGetProperty("type", out var typeProperty) ? typeProperty.GetString() : null,
                    Size = root.GetProperty("size").GetInt64(),
                    Url = GetFileUrl(fileId)
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении информации о файле: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Загружает файл и возвращает его как Stream
    /// </summary>
    public async Task<Stream?> DownloadFileAsync(Guid fileId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/file/{fileId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            return null;
        }
    }

    #endregion
}
