using System;
using System.Collections.Generic;
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
            get => PageIndex<IPageViewModel>();
            set => PageNavigate<IPageViewModel>(value);
        }

        public int CurrentOptionPageIndex
        {
            get => PageIndex<IOptionPageViewModel>();
            set => PageNavigate<IOptionPageViewModel>(value);
        }

        public IEnumerable<HamburgerMenuIconItem> MainPageItems => Pages.Where(p => p is IPageViewModel)
            .Select(p => new HamburgerMenuIconItem { Label = p.Title, Icon = p.Icon, Tag = p });
        public IEnumerable<HamburgerMenuIconItem> OptionPageItems => Pages.Where(p => p is IOptionPageViewModel)
            .Select(p => new HamburgerMenuIconItem { Label = p.Title, Icon = p.Icon, Tag = p });

        public IChildViewModel CurrentPage { get; private set; }
        public bool CanGoBack => _navigationService.CanGoBack;
        public bool CanGoForward => _navigationService.CanGoForward;
        public ReactiveCommand GoBackCommand { get; }

        // TODO: Implement overlay
        public bool HasOverlay => Overlay != null;
        public string OverlayTitle { get; private set; }
        public BaseViewModel Overlay { get; private set; }
        public ReactiveCommand CloseOverlayCommand { get; }

        public ShellViewModel(string title, IOrderedEnumerable<IChildViewModel> pages, INavigationService navigationService, IOverlayService overlayService)
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

            // TODO: Overlay

            // Default page
            if (Pages.Any())
                PageNavigate<IPageViewModel>(0);
        }

        private int PageIndex<T>()
            where T : IChildViewModel
        {
            return Pages.Where(p => p is T).IndexOf(CurrentPage);
        }

        private void PageNavigate<T>(int index)
            where T : IChildViewModel
        {
            if (index > -1 && index < Pages.Count())
                _navigationService.NavigateTo(Pages.Where(p => p is T).ElementAt(index));
        }

        private void OnNavigateBack(object obj)
        {
            _navigationService.NavigateBack();
        }

        private void OnNavigated(IChildViewModel page)
        {
            CurrentPage = page;

            this.RaisePropertyChanged(() => CurrentPageIndex);
            this.RaisePropertyChanged(() => CurrentOptionPageIndex);

            this.RaisePropertyChanged(() => CurrentPage);
            this.RaisePropertyChanged(() => CanGoBack);
            this.RaisePropertyChanged(() => CanGoForward);
        }
    }
}
