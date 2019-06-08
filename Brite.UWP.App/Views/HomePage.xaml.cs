using Brite.UWP.App.ViewModels;

using Windows.UI.Xaml.Controls;

namespace Brite.UWP.App.Views
{
    public sealed partial class HomePage : Page
    {
        public HomeViewModel ViewModel { get; } = new HomeViewModel();

        public HomePage()
        {
            InitializeComponent();
        }
    }
}
