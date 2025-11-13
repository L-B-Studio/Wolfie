using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;

namespace Wolfie.Popups
{
    public partial class PrivacyPopup : Popup
    {
        public PrivacyPopup()
        {
            InitializeComponent();

            // Нативный контент для каждой вкладки (можно вынести в ресурсы/локализацию)
            TabsCarousel.ItemsSource = new[]
            {
                GetGeneralText(),
                GetDataText(),
                GetUsageText(),
                GetRightsText()
            };

            TabsCarousel.PositionChanged += TabsCarousel_PositionChanged;
            UpdateTabHeaderStates(0);
        }

        void TabsCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            UpdateTabHeaderStates(e.CurrentPosition);
        }

        void UpdateTabHeaderStates(int activeIndex)
        {
            // Простая визуальная индикация: фон активной кнопки
            TabBtnGeneral.BackgroundColor = activeIndex == 0 ? Colors.LightGray : Colors.Transparent;
            TabBtnData.BackgroundColor = activeIndex == 1 ? Colors.LightGray : Colors.Transparent;
            TabBtnUsage.BackgroundColor = activeIndex == 2 ? Colors.LightGray : Colors.Transparent;
            TabBtnRights.BackgroundColor = activeIndex == 3 ? Colors.LightGray : Colors.Transparent;
        }

        void OnTabHeaderClicked(object sender, EventArgs e)
        {
            if (sender == TabBtnGeneral) TabsCarousel.Position = 0;
            if (sender == TabBtnData) TabsCarousel.Position = 1;
            if (sender == TabBtnUsage) TabsCarousel.Position = 2;
            if (sender == TabBtnRights) TabsCarousel.Position = 3;
        }

        // Замените эти строки на полный текст каждой вкладки (или подтяните из ресурсов)
        string GetGeneralText() => "Мы обязуемся соблюдать конфиденциальность ваших данных и ...";
        string GetDataText() => "Данные, которые мы собираем: Аккаунт, Сообщения, Контакты, Технические данные ...";
        string GetUsageText() => "Использование данных: Работа сервиса, Безопасность, Улучшения ...";
        string GetRightsText() => "Права субъекта данных: доступ, исправление, удаление, переносимость ...";
    }
}
