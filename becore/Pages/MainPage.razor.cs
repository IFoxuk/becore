using becore.Services;
using Microsoft.AspNetCore.Components;
using PageDto = becore.shared.DTOs.PageDto;

namespace becore.Pages
{
    public partial class MainPage
    {
        private string selectedSlide = "1";
        private List<string> tags = new() { "tag1", "tag2", "tag3", "tag4", "tag5", "tag6", "tag,7", "tag8" };
        private List<PageDto> _bestModifications = new();
        private List<PageDto> _bestWorlds = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Загружаем первые 12 элементов для отображения на главной странице
                var response = await ContentService.SearchContentAsync("", 1, 12);
                _bestModifications = response?.Items ?? new List<PageDto>();
                
                // Пока что используем те же данные для миров
                _bestWorlds = _bestModifications.Take(6).ToList();
            }
            catch (Exception)
            {
                // В случае ошибки API создаем тестовые данные
                _bestModifications = CreateMockData("modification");
                _bestWorlds = CreateMockData("world", 6);
            }
        }

        private List<PageDto> CreateMockData(string contentType = "modification", int count = 12)
        {
            var mockData = new List<PageDto>();
            var isWorld = contentType == "world";
            
            for (int i = 1; i <= count; i++)
            {
                mockData.Add(new PageDto
                {
                    Id = i + (isWorld ? 1000 : 0), // Уникальные ID
                    Title = isWorld ? $"Мир {i}" : $"Модификация {i}",
                    Author = $"Автор {i}",
                    Description = isWorld 
                        ? $"Описание мира {i}. Это тестовое описание для мира номер {i}, которое показывает как работает система отображения карточек контента."
                        : $"Описание модификации {i}. Это тестовое описание для модификации номер {i}, которое показывает как работает система отображения карточек контента.",
                    ImageUrl = "img/jpg/world.jpg",
                    Tags = isWorld 
                        ? new List<string> { $"тег{i}", "мир", "minecraft" }
                        : new List<string> { $"тег{i}", "модификация", "minecraft" },
                    CreatedAt = DateTime.Now.AddDays(-i),
                    ViewCount = i * 100,
                    DownloadCount = i * 50
                });
            }
            return mockData;
        }
    }
}
