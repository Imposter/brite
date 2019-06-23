using Brite.App.Win.Models;
using Brite.App.Win.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Markup;

namespace Brite.App.Win.Views
{
    [ContentProperty("DialogBody")]
    public sealed class MessageDialog : BaseMetroDialog
    {
        private readonly Message _message;

        public MessageDialog(Message message)
        {
            _message = message;

            Title = _message.Title;
            Content = _message.ViewModel;
        }

        public ICloseableViewModel CloseableContent => _message.ViewModel;
    }
}
