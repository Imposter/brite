using System.Collections.Generic;
using Brite.App.Win.Commands;

namespace Brite.App.Win.ViewModels
{
    public interface IShellViewModel : IViewModel
    {
        string Title { get; }

        IEnumerable<IChildViewModel> Pages { get; }

        IChildViewModel CurrentPage { get; }
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        ReactiveCommand GoBackCommand { get; }
        ReactiveCommand GoForwardCommand { get; }
        ReactiveCommand GoToCommand { get; }

        bool HasOverlay { get; }
        string OverlayTitle { get; }
        BaseViewModel Overlay { get; }
        ReactiveCommand CloseOverlayCommand { get; }
    }
}
