using System;
using System.Reflection;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Brite.UWP.App.Common;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
namespace Brite.UWP.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootFrame : Page
    {
        public ICommand NavigateCommand => new DelegateCommand(p => Navigate((Type)p));

        public RootFrame()
        {
            InitializeComponent();
        }

        public void Navigate(Type targetPageType)
        {
            var frame = Content as Frame;
            frame?.Navigate(targetPageType);
        }
    }
}
