/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using Brite.API;
using Brite.Utility.IO;
using Brite.UWP.App.Views;
using Brite.UWP.Core.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Brite.API.Client;
using Microsoft.Toolkit.Uwp.Helpers;

namespace Brite.UWP.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Check if the application is already running
            if (Global.Running)
                Exit();

            // Set as running
            Global.Running = true;

            // Initialize logger
            var localFolder = ApplicationData.Current.LocalFolder;

            var logFile = await localFolder.CreateFileAsync("brite.log", CreationCollisionOption.ReplaceExisting);
            var logger = new FileLogger(logFile);
            Logger.SetInstance(logger);

            // Read config
            var configFile = await localFolder.CreateFileAsync("config.json", CreationCollisionOption.OpenIfExists);
            // TODO: If empty, create defaults

            var config = await FileIO.ReadTextAsync(configFile);
            Global.Config = JsonConvert.DeserializeObject<Config>(config);

            // Initialize brite client
            Global.TcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(Global.Config.ServerIpAddress), Global.Config.ServerPort));
            Global.BriteClient = new BriteClient(Global.TcpClient, "Brite.UWP.App");

            // Connect to server
            await Global.BriteClient.ConnectAsync(); // TODO: Catch exceptions
            // TODO: Add class for management of network and brite client

            // Get window root frame
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // NOTE: We probably won't need this
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application

                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(RootFrame), e.Arguments);

                    // Store inner frame in global variables
                    Global.RootFrame = rootFrame.Content as RootFrame;

                    // Initialize menu
                    // ReSharper disable once PossibleNullReferenceException
                    Global.RootFrame.MenuItems.AddRange(new[]
                    {
                        new MenuItem(Symbol.Home, "Home", typeof(HomeView))
                    });

                    Global.RootFrame.MenuOptionItems.AddRange(new[]
                    {
                        new MenuItem(Symbol.Setting, "Settings", typeof(SettingsView)),
                        new MenuItem(Symbol.Upload, "Upgrade Firmware", null) // TODO: ...
                    });

                    // Navigate to home view
                    Global.RootFrame.Navigate(typeof(HomeView));
                }

                // Register a global back event handler. This can be registered on a per-page-bases if you only have a subset of your pages
                // that needs to handle back or if you want to do page-specific logic before deciding to navigate back on those pages.
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when a user issues a global back on the device.
        /// If the app has no in-app back stack left for the current view/frame the user may be navigated away
        /// back to the previous app in the system's app back stack or to the start screen.
        /// In windowed mode on desktop there is no system app back stack and the user will stay in the app even when the in-app back stack is depleted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            var frame = Global.RootFrame;
            if (frame == null)
                return;

            // If we can go back and the event has not already been handled, do so.
            if (frame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                frame.GoBack();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
