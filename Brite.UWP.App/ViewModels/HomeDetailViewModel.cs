using System;
using System.Linq;

using Brite.UWP.App.Core.Models;
using Brite.UWP.App.Core.Services;
using Brite.UWP.App.Helpers;

namespace Brite.UWP.App.ViewModels
{
    public class HomeDetailViewModel : Observable
    {
        private SampleOrder _item;

        public SampleOrder Item
        {
            get { return _item; }
            set { Set(ref _item, value); }
        }

        public HomeDetailViewModel()
        {
        }

        public void Initialize(long orderId)
        {
            var data = SampleDataService.GetContentGridData();
            Item = data.First(i => i.OrderId == orderId);
        }
    }
}
