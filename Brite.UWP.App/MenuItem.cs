/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

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
