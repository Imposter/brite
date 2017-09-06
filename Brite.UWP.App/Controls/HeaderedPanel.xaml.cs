using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Brite.UWP.App.Controls
{
    public sealed partial class HeaderedPanel : UserControl
    {
        public string Title
        {
            get => TextBlock.Text;
            set => TextBlock.Text = value;
        }

        public double TitleHeight
        {
            get => TextBlock.Height;
            set => TextBlock.Height = value;
        }

        public new object Content
        {
            get => ContentControl.Content;
            set => ContentControl.Content = value;
        }

        public new FontFamily FontFamily
        {
            get => TextBlock.FontFamily;
            set => TextBlock.FontFamily = value;
        }

        public new double FontSize
        {
            get => TextBlock.FontSize;
            set => TextBlock.FontSize = value;
        }

        public new FontWeight FontWeight
        {
            get => TextBlock.FontWeight;
            set => TextBlock.FontWeight = value;
        }

        public HeaderedPanel()
        {
            InitializeComponent();
        }
    }
}
