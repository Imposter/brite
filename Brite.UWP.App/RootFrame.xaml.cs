/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using Brite.UWP.App.Core;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace Brite.UWP.App
{
    public sealed partial class RootFrame : Page
    {
        public List<MenuItem> MenuItems { get; }
        public List<MenuItem> MenuOptionItems { get; }
        public bool CanGoBack => ContentFrame.CanGoBack;
        public bool CanGoForward => ContentFrame.CanGoForward;

        public RootFrame()
        {
            InitializeComponent();

            MenuItems = new List<MenuItem>();
            MenuOptionItems = new List<MenuItem>();

            Menu.ItemsSource = MenuItems;
            Menu.OptionsItemsSource = MenuOptionItems;
        }

        private void Menu_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MenuItem;
            if (ContentFrame.Content != null && item.PageType == ContentFrame.Content.GetType())
                return;

            Header.Text = item.Name;
            HeaderStoryboard.Begin();
            if (!ContentFrame.Navigate(item.PageType, null))
                throw new Exception("Navigation failed");

            Menu.StartBringIntoView();
        }

        public void Navigate(Type pageType, object parameter = null)
        {
            if (ContentFrame.Content != null && pageType == ContentFrame.Content.GetType())
                return;
            
            if (!ContentFrame.Navigate(pageType, NavigationStore.Store(parameter)))
                throw new Exception("Navigation failed");

            Menu.StartBringIntoView();
            UpdateSelectedMenuItem(pageType);
        }

        public void GoBack()
        {
            ContentFrame.GoBack();
            UpdateSelectedMenuItem(ContentFrame.Content.GetType());
        }

        public void GoForward()
        {
            ContentFrame.GoBack();
            UpdateSelectedMenuItem(ContentFrame.Content.GetType());
        }

        private void UpdateSelectedMenuItem(Type pageType)
        {
            Header.Text = string.Empty;
            var index = MenuItems.FindIndex(i => i.PageType == pageType);
            if (index != -1)
            {
                Menu.SelectedIndex = index;
                Header.Text = MenuItems[index].Name;
            }
            else Menu.SelectedIndex = -1;

            index = MenuOptionItems.FindIndex(i => i.PageType == pageType);
            if (index != -1)
            {
                Menu.SelectedOptionsIndex = index;
                Header.Text = MenuOptionItems[index].Name;
            }
            else Menu.SelectedOptionsIndex = -1;
            HeaderStoryboard.Begin();
        }
    }
}
