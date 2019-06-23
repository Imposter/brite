using Brite.App.Win.ViewModels;

namespace Brite.App.Win.Models
{
    public sealed class Message
    {
        public string Title { get; }
        public ICloseableViewModel ViewModel { get; }

        public Message(string title, ICloseableViewModel viewModel)
        {
            Title = title;
            ViewModel = viewModel;
        }
    }
}
