using System;
using System.Collections.Generic;
using Utils;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace WindowsApp2._0
{
    public sealed partial class EntryPage
    {

        #region Properties, varables and stuff

        Size _internalWindowSize;

        String loadFileToken;
        String saveFileToken;

        DataSelectionState _currentState = DataSelectionState.None;

        DataSelectionState CurrentState
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
                if (value == DataSelectionState.Open)
                    OpenPageOpen();
                else if (value == DataSelectionState.Connect)
                    ConnectPageOpen();
                else if (value == DataSelectionState.OpenView)
                    FileViewPageOpen();
                else if (value == DataSelectionState.ConnectView)
                    ConnectModulePageOpen();
                else
                    EntryViewOpen();
                Bindings.Update();
                DataSelectionAnimation.Begin();
                AdjustTitleBar();
            }
        }

        LayoutState CurrentLayout => DesiredSize.Width >= 720
                                                ? LayoutState.Wide
                                                : (DesiredSize.Width > 0
                                                    ? LayoutState.Narrow
                                                    : LayoutState.None);

        LayoutState _previousLayout;

        Double? ViewTranslateX => CurrentLayout == LayoutState.Wide
                                            ? (CurrentState == DataSelectionState.None
                                                ? -_internalWindowSize.Width / 2
                                                : (CurrentState == DataSelectionState.Connect || CurrentState == DataSelectionState.ConnectView
                                                    ? -_internalWindowSize.Width
                                                    : 0))
                                            : (CurrentState == DataSelectionState.None || CurrentState == DataSelectionState.Connect || CurrentState == DataSelectionState.Open
                                                ? -_internalWindowSize.Width
                                                : 0);

        Double? ViewTranslateY => CurrentLayout == LayoutState.Narrow
                                            ? (CurrentState == DataSelectionState.None
                                                ? -_internalWindowSize.Height / 2
                                                : (CurrentState == DataSelectionState.Connect || CurrentState == DataSelectionState.ConnectView
                                                    ? -_internalWindowSize.Height
                                                    : 0))
                                            : (CurrentState == DataSelectionState.OpenView || CurrentState == DataSelectionState.ConnectView
                                                ? -_internalWindowSize.Height
                                                : 0);

        List<DeviceInformation> Ports { get; set; } = new List<DeviceInformation>();

        Boolean PortAvailable => Ports?.Count > 0;

        #endregion

        #region Render stuff
        public EntryPage()
        {
            InitializeComponent();
            SizeChanged += (s, e) => Recompose(e.NewSize);
            Loaded += (s, e) =>
            {
                Recompose(DesiredSize);
                //! Change to DataSelectionState.None after debugging
                CurrentState = DataSelectionState.OpenView;
            };
            HideStatBar();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            ApplicationView.GetForCurrentView().TitleBar.ForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonPressedForegroundColor = Colors.White;
        }

        async void ShowStatBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await StatusBar.GetForCurrentView().ShowAsync();
            }
        }

        async void HideStatBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await StatusBar.GetForCurrentView().HideAsync();
            }
        }

        void Recompose(Size newSize)
        {
            _internalWindowSize = newSize;
            MainGrid.Width = 2 * _internalWindowSize.Width;
            MainGrid.Height = 2 * _internalWindowSize.Height;
            (MainGrid.RenderTransform as TranslateTransform).X = ViewTranslateX.Value;
            (MainGrid.RenderTransform as TranslateTransform).Y = ViewTranslateY.Value;
            if (CurrentLayout == _previousLayout) return;
            VisualStateManager.GoToState(this, CurrentLayout.ToString(), false);
            AdjustTitleBar();
            _previousLayout = CurrentLayout;
        }

        void AdjustTitleBar()
        {
            ApplicationView.GetForCurrentView().TitleBar.BackgroundColor =
                ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = CurrentLayout == LayoutState.Wide
                                                                                        ? (CurrentState == DataSelectionState.Open || CurrentState == DataSelectionState.OpenView
                                                                                            ? (OpenFileControl.Background as SolidColorBrush).Color
                                                                                            : (CurrentState == DataSelectionState.Connect || CurrentState == DataSelectionState.ConnectView)
                                                                                                ? (ConnectModuleControl.Background as SolidColorBrush).Color
                                                                                                : Colors.Black)
                                                                                        : (CurrentState == DataSelectionState.Open
                                                                                            ? (OpenFileOptions.Background as SolidColorBrush).Color
                                                                                            : (CurrentState == DataSelectionState.OpenView
                                                                                                ? (OpenFileControl.Background as SolidColorBrush).Color
                                                                                                : (CurrentState == DataSelectionState.Connect || CurrentState == DataSelectionState.ConnectView
                                                                                                    ? (ConnectModuleControl.Background as SolidColorBrush).Color
                                                                                                    : Colors.Black)));
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverBackgroundColor = ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor.Value.Lighten(30);
            ApplicationView.GetForCurrentView().TitleBar.ButtonPressedBackgroundColor = ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor.Value.Lighten(50);
        }

        #endregion

        #region Control stuff

        void GridControlManipulationCompleted(Object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Grid controlGrid = sender as Grid;
            if (controlGrid == null) return;

            CurrentState = controlGrid.Name == "OpenFileControl" &&
                            ((CurrentLayout == LayoutState.Wide && (e.Velocities.Linear.X > 0 || (e.Velocities.Linear.X == 0 && (MainGrid.RenderTransform as TranslateTransform).X >= -_internalWindowSize.Width / 4)))
                            || (CurrentLayout == LayoutState.Narrow && (e.Velocities.Linear.Y > 0 || (e.Velocities.Linear.Y == 0 && (MainGrid.RenderTransform as TranslateTransform).Y >= -_internalWindowSize.Height / 4))))
                            ? DataSelectionState.Open
                            : (controlGrid.Name == "ConnectModuleControl" && ((CurrentLayout == LayoutState.Wide && (e.Velocities.Linear.X < 0 || (e.Velocities.Linear.X == 0 && (MainGrid.RenderTransform as TranslateTransform).X <= 3 * -_internalWindowSize.Width / 4)))
                                || (CurrentLayout == LayoutState.Narrow && (e.Velocities.Linear.Y < 0 || (e.Velocities.Linear.Y == 0 && (MainGrid.RenderTransform as TranslateTransform).Y <= 3 * -_internalWindowSize.Height / 4))))
                                ? DataSelectionState.Connect
                                : DataSelectionState.None);
        }

        void GridControlManipulationDelta(Object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Grid controlGrid = sender as Grid;
            if (controlGrid == null) return;

            (MainGrid.RenderTransform as TranslateTransform).X = CurrentLayout == LayoutState.Wide ? MathUtils.Limit((MainGrid.RenderTransform as TranslateTransform).X + e.Delta.Translation.X, controlGrid.Name == "OpenFileControl" ? -_internalWindowSize.Width / 2 : -_internalWindowSize.Width, controlGrid.Name == "OpenFileControl" ? 0 : -_internalWindowSize.Width / 2) : -_internalWindowSize.Width;
            (MainGrid.RenderTransform as TranslateTransform).Y = CurrentLayout == LayoutState.Narrow ? MathUtils.Limit((MainGrid.RenderTransform as TranslateTransform).Y + e.Delta.Translation.Y, controlGrid.Name == "OpenFileControl" ? -_internalWindowSize.Height / 2 : -_internalWindowSize.Height, controlGrid.Name == "OpenFileControl" ? 0 : -_internalWindowSize.Height / 2) : 0;

            if (e.IsInertial) e.Complete();
            e.Handled = true;
        }

        void GridControlTapped(Object sender, TappedRoutedEventArgs e)
        {
            CurrentState = (sender as Grid)?.Name == "OpenFileControl"
                               ? (CurrentState == DataSelectionState.None
                                      ? DataSelectionState.Open
                                      : DataSelectionState.None)
                               : (CurrentState == DataSelectionState.None
                                      ? DataSelectionState.Connect
                                      : DataSelectionState.None);
        }

        #endregion

        #region Navigation stuff
        void EntryViewOpen()
        {

        }

        void OpenPageOpen()
        {

        }

        void ConnectPageOpen()
        {
            LoadSerialPorts();
        }
        void FileViewPageOpen()
        {
            OpeningFileIconAnimation.Begin();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += GoBackToDataSelect;
            OpeningFileIconAnimation.Stop();
            OpeningFileLoader.Visibility = Visibility.Collapsed;
            FileDataView.Visibility = Visibility.Visible;
        }
        void ConnectModulePageOpen()
        {
            ConnectingModuleIconAnimation.Begin();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += GoBackToDataSelect;
        }

        void GoBackToDataSelect(object o, BackRequestedEventArgs e)
        {
            CurrentState = CurrentState == DataSelectionState.OpenView ? DataSelectionState.Open : (CurrentState == DataSelectionState.ConnectView ? DataSelectionState.Connect : DataSelectionState.None);
            OpeningFileIconAnimation.Stop();
            ConnectingModuleIconAnimation.Stop();
            SystemNavigationManager.GetForCurrentView().BackRequested -= GoBackToDataSelect;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        void CancelDataSelection(Object sender, TappedRoutedEventArgs e) => CurrentState = DataSelectionState.None;

        void ShowDataRequested(Object sender, TappedRoutedEventArgs e)
        {
            Button buttonObject = sender as Button;
            if (buttonObject == null) return;
            CurrentState = (String) buttonObject.Content == "Open" ? DataSelectionState.OpenView : ((String) buttonObject.Content == "Connect" ? DataSelectionState.ConnectView : DataSelectionState.None);
        }

        #endregion

        #region Serial stuff

        void RefreshRequested(Object sender, TappedRoutedEventArgs e) => LoadSerialPorts();

        async void LoadSerialPorts()
        {
            Ports.Clear();
            var selector = SerialDevice.GetDeviceSelector();
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);
            foreach (DeviceInformation di in devices)
            {
                Ports.Add(di);
            }
            Bindings.Update();
            PortSelector.SelectedIndex = Ports.Count - 1;
        }

        #endregion

        #region File stuff

        async void BrowseOpenTapped(Object sender, TappedRoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = {
                    ".csv",
                    ".txt",
                    ".csmes"
                }
            };
            StorageFile loadFile = await openPicker.PickSingleFileAsync();
            if (loadFile == null) return;
            loadFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(loadFile);
            SelectedFileLabel.Text = loadFile.Path;
            ToolTipService.SetToolTip(SelectedFileLabel, loadFile.Path);
        }

        #endregion
    }

    #region Enum (no stuff in this one yet)

    enum DataSelectionState
    {
        Open,
        Connect,
        OpenView,
        ConnectView,
        None
    }

    enum LayoutState
    {
        None,
        Wide,
        Narrow
    }
    #endregion
}
