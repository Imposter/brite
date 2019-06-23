using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Brite.App.Win.ViewModels.Pages
{
    public sealed class DevicesPageViewModel : IPageViewModel
    {
        public int Order => 1;
        public string Title => "Devices";
        public Control Icon => new PackIconModern { Kind = PackIconModernKind.HardwareCpu };

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
