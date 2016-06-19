using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using WindowsApp2._0.Controls;

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
                Bindings.Update();
                DataSelectionAnimation.Begin();
                AdjustTitleBar();
                HintRight.Stop();
                HintLeft.Stop();
                HintDown.Stop();
                HintUp.Stop();
                switch (value)
                {
                    case DataSelectionState.Open:
                        OpenPageOpen();
                        break;
                    case DataSelectionState.Connect:
                        ConnectPageOpen();
                        break;
                    case DataSelectionState.OpenView:
                        FileViewPageOpen();
                        break;
                    case DataSelectionState.ConnectView:
                        ConnectModulePageOpen();
                        break;
                    default:
                        EntryViewOpen();
                        (CurrentState == DataSelectionState.None ? (CurrentLayout == LayoutState.Wide ? HintRight : HintDown) : NullAnimation).Begin();
                        break;
                }
            }
        }

        LayoutState CurrentLayout => Math.Abs(DesiredSize.Height) < 0 || Math.Abs(DesiredSize.Width) < 0
                                        ? LayoutState.None
                                        : (DesiredSize.Width / DesiredSize.Height >= 1)
                                            ? LayoutState.Wide
                                            : LayoutState.Narrow;

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


        List<DeviceInformation> Ports { get; } = new List<DeviceInformation>();

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
                CurrentState = DataSelectionState.None;
            };
            HintRight.Completed += (s, e) => HintLeft.Begin();
            HintDown.Completed += (s, e) => HintUp.Begin();
            HideStatBar();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            ApplicationView.GetForCurrentView().TitleBar.ForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonPressedForegroundColor = Colors.White;
        }

        async Task ShowStatBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                await StatusBar.GetForCurrentView().ShowAsync();
        } 

        async Task HideStatBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                await StatusBar.GetForCurrentView().HideAsync();
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
            var controlGrid = sender as Grid;
            if (controlGrid == null) return;

            CurrentState = controlGrid.Name == "OpenFileControl" &&
                            ((CurrentLayout == LayoutState.Wide && (e.Velocities.Linear.X > 0 || (Math.Abs(e.Velocities.Linear.X) < 0 && (MainGrid.RenderTransform as TranslateTransform).X >= -_internalWindowSize.Width / 4)))
                            || (CurrentLayout == LayoutState.Narrow && (e.Velocities.Linear.Y > 0 || (Math.Abs(e.Velocities.Linear.Y) < 0 && (MainGrid.RenderTransform as TranslateTransform).Y >= -_internalWindowSize.Height / 4))))
                            ? DataSelectionState.Open
                            : (controlGrid.Name == "ConnectModuleControl" && ((CurrentLayout == LayoutState.Wide && (e.Velocities.Linear.X < 0 || (Math.Abs(e.Velocities.Linear.X) < 0 && (MainGrid.RenderTransform as TranslateTransform).X <= 3 * -_internalWindowSize.Width / 4)))
                                || (CurrentLayout == LayoutState.Narrow && (e.Velocities.Linear.Y < 0 || (Math.Abs(e.Velocities.Linear.Y) < 0 && (MainGrid.RenderTransform as TranslateTransform).Y <= 3 * -_internalWindowSize.Height / 4))))
                                ? DataSelectionState.Connect
                                : DataSelectionState.None);
        }

        void GridControlManipulationDelta(Object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var controlGrid = sender as Grid;
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
        async Task FileViewPageOpen()
        {
            OpeningFileIconAnimation.Begin();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += GoBackToDataSelect;

            IList<String> lines = null;
            VisualStateManager.GoToState(this, "FileLoading", false);

            VisualStateManager.GoToState(this, "File" + ((!String.IsNullOrWhiteSpace(loadFileToken) && Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(loadFileToken) && (lines = await FileIO.ReadLinesAsync(await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(loadFileToken))) != null) ? (lines.Count > 0 && lines[0] == "UTC Time [hhmmss.ss],Temperature [°C],Pressure [mB],X Acceleration [Gs],Y Acceleration [Gs],Z Acceleration [Gs],Latitude [dddmm.mm],N/S Indicator,Longitude [dddmm.mm],W/E Indicator,Altitude [m]" ? "Success" : "UnknownHeader") : "Fail"), false);

            OpeningFileIconAnimation.Stop();

            if (((IEnumerable<VisualStateGroup>) VisualStateManager.GetVisualStateGroups(MainGrid)).First(p => p.Name == "DataLoadStates").CurrentState.Name == "FileSuccess")
            {
                lines.Remove(lines[0]);
                //! File loaded successfuly

                Int32[] Times = (from line in lines select Int32.Parse(line.Split(',')[0])).ToArray();
                Double[] Temperatures = (from line in lines select Double.Parse(line.Split(',')[1])).ToArray();
                Double[] Pressures = (from line in lines select Double.Parse(line.Split(',')[2])).ToArray();
                Double[][] Accelerations = (from line in lines select new Double[] { Double.Parse(line.Split(',')[3]), Double.Parse(line.Split(',')[4]), Double.Parse(line.Split(',')[5]) }).ToArray();
                Double[] Latitudes = (from line in lines select Double.Parse(line.Split(',')[6])).ToArray();
                Char[] NSIndicators = (from line in lines select line.Split(',')[7][0]).ToArray();
                Double[] Longitudes = (from line in lines select Double.Parse(line.Split(',')[8])).ToArray();
                Char[] WEIndicators = (from line in lines select line.Split(',')[9][0]).ToArray();
                Double[] Altitudes = (from line in lines select Double.Parse(line.Split(',')[10])).ToArray();

                TemperatureChart.Push(Times, Temperatures);
                PressureChart.Push(Times, Pressures);
                XAccChart.Push(Times, (from XYZ in Accelerations select XYZ[0]).ToArray());
                YAccChart.Push(Times, (from XYZ in Accelerations select XYZ[1]).ToArray());
                ZAccChart.Push(Times, (from XYZ in Accelerations select XYZ[2]).ToArray());
                //? For some reason, this caused a crash
                //RawPacketView.Text = String.Join("\n", lines);

            }
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
            foreach (var chart in (FileDataView.Content as Grid).Children.OfType<Chart2D>()) chart.Clear();
            SystemNavigationManager.GetForCurrentView().BackRequested -= GoBackToDataSelect;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        void CancelDataSelection(Object sender, TappedRoutedEventArgs e) => CurrentState = DataSelectionState.None;

        void ShowDataRequested(Object sender, TappedRoutedEventArgs e)
        {
            var buttonObject = sender as Button;
            if (buttonObject == null) return;
            CurrentState = (String) buttonObject.Content == "Open" ? DataSelectionState.OpenView : ((String) buttonObject.Content == "Connect" ? DataSelectionState.ConnectView : DataSelectionState.None);
        }

        #endregion

        #region Serial stuff

        async void RefreshRequested(Object sender, TappedRoutedEventArgs e) => await LoadSerialPorts();

        async Task LoadSerialPorts()
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
            var openPicker = new FileOpenPicker
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
            ToolTipService.SetToolTip(SelectedFileLabelBorder, loadFile.Path);
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
