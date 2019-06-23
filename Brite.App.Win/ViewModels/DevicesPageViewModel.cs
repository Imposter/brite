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
    class DevicesPageViewModel : IPageViewModel
    {
        // TODO: Add way of sorting by order
        public string Title => "Devices";
        public Control Icon => new PackIconModern { Kind = PackIconModernKind.HardwareCpu };

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
