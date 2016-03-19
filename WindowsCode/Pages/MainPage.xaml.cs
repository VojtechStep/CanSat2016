using Windows.UI;
using static Windows.UI.Colors;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.Foundation.Metadata;
using WindowsCode.Pages;
using System;
using WindowsCode.CustomControls;
using WindowsCode.Classes;
using Windows.UI.Xaml.Media;

namespace WindowsCode
{
    public sealed partial class MainPage : Page
    {
        public DeviceFamily CurrentDeviceFamily
        {
            get
            {
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Contains("Desktop"))
                    return DeviceFamily.Desktop;
                return DeviceFamily.Mobile;
            }
        }

        private Visibility _dataTabVisibility = Visibility.Collapsed;

        public enum DeviceFamily
        {
            Mobile,
            Desktop
        }

        public MainPage()
        {
            InitializeComponent();
            HamburgerMenu.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, IsPaneOpenPropertyChanged);
            SizeChanged += HandleSizeChange;
        }


        private void HandleSizeChange(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width >= 720)
            {
                VisualStateManager.GoToState(this, "Wide", false);
                HideStatusBar();
            }
            else
            {
                VisualStateManager.GoToState(this, "Narrow", false);
                ShowStatusBar();
            }
        }

        private async void ShowStatusBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusbar = StatusBar.GetForCurrentView();
                await statusbar.ShowAsync();
                statusbar.BackgroundColor = (Color)Resources["SystemAccentColor"];
                statusbar.BackgroundOpacity = 1;
                statusbar.ForegroundColor = White;
            }
        }

        private async void HideStatusBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusbar = StatusBar.GetForCurrentView();
                await statusbar.HideAsync();
            }
        }

        public bool IsPaneOpen
        {
            get { return HamburgerMenu.IsPaneOpen; }
            set { HamburgerMenu.IsPaneOpen = value; }
        }


        private void IsPaneOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            HamburgerMenu.OpenPaneLength = (Double)Resources["OpenPaneLengthPercentage"] * ActualWidth;
            foreach (HamburgerMenuItem hmi in HMRelativePanel.Children)
            {
                hmi.Width = HamburgerMenu.OpenPaneLength;
            }
        }

        public void Frame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            String PageTypeFullName = (DataContext as Frame).Content.GetType().Name;
            String PageTypeName = PageTypeFullName.Substring(0, PageTypeFullName.Length - "Page".Length);

            foreach (HamburgerMenuItem hmi in HMRelativePanel.Children)
            {
                hmi.Selected = hmi.MenuItemLabel == PageTypeName;
            }
            PageTitle.Text = PageTypeName.ToUpper();
        }

        private void HamburgerToggle_Click(object sender, RoutedEventArgs e)
        {
            HamburgerMenu.IsPaneOpen = !HamburgerMenu.IsPaneOpen;
        }

        public void GoToPage(Type PageType)
        {
            GoToPage(PageType, null);
        }

        public Visibility DataTabVisibility
        {
            get { return _dataTabVisibility; }
            set
            {
                _dataTabVisibility = value;
                Bindings.Update();
            }
        }

        public void GoToPage(Type PageType, object args)
        {
            Frame frame = this.DataContext as Frame;
            Page page = frame?.Content as Page;
            page.Background = new SolidColorBrush(Colors.Navy);
            if (page?.GetType() != PageType)
            {
                frame.Navigate(PageType, args);
            }
        }

        private void MesurementsItem_Click(object sender, RoutedEventArgs e)
        {
            GoToPage(typeof(MesurementsPage));
        }

        private void SettingsItem_Click(object sender, RoutedEventArgs e)
        {
            GoToPage(typeof(SettingsPage));
        }

        private void DataItem_Click(object sender, RoutedEventArgs e)
        {
            GoToPage(typeof(DataPage));

        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if(!DataState.ReadCancellationTokenSource.IsCancellationRequested) DataState.ReadCancellationTokenSource.Cancel();
        }

        private async void StartMesurement_Click(Object sender, RoutedEventArgs e)
        {
            await Communication.WriteAsync(0x67);
            StartMesurement.Visibility = Visibility.Collapsed;
            StopMesurement.Visibility = Visibility.Visible;
        }

        private async void RequestSample_Click(Object sender, RoutedEventArgs e)
        {
            await Communication.WriteAsync(0x69);
        }

        private async void StopMesurement_Click(Object sender, RoutedEventArgs e)
        {
            await Communication.WriteAsync(0x68);
            StopMesurement.Visibility = Visibility.Collapsed;
            StartMesurement.Visibility = Visibility.Visible;
        }
    }

}
