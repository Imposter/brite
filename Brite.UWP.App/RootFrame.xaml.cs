using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Brite.UWP.App
{
    public sealed partial class RootFrame : Page
    {
        public List<MenuItem> MenuItems { get; }
        public List<MenuItem> MenuOptionItems { get; }
        public bool CanGoBack => _frame.CanGoBack;
        public bool CanGoForward => _frame.CanGoForward;

        private readonly Frame _frame;

        public RootFrame()
        {
            InitializeComponent();

            _frame = new Frame();

            MenuItems = new List<MenuItem>();
            MenuOptionItems = new List<MenuItem>();

            Menu.ItemsSource = MenuItems;
            Menu.OptionsItemsSource = MenuOptionItems;
            InnerContent.Content = _frame;
        }

        private void Menu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MenuItem;
            if (!_frame.Navigate(item.PageType, null))
                throw new Exception("Navigation failed");

            Menu.StartBringIntoView();
        }

        public void Navigate(Type pageType)
        {
            if (!_frame.Navigate(pageType, null))
                throw new Exception("Navigation failed");

            Menu.StartBringIntoView();
            UpdateSelectedMenuItem(pageType);
        }

        public void GoBack()
        {
            _frame.GoBack();
            UpdateSelectedMenuItem(_frame.Content.GetType());
        }

        public void GoForward()
        {
            _frame.GoBack();
            UpdateSelectedMenuItem(_frame.Content.GetType());
        }

        private void UpdateSelectedMenuItem(Type pageType)
        {
            var index = MenuItems.FindIndex(i => i.PageType == pageType);
            if (index != -1) Menu.SelectedIndex = index;
            else Menu.SelectedIndex = -1;

            index = MenuOptionItems.FindIndex(i => i.PageType == pageType);
            if (index != -1) Menu.SelectedOptionsIndex = index;
            else Menu.SelectedOptionsIndex = -1;
        }
    }
}
