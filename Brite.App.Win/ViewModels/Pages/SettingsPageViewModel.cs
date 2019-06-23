using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Brite.App.Win.ViewModels.Pages
{
    public sealed class SettingsPageViewModel : BaseViewModel, IOptionPageViewModel
    {
        public int Order => 0;
        public string Title => "Settings";
        public Control Icon => new PackIconModern { Kind = PackIconModernKind.Settings };

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
