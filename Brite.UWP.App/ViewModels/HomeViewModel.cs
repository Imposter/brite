using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Brite.UWP.App.Core.Models;
using Brite.UWP.App.Core.Services;
using Brite.UWP.App.Helpers;
using Brite.UWP.App.Services;
using Brite.UWP.App.Views;

using Microsoft.Toolkit.Uwp.UI.Animations;

namespace Brite.UWP.App.ViewModels
{
    public class HomeViewModel : Observable
    {
        private ICommand _itemClickCommand;

        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new RelayCommand<SampleOrder>(OnItemClick));

        public ObservableCollection<SampleOrder> Source
        {
            get
            {
                // TODO WTS: Replace this with your actual data
                return SampleDataService.GetContentGridData();
            }
        }

        public HomeViewModel()
        {
        }

        private void OnItemClick(SampleOrder clickedItem)
        {
            if (clickedItem != null)
            {
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(clickedItem);
                NavigationService.Navigate<HomeDetailPage>(clickedItem.OrderId);
            }
        }
    }
}
