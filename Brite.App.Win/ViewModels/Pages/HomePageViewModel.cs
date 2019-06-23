using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Brite.App.Win.ViewModels.Pages
{
    public sealed class HomePageViewModel : BaseViewModel, IPageViewModel
    {
        public int Order => 0;
        public string Title => "Home";
        public Control Icon => new PackIconModern { Kind = PackIconModernKind.Home };

        // TODO: Implement

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
