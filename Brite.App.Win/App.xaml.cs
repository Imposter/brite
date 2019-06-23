using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Autofac;
using Autofac.Core;
using Brite.App.Win.Services;
using Brite.App.Win.ViewModels;
using Brite.App.Win.Views;
using Brite.Utility.IO;

namespace Brite.App.Win
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Log Log = Logger.GetLog<App>();

        private IMessageService _messageService;
        private ISchedulerService _schedulerService;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        // TODO: Update
        protected override void OnStartup(StartupEventArgs e)
        {
            // Get application title
            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute));
            var assemblyTitleAttribute = attributes.SingleOrDefault() as AssemblyTitleAttribute;

            // Initialize bootstrapper
            Bootstrapper.Initialize(assemblyTitleAttribute?.Title);

            _messageService = Bootstrapper.Resolve<IMessageService>();
            _schedulerService = Bootstrapper.Resolve<ISchedulerService>();

            var window = new ShellWindow(_schedulerService, _messageService) { DataContext = Bootstrapper.RootVisual };

            // TODO: Cancel event, only shutdown on NotifyIcon exit
            window.Closed += (s, a) =>
            {
                Bootstrapper.Shutdown();
            };

            Current.Exit += async (s, a) =>
            {
                // TODO: Any remaining shutdown procedures
            };

            window.Show();

            base.OnStartup(e);
        }

        private async void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            await Log.ErrorAsync("Unhandled app domain exception");
            HandleException(args.ExceptionObject as Exception);
        }

        private async void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            await Log.ErrorAsync("Unhandled dispatcher thread exception");
            args.Handled = true;

            HandleException(args.Exception);
        }

        private async void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            await Log.ErrorAsync("Unhandled task exception");
            args.SetObserved();

            HandleException(args.Exception.GetBaseException());
        }

        private async void HandleException(Exception exception)
        {
            await Log.ErrorAsync($"Unhandled Exception: {exception}");

            _schedulerService.Dispatcher.Schedule(() =>
            {
                var parameters = new Parameter[] { new NamedParameter("exception", exception) };
                var viewModel = Bootstrapper.Resolve<IExceptionViewModel>(parameters);

                viewModel.Closed
                    .Take(1)
                    .Subscribe(x => viewModel.Dispose());

                _messageService.Post("Well, that's embarassing...", viewModel);
            });
        }
    }
}
