using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using Brite.App.Win.Commands;
using Brite.App.Win.Extensions;
using Brite.App.Win.Services;
using DynamicData;
using MahApps.Metro.Controls;

namespace Brite.App.Win.ViewModels
{
    public class ShellViewModel : BaseViewModel, IShellViewModel
    {
        private readonly INavigationService _navigationService;

        public string Title { get; }

        public IEnumerable<IChildViewModel> Pages { get; }

        public int CurrentPageIndex
        {
            get => Pages.Where(p => p is IPageViewModel).IndexOf(CurrentPage);
            set
            {
                if (value > -1)
                    OnNavigateTo(value);
            }
        }

        public int CurrentOptionPageIndex
        {
            get => Pages.Where(p => p is IOptionPageViewModel).IndexOf(CurrentPage);
            set
            {
                if (value > -1)
                    OnNavigateTo(value);
            }
        }

        public IEnumerable<HamburgerMenuIconItem> MainPageItems => Pages.Where(p => p is IPageViewModel)
            .Select(p => new HamburgerMenuIconItem { Label = p.Title, Icon = p.Icon, Tag = p });
        public IEnumerable<HamburgerMenuIconItem> OptionPageItems => Pages.Where(p => p is IOptionPageViewModel)
            .Select(p => new HamburgerMenuIconItem { Label = p.Title, Icon = p.Icon, Tag = p });

        public IChildViewModel CurrentPage { get; private set; }
        public bool CanGoBack => _navigationService.CanGoBack;
        public bool CanGoForward => _navigationService.CanGoForward;
        public ReactiveCommand GoBackCommand { get; }
        public ReactiveCommand GoForwardCommand { get; }

        // TODO: Implement overlay
        public bool HasOverlay => Overlay != null;
        public string OverlayTitle { get; private set; }
        public BaseViewModel Overlay { get; private set; }
        public ReactiveCommand CloseOverlayCommand { get; }

        public ShellViewModel(string title, IEnumerable<IChildViewModel> pages, INavigationService navigationService, IOverlayService overlayService)
        {
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

            // TODO: Overlay

            // Default page
            OnNavigateTo(0);
        }

        private void OnNavigateBack(object obj)
        {
            _navigationService.NavigateBack();
        }

        private void OnNavigateForward(object obj)
        {
            _navigationService.NavigateForward();
        }

        private void OnNavigateTo(int index)
        {
            _navigationService.NavigateTo(Pages.ElementAt(index));
        }

        private void OnNavigated(IChildViewModel page)
        {
            CurrentPage = page;

            Debug.WriteLine("Navigated to " + CurrentPage);

            this.RaisePropertyChanged(() => CurrentPageIndex);
            this.RaisePropertyChanged(() => CurrentOptionPageIndex);

            this.RaisePropertyChanged(() => CurrentPage);
            this.RaisePropertyChanged(() => CanGoBack);
            this.RaisePropertyChanged(() => CanGoForward);
        }
    }
}
