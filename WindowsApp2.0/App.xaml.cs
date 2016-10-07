using System;
using System.Threading;
using Microsoft.HockeyApp;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WindowsApp2._0
{
    sealed partial class App : Application
    {
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            HockeyClient.Current.Configure("898dfc3f60054df2997a787deeaacddb");
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            var rootFrame = Init();
            if (!args.PrelaunchActivated)
            {
                if (rootFrame.Content == null) rootFrame.Navigate(typeof(EntryPage));
                Window.Current.Activate();
            }
        }

        Frame Init()
        {
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }
            return rootFrame;
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            var rootFrame = Init();
            if (rootFrame.Content == null) rootFrame.Navigate(typeof(EntryPage));
            Window.Current.Activate();
            await Task.Delay(1000);
            var current = rootFrame.Content as EntryPage;
            current.UpdateEnvironmentWithFile(args.Files[0]);
            current.CurrentState = DataSelectionState.OpenView;
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if(args.Kind == ActivationKind.Protocol)
            {
                var prtArgs = args as ProtocolActivatedEventArgs;
                //TODO: Use the protocol data
                var rootFrame = Init();
                if (rootFrame.Content == null) rootFrame.Navigate(typeof(EntryPage));
                Window.Current.Activate();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
