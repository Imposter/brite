using System;
using Brite.App.Win.ViewModels;

namespace Brite.App.Win.Services
{
    public interface INavigationService : IService
    {
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        IObservable<IChildViewModel> Page { get; }

        void NavigateTo(IChildViewModel viewModel);
        void NavigateBack();
        void NavigateForward();

        void ClearHistory();
    }
}
