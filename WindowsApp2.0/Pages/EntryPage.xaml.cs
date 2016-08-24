using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Patha.Utils;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
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
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WindowsApp2._0.Controls;
using WindowsApp2._0.Utils;

namespace WindowsApp2._0
{
    public sealed partial class EntryPage
    {

        #region Properties, varables and stuff

        Size _internalWindowSize;
        DispatcherTimer dataAnimationTimer = new DispatcherTimer();
        DispatcherTimer nastyHackyTimerThingy = new DispatcherTimer { Interval = new TimeSpan(0, 0 , 1) };
        Boolean _centerMap = true;

        static Int32[] Times;
        Double[] Longitudes;
        Double[] Latitudes;
        Double[] Altitudes;

        static Int32 _currentTime;
        public static Int32 CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
            }
        }


        public String loadFileToken;
        String saveFileToken;

        DataSelectionState _currentState = DataSelectionState.None;

        public DataSelectionState CurrentState
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
                Bindings.Update();
                DataSelectionAnimation.Begin();
                AdjustTitleBar();
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
                        break;
                }
            }
        }

        LayoutState CurrentLayout => Math.Abs(_internalWindowSize.Height) <= 0 || Math.Abs(_internalWindowSize.Width) <= 0
                                        ? LayoutState.None
                                        : (_internalWindowSize.Width / _internalWindowSize.Height >= 1)
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
            Init();
        }

        async Task Init()
        {
            InitializeComponent();
            SizeChanged += (s, e) => Recompose(e.NewSize);
            Loaded += (s, e) =>
            {
                Recompose(DesiredSize);
                CurrentState = DataSelectionState.None;
            };
            dataAnimationTimer.Tick += (s, o) => TimeSlider.Value += 1;
            nastyHackyTimerThingy.Tick += (s, o) =>
            {
                nastyHackyTimerThingy.Stop();
                Debug.WriteLine("Firing rerender");
                if (CurrentState == DataSelectionState.OpenView) try { (FileDataView.Content as Grid)?.Children.OfType<Chart2D>().ForEach(p => p.ReRender(p.RenderSize)); } catch (InvalidOperationException) { }
            };
            await HideStatBar();
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = false;
            ApplicationView.GetForCurrentView().TitleBar.ForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonPressedForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Standard;
        }

        async Task ShowStatBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                await StatusBar.GetForCurrentView().ShowAsync();
            }
        }

        async Task HideStatBar()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
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

            nastyHackyTimerThingy.Stop();
            nastyHackyTimerThingy.Start();

            if (CurrentLayout == _previousLayout) return;

            AdjustTitleBar();
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
            if (ApiInformation.IsApiContractPresent("Windows.Phone.UI.Input.HardwareButtons", 1, 0))
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += GoToEntryPage;
        }

        async Task ConnectPageOpen()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Phone.UI.Input.HardwareButtons", 1, 0))
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += GoToEntryPage;
            await LoadSerialPorts();
        }

        private void GoToEntryPage(Object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(loadFileToken)) try { Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(loadFileToken); } catch (Exception) { }
            CurrentState = DataSelectionState.None;
            if (ApiInformation.IsApiContractPresent("Windows.Phone.UI.Input.HardwareButtons", 1, 0))
                Windows.Phone.UI.Input.HardwareButtons.BackPressed -= GoToEntryPage;
        }



        async Task FileViewPageOpen()
        {
            TimeSlider.IsEnabled = Play.IsEnabled = Settings.IsEnabled = false;
            OpeningFileIconAnimation.Begin();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += GoBackToDataSelect;

            IList<String> lines = null;
            VisualStateManager.GoToState(this, "FileLoading", false);

            //VisualStateManager.GoToState(this, "File" + ((!String.IsNullOrWhiteSpace(loadFileToken) && Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(loadFileToken) && (lines = await FileIO.ReadLinesAsync(await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(loadFileToken))) != null) ? (lines.Count > 0 && lines[0] == "UTC Time [hhmmss.ss],Temperature [°C],Pressure [mB],X Acceleration [Gs],Y Acceleration [Gs],Z Acceleration [Gs],Latitude [dddmm.mm],N/S Indicator,Longitude [dddmm.mm],W/E Indicator,Altitude [m]" ? (lines.Count > 2 ? "Success" : "Empty") : "UnknownHeader") : "Fail"), false);

            String fileStateAppend;

            if (!String.IsNullOrWhiteSpace(loadFileToken) && Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(loadFileToken))
            {
                lines = await FileIO.ReadLinesAsync(await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(loadFileToken));
                if (lines != null)
                {
                    if (lines.Count > 0 && lines[0] == "UTC Time [hhmmss.ss],Temperature [°C],Pressure [mB],X Acceleration [Gs],Y Acceleration [Gs],Z Acceleration [Gs],Latitude [dddmm.mm],N/S Indicator,Longitude [dddmm.mm],W/E Indicator,Altitude [m]")
                    {
                        if (lines.Count > 2)
                        {
                            fileStateAppend = "Success";
                        }
                        else { fileStateAppend = "Empty"; }
                    }
                    else { fileStateAppend = "UnknownHeader"; }
                }
                else { fileStateAppend = "Fail"; }
            }
            else { fileStateAppend = "Fail"; }

            VisualStateManager.GoToState(this, "File" + fileStateAppend, false);

            OpeningFileIconAnimation.Stop();

            TimeSlider.IsEnabled = Play.IsEnabled = Settings.IsEnabled = true;

            if (((IEnumerable<VisualStateGroup>)VisualStateManager.GetVisualStateGroups(MainGrid)).First(p => p.Name == "DataLoadStates").CurrentState.Name == "FileSuccess")
            {
                lines.Remove(lines[0]);
                //! File loaded successfuly

                foreach (Chart2D chart in (FileDataView.Content as Grid).Children.OfType<Chart2D>()) chart.XMarkerConverter = ReadableTimeFromGPSTime;

                Times = (from line in lines select (Int32)Double.Parse(line.Split(',')[0])).ToArray();
                TemperatureChart.Push(Times, (from line in lines select Double.Parse(line.Split(',')[1])).ToArray());
                PressureChart.Push(Times, (from line in lines select Double.Parse(line.Split(',')[2])).ToArray());
                XAccChart.Push(Times, (from line in lines select Double.Parse(line.Split(',')[3])).ToArray());
                YAccChart.Push(Times, (from line in lines select Double.Parse(line.Split(',')[4])).ToArray());
                ZAccChart.Push(Times, (from line in lines select Double.Parse(line.Split(',')[5])).ToArray());
                if ((await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(loadFileToken)).DisplayName.Contains("FinalData"))
                {
                    Latitudes = (from line in lines select Double.Parse(line.Split(',')[6]) / 100).ToArray();
                    Longitudes = (from line in lines select Double.Parse(line.Split(',')[8]) / 100).ToArray();
                }
                else
                {
                    Latitudes = (from line in lines select (Math.Floor((Double.Parse(line.Split(',')[6])) / 100) + (Double.Parse(line.Split(',')[6]) % 100) / 60)).ToArray();
                    Longitudes = (from line in lines select (Math.Floor((Double.Parse(line.Split(',')[8])) / 100) + (Double.Parse(line.Split(',')[8]) % 100) / 60)).ToArray();
                }
                Altitudes = (from line in lines select Double.Parse(line.Split(',')[10])).ToArray();

                RawPacketView.Text = String.Join("\n", lines);
                TimeSlider.Maximum = Times.Count() - 1;

                //TimeSlider.ThumbToolTipValueConverter = new TimeSliderConverter();

                UpdateGraphs(0);
            }
        }
        async Task ConnectModulePageOpen()
        {
            ConnectingModuleIconAnimation.Begin();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += GoBackToDataSelect;
            if (ApiInformation.IsApiContractPresent("Windows.Phone.UI.Input.HardwareButtons", 1, 0))
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += GoBackToDataSelect;


            VisualStateManager.GoToState(this, "ModuleConnecting", false);

            //VisualStateManager.GoToState(this, "Module" + ((PortAvailable) ? (!String.IsNullOrWhiteSpace(saveFileToken) ? (await Communication.ConnectAsync(1000, (await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector()))[PortSelector.SelectedIndex].Id) ? (((await Communication.WriteAsync((Byte)'s')) && (await Communication.ReadAsync(1000)) == 0x06) ? (((await Communication.ReadAsync(1000)) == 0x07) ? "Success" : "SDFail") : "NotRecognised") : "NotConnectable") : "FileNotSelected") : "NotConnected"), false);

            String moduleStateAppend;

            if (PortAvailable)
            {
                if (!String.IsNullOrWhiteSpace(saveFileToken))
                {
                    await FileIO.WriteLinesAsync(await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(saveFileToken), new String[] { "UTC Time [hhmmss.ss],Temperature [°C],Pressure [mB],X Acceleration [Gs],Y Acceleration [Gs],Z Acceleration [Gs],Latitude [dddmm.mm],N/S Indicator,Longitude [dddmm.mm],W/E Indicator,Altitude [m]" });
                    if (await Communication.ConnectAsync(1500, (await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector()))[PortSelector.SelectedIndex].Id))
                    {
                        await Communication.WriteAsync(1500, (Byte)'s');
                        if (await Communication.ReadAsync(1500) == 0x06)
                        {
                            if (await Communication.ReadAsync(1500) == 0x07)
                            {
                                moduleStateAppend = "Success";
                            }
                            else { moduleStateAppend = "SDFail"; }
                        }
                        else { moduleStateAppend = "NotRecognised"; }
                    }
                    else { moduleStateAppend = "NotConnectable"; }
                }
                else { moduleStateAppend = "FileNotSelected"; }
            }
            else { moduleStateAppend = "NotConnected"; }

            VisualStateManager.GoToState(this, "Module" + moduleStateAppend, false);

            ConnectingModuleIconAnimation.Stop();
            if (((IEnumerable<VisualStateGroup>)VisualStateManager.GetVisualStateGroups(MainGrid)).First(p => p.Name == "DataLoadStates").CurrentState.Name == "ModuleSuccess")
            {
                Listen();
                Debug.WriteLine("Listening");
            }
        }

        void GoBackToDataSelect(Object o, Windows.Phone.UI.Input.BackPressedEventArgs e) => GoBackToDataSelect(o, (BackRequestedEventArgs)null);
        void GoBackToDataSelect(object o, BackRequestedEventArgs e)
        {
            CurrentState = CurrentState == DataSelectionState.OpenView ? DataSelectionState.Open : (CurrentState == DataSelectionState.ConnectView ? DataSelectionState.Connect : DataSelectionState.None);
            OpeningFileIconAnimation.Stop();
            ConnectingModuleIconAnimation.Stop();
            dataAnimationTimer.Stop();
            (FileDataView.Content as Grid).Children.OfType<Chart2D>().ForEach(p => p.Clear());
            if (ApiInformation.IsApiContractPresent("Windows.Phone.UI.Input.HardwareButtons", 1, 0))
                Windows.Phone.UI.Input.HardwareButtons.BackPressed -= GoBackToDataSelect;
            SystemNavigationManager.GetForCurrentView().BackRequested -= GoBackToDataSelect;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        void CancelDataSelection(Object sender, TappedRoutedEventArgs e) => CurrentState = DataSelectionState.None;

        void ShowDataRequested(Object sender, TappedRoutedEventArgs e)
        {
            var buttonObject = sender as Button;
            if (buttonObject == null) return;
            if ((String)buttonObject.Content == "Open")
            {
                if (!String.IsNullOrWhiteSpace(loadFileToken))
                    CurrentState = DataSelectionState.OpenView;
                else
                {
                    var errorMsg = new Grid { Background = new SolidColorBrush(Color.FromArgb(255, 255, 100, 100)), Padding = new Thickness(10), BorderThickness = new Thickness(2) };
                    errorMsg.Children.Add(new TextBlock { Text = "First, select a file to show", TextWrapping = TextWrapping.WrapWholeWords });
                    new Flyout { Content = errorMsg, FlyoutPresenterStyle = (Style)Resources["BorderlessFlyout"] }.ShowAt(buttonObject);
                }
            }
            else if ((String)buttonObject.Content == "Connect")
            {
                if (!String.IsNullOrWhiteSpace(saveFileToken) && PortAvailable)
                    CurrentState = DataSelectionState.ConnectView;
                else
                {
                    var errorMsg = new Grid { Background = new SolidColorBrush(Color.FromArgb(255, 255, 100, 100)), Padding = new Thickness(10), BorderThickness = new Thickness(2) };
                    errorMsg.Children.Add(new TextBlock { Text = "First, connect and select the module and select an output file's location", TextWrapping = TextWrapping.WrapWholeWords });
                    new Flyout { Content = errorMsg, FlyoutPresenterStyle = (Style)Resources["BorderlessFlyout"] }.ShowAt(buttonObject);
                }
            }
            else
            {
                CurrentState = DataSelectionState.None;
            }
        }

        #endregion

        #region Serial stuff
        async void BrowseSaveTapped(Object sender, TappedRoutedEventArgs e)
        {
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "Mesurement",
                DefaultFileExtension = ".csv",
                FileTypeChoices =
                {
                    ["Comma Separated Values"] = new String[] {".csv"},
                    ["Text"] = new String[] {".txt"},
                    ["CanSat Mesurement"] = new String[] {".csmes"},
                    ["Log"] = new String[] {".log"}
                }
            };
            var saveFile = await savePicker.PickSaveFileAsync();
            if (saveFile == null) return;
            saveFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(saveFile);
            SaveToFileLabel.Text = saveFile.Path;
            ToolTipService.SetToolTip(SaveToFileLabelBorder, saveFile.Path);
        }

        async void RefreshRequested(Object sender, TappedRoutedEventArgs e) => await LoadSerialPorts();

        async Task LoadSerialPorts()
        {
            Ports.Clear();
            var selector = SerialDevice.GetDeviceSelector();
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector).AsTask(new CancellationTokenSource(2000).Token);
            foreach (DeviceInformation di in devices)
            {
                Ports.Add(di);
            }
            Bindings.Update();
            if (PortAvailable) PortSelector.SelectedIndex = 0;
        }

        async Task Listen()
        {
            await Task.Run(() => { });
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
                    ".csmes",
                    ".log"
                }
            };
            var loadFile = await openPicker.PickSingleFileAsync();

            if (loadFile == null) return;
            if (!String.IsNullOrWhiteSpace(loadFileToken)) Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(loadFileToken);
            UpdateEnvironmentWithFile(loadFile);
        }

        public void UpdateEnvironmentWithFile(IStorageItem loadFile)
        {
            loadFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(loadFile);
            SelectedFileLabel.Text = loadFile.Path;
            ToolTipService.SetToolTip(SelectedFileLabelBorder, loadFile.Path);
        }

        void TimeSlider_ValueChanged(Object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e) => UpdateGraphs((Int32)e.NewValue);
        void UpdateGraphs(Int32 newValue)
        {
            CurrentTime = Times[newValue];
            TempIndicatorValue.Text = TemperatureChart.Get(CurrentTime).ToString();
            PresIndicatorValue.Text = PressureChart.Get(CurrentTime).ToString();
            XAccIndicatorValue.Text = XAccChart.Get(CurrentTime).ToString();
            YAccIndicatorValue.Text = YAccChart.Get(CurrentTime).ToString();
            ZAccIndicatorValue.Text = ZAccChart.Get(CurrentTime).ToString();

            try { GPSDataMap.MapElements.Remove(GPSDataMap.MapElements.OfType<MapIcon>().First(p => p.Title == "Probe")); } catch (InvalidOperationException) { }
            var ps = new Geopoint(new BasicGeoposition
            {
                Latitude = Latitudes[newValue],
                Longitude = Longitudes[newValue],
                Altitude = Altitudes[newValue]
            });
            if (_centerMap)
                GPSDataMap.Center = ps;
            GPSDataMap.MapElements.Add(new MapIcon
            {
                Title = "Probe",
                Location = ps
            });

            LatitudeIndicatorValue.Text = Latitudes[newValue].ToString();
            LongitudeIndicatorValue.Text = Longitudes[newValue].ToString();
            AltitudeIndicatorValue.Text = Altitudes[newValue].ToString();


            TimeTextBox.Text = ReadableTimeFromGPSTime(CurrentTime);
            (FileDataView.Content as Grid).Children.OfType<Chart2D>().ForEach(p => p.CurrentData = CurrentTime);
        }

        Double ToDegrees(Double gpsData)
        {
            if (gpsData > Math.Pow(10, 5))
            {
                var Minutes = Double.Parse(gpsData.ToString().Substring(gpsData.ToString().Length - 5));
                return (gpsData - Minutes) / (Math.Pow(10, gpsData.ToString().Length - 5)) + (Minutes / 60);
            }
            return 0;
        }

        void Play_Tapped(Object sender, TappedRoutedEventArgs e)
        {
            var b = sender as Button;
            if (b.Tag as String == "Play")
            {
                if (Math.Abs(TimeSlider.Value - (Times.Length - 1)) <= 0) TimeSlider.Value = 0;
                dataAnimationTimer.Start();
                b.Content = "\uE103";
                b.Tag = "Stop";
            }
            else
            {
                dataAnimationTimer.Stop();
                b.Content = "\uE102";
                b.Tag = "Play";
            }
        }

        void ComboBox_SelectionChanged(Object sender, SelectionChangedEventArgs e)
        {
            var ratio = Double.Parse(((sender as ComboBox).SelectedItem as TextBlock).Text);
            dataAnimationTimer.Interval = TimeSpan.FromSeconds(1 / ratio);
        }
        void CenterMap_Toggled(Object sender, RoutedEventArgs e)
        {
            _centerMap = (sender as ToggleSwitch).IsOn;
        }

        #endregion

        public static String ReadableTimeFromGPSTime(Double toConvert)
        {
            var Hours = Math.Floor(toConvert / 10000) % 100 < 10 ? $"0{Math.Floor(toConvert / 10000) % 100}" : (Math.Floor(toConvert / 10000) % 100).ToString();
            var Minutes = Math.Floor(toConvert / 100) % 100 < 10 ? $"0{Math.Floor(toConvert / 100) % 100}" : (Math.Floor(toConvert / 100) % 100).ToString();
            var Seconds = Math.Floor(toConvert) % 100 < 10 ? $"0{Math.Floor(toConvert) % 100}" : (Math.Floor(toConvert) % 100).ToString();
            return $"{Hours}:{Minutes}:{Seconds}";
        }

        private void ToggleSwitch_Toggled(Object sender, RoutedEventArgs e)
        {
            GPSDataMap.Style = (sender as ToggleSwitch).IsOn ? MapStyle.AerialWithRoads : MapStyle.Terrain;
        }
    }

    #region Enums and little helpers

    public enum DataSelectionState
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

    class TimeSliderConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, String language)
        {
            Debug.WriteLine("?");
            return "?";
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, String language)
        {
            throw new NotImplementedException();
        }
    }

    class IsTrueStateTrigger : StateTriggerBase
    {
        public Boolean Value
        {
            get { return (Boolean)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Boolean), typeof(IsTrueStateTrigger), new PropertyMetadata(true, ValueChangedCallback));
        static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as IsTrueStateTrigger;
            var value = (Boolean)e.NewValue;
            obj.SetActive(value);
        }
    }

    class IsFalseStateTrigger : StateTriggerBase
    {
        public Boolean Value
        {
            get { return (Boolean)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Boolean), typeof(IsFalseStateTrigger), new PropertyMetadata(true, ValueChangedCallback));
        static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as IsFalseStateTrigger;
            var value = (Boolean)e.NewValue;
            obj.SetActive(!value);
        }
    }

    #endregion
}
