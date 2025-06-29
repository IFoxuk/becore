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
            { }
        }
    }
}
