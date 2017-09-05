using System;
using Windows.UI.Xaml.Controls;

namespace Brite.UWP.App
{
    public class MenuItem
    {
        public Symbol Icon { get; set; }
        public string Name { get; set; }
        public Type PageType { get; set; }

        public MenuItem(Symbol icon, string name, Type pageType)
        {
            Icon = icon;
            Name = name;
            PageType = pageType;
        }
    }
}
