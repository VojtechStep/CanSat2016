using WindowsCode.Pages;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Diagnostics;
using WindowsCode.Classes;

namespace WindowsCode
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {


            MainPage mainPage = Window.Current.Content as MainPage;
            Frame rootFrame = null;

            if (mainPage == null)
            {
                mainPage = new MainPage();

                if (rootFrame == null)
                {
                    rootFrame = new Frame();
                    rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                    rootFrame.NavigationFailed += OnNavigationFailed;
                    rootFrame.Navigated += OnNavigated;
                    rootFrame.Navigated += mainPage.Frame_Navigated;

                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {

                    }
                }

                mainPage.DataContext = rootFrame;

                ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
                ApplicationDataContainer CanSatSettings = roamingSettings.CreateContainer("CanSatSettings", ApplicationDataCreateDisposition.Always);
                if (roamingSettings.Containers.ContainsKey("CanSatSettings"))
                {
                    if (roamingSettings.Containers["CanSatSettings"].Values.ContainsKey("IntTheme"))
                        mainPage.RequestedTheme = (Int32)roamingSettings.Containers["CanSatSettings"].Values["IntTheme"] == 0 ? ElementTheme.Default : ((Int32)roamingSettings.Containers["CanSatSettings"].Values["IntTheme"] == 1 ? ElementTheme.Dark : ElementTheme.Light);
                }


                Window.Current.Content = mainPage;

                if (ApiInformation.IsApiContractPresent("Windows.Phone.UI.Input.HardwareButtons", 1, 0))
                    Windows.Phone.UI.Input.HardwareButtons.BackPressed += BackRequested;

                SystemNavigationManager.GetForCurrentView().BackRequested += BackRequested;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;



                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MesurementsPage), e.Arguments);

                }
                // Ensure the current window is active
                Window.Current.Activate();

            }

        }

        private void BackRequested(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            GoBack();
            e.Handled = true;
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            GoBack();
            e.Handled = true;
        }

        public void GoBack()
        {
            MainPage mainPage = Window.Current.Content as MainPage;
            Frame rootFrame = mainPage.DataContext as Frame;
            if (rootFrame.CanGoBack)
            {
                rootFrame?.GoBack();
            }
            else
            {
                Exit();
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
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
            if(!DataState.ReadCancellationTokenSource?.IsCancellationRequested ?? false) DataState.ReadCancellationTokenSource?.Cancel();
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            ApplicationDataContainer CanSatSettings = roamingSettings.CreateContainer("CanSatSettings", ApplicationDataCreateDisposition.Always);
            roamingSettings.Containers["CanSatSettings"].Values["IntTheme"] = (Window.Current.Content as MainPage).RequestedTheme == ElementTheme.Default ? 0 : ((Window.Current.Content as MainPage).RequestedTheme == ElementTheme.Dark ? 1 : 2);

            deferral.Complete();
        }
    }
}
