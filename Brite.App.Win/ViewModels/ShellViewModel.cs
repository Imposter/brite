using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using Brite.App.Win.Commands;
using Brite.App.Win.Extensions;
using Brite.App.Win.Services;
using MahApps.Metro.Controls;

namespace Brite.App.Win.ViewModels
{
    public class ShellViewModel : BaseViewModel, IShellViewModel
    {
        private readonly INavigationService _navigationService;

        public string Title { get; }

        public IEnumerable<IChildViewModel> Pages { get; }

        // TODO: How do we create a control from a view model?
        public IEnumerable<HamburgerMenuIconItem> MainPages => Pages.Where(p => p is IPageViewModel)
            .Select(p => new HamburgerMenuIconItem { Label = p.Title, Icon = p.Icon, Tag = p });
        public IEnumerable<HamburgerMenuIconItem> OptionPages => Pages.Where(p => p is IOptionPageViewModel)
            .Select(p => new HamburgerMenuIconItem { Label = p.Title, Icon = p.Icon, Tag = p });

        public IChildViewModel CurrentPage { get; private set; }
        public bool CanGoBack => _navigationService.CanGoBack;
        public bool CanGoForward => _navigationService.CanGoForward;
        public ReactiveCommand GoBackCommand { get; }
        public ReactiveCommand GoForwardCommand { get; } // TODO: Test Navigation
        public ReactiveCommand GoToCommand { get; }

        // TODO: Implement overlay
        public bool HasOverlay => Overlay != null;
        public string OverlayTitle { get; private set; }
        public BaseViewModel Overlay { get; private set; }
        public ReactiveCommand CloseOverlayCommand { get; }

        // TODO: Pass application global properties through constructor? because we can't pass vars like this using AutoFac
        public ShellViewModel(string title, IEnumerable<IChildViewModel> pages, INavigationService navigationService, IOverlayService overlayService)
        {
            // TODO: Temp
            Title = title;

            Pages = pages;

            // Subscribe to the navigation service
            _navigationService = navigationService;
            _navigationService.Page
                .Subscribe(OnNavigated)
                .DisposeWith(this);

            GoBackCommand = ReactiveCommand.Create()
                .DisposeWith(this);
            GoBackCommand.Subscribe(OnNavigateBack)
                .DisposeWith(this);

            GoForwardCommand = ReactiveCommand.Create()
                .DisposeWith(this);
            GoForwardCommand.Subscribe(OnNavigateForward)
                .DisposeWith(this);

            GoToCommand = ReactiveCommand.Create()
                .DisposeWith(this);
            GoToCommand.Subscribe(OnNavigateTo)
                .DisposeWith(this);

            // TODO: Overlay
        }

        private void OnNavigateBack(object obj)
        {
            _navigationService.NavigateBack();
        }

        private void OnNavigateForward(object obj)
        {
            _navigationService.NavigateBack();
        }

        private void OnNavigateTo(object obj)
        {
            if (obj is HamburgerMenuItem item)
            {
                _navigationService.NavigateTo((IChildViewModel) item.Tag);
            }
        }

        private void OnNavigated(IChildViewModel page)
        {
            CurrentPage = page;
            NotifyNavigated();
        }

        private void NotifyNavigated()
        {
            this.RaisePropertyChanged(() => CurrentPage);
            this.RaisePropertyChanged(() => CanGoBack);
            this.RaisePropertyChanged(() => CanGoForward);
        }
    }
}
