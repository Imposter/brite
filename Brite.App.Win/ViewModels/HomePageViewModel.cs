using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;

namespace Brite.App.Win.ViewModels
{
    class HomePageViewModel : IPageViewModel
    {
        public string Title => "Home";
        public Control Icon => new PackIconModern { Kind = PackIconModernKind.Home };

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
