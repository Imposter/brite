using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Brite.App.Win.ViewModels.Pages
{
    class HomePageViewModel : IPageViewModel
    {
        public int Order => 0;
        public string Title => "Home";
        public Control Icon => new PackIconModern { Kind = PackIconModernKind.Home };

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
