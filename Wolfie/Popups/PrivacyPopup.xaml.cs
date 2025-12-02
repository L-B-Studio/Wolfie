using CommunityToolkit.Maui.Views;

namespace Wolfie.Popups
{
    public partial class PrivacyPopup : Popup
    {
        public PrivacyPopup()
        {
            InitializeComponent();

            // Берём тексты из ResourceDictionary
            TabsCarousel.ItemsSource = new string[]
            {
                (string)Resources["GeneralText"],
                (string)Resources["DataText"],
                (string)Resources["UsageText"],
                (string)Resources["RightsText"]
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
    }
}
