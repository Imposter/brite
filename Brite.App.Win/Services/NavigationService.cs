using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Brite.App.Win.Models;
using Brite.App.Win.ViewModels;
using Nito.Collections;

namespace Brite.App.Win.Services
{
    public sealed class NavigationService : DisposableObject, INavigationService
    {
        private readonly Deque<IChildViewModel> _previousPages;
        private readonly Deque<IChildViewModel> _nextPages;

        private IChildViewModel _currentPage;
        private readonly Subject<IChildViewModel> _page;
        
        public bool CanGoBack => _previousPages.Count > 0;
        public bool CanGoForward => _nextPages.Count > 0;
        public IObservable<IChildViewModel> Page => _page;

        public NavigationService()
        {
            _previousPages = new Deque<IChildViewModel>();
            _nextPages = new Deque<IChildViewModel>();
            _page = new Subject<IChildViewModel>().DisposeWith(this);
        }

        public void NavigateTo(IChildViewModel viewModel)
        {
            // Add current page to previous history
            _previousPages.AddToFront(_currentPage);

            // Clear next history
            _nextPages.Clear();

            _currentPage = viewModel;

            _page.OnNext(_currentPage);
        }

        public void NavigateBack()
        {
            if (!CanGoBack) return;

            // Add current page to next history
            _nextPages.AddToFront(_currentPage);

            // Take last page from previous history
            var p = _previousPages.RemoveFromFront();

            _currentPage = p;

            _page.OnNext(_currentPage);
        }

        public void NavigateForward()
        {
            if (!CanGoForward) return;

            // Add current page to previous history
            _previousPages.AddToFront(_currentPage);

            // Take last page from next history
            var p = _nextPages.RemoveFromFront();

            _currentPage = p;

            _page.OnNext(_currentPage);
        }

        public void ClearHistory()
        {
            _nextPages.Clear();
            _previousPages.Clear();
        }
    }
}
