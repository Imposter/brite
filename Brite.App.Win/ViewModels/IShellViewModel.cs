using System.Collections.Generic;

namespace Brite.App.Win.ViewModels
{
    public interface IShellViewModel : IViewModel
    {
        string Title { get; }

        IEnumerable<IChildViewModel> Pages { get; }

        IChildViewModel CurrentPage { get; }
        bool CanGoBack { get; }
        bool CanGoForward { get; }

        bool HasOverlay { get; }
        string OverlayTitle { get; }
        BaseViewModel Overlay { get; }
    }
}
