using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Brite.UWP.App.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeView : Page
    {
        public HomeView()
        {
            InitializeComponent();

            // Cache home page
            NavigationCacheMode = NavigationCacheMode.Required;

            // TODO: Instead of setting stuff like this, use a ViewModel and set the item source as a property in the view model
            Menu.ItemsSource = new List<MenuItem>()
            {
                new MenuItem { Icon = Symbol.Home, Name = "Home", PageType = typeof(HomeView) },
                new MenuItem { Icon = Symbol.Setting, Name = "Settings", PageType = typeof(SettingsView) }
            };

            Menu.OptionsItemsSource = new List<MenuItem>()
            {
                new MenuItem { Icon = Symbol.Setting, Name = "Settings", PageType = typeof(SettingsView) },
                new MenuItem { Icon = Symbol.Upload, Name = "Upgrade Firmware", PageType = typeof(SettingsView) }
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set as the current page
            Menu.SelectedItem = ((List<MenuItem>)Menu.ItemsSource)[0];

            // This page is always at the top of our in-app back stack.
            // Once it is reached there is no further back so we can always disable the title bar back UI when navigated here.
            // If you want to you can always to the Frame.CanGoBack check for all your pages and act accordingly.
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void Menu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MenuItem;
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(item.PageType);
        }
    }

    public class MenuItem
    {
        public Symbol Icon { get; set; }
        public string Name { get; set; }
        public Type PageType { get; set; }
    }
}
