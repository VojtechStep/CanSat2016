﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Patha.Utils;
using Windows.ApplicationModel.DataTransfer;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WindowsApp2._0.Controls;
using System.Text;
using Windows.Storage.Streams;

namespace WindowsApp2._0
{
    public sealed partial class EntryPage
    {

        #region Properties, variables and stuff

        Size _internalWindowSize;
        DispatcherTimer dataAnimationTimer = new DispatcherTimer();
        DispatcherTimer nastyHackyTimerThingy = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
        DeviceWatcher deviceWatcher;
        CancellationTokenSource serialCancelTokenSource;

        SerialDevice Device;

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
        Boolean CenterMap { get; set; } = true;
        Boolean LoopAnimation { get; set; } = false;

        public DataSelectionState CurrentState
        {
            get { return _currentState; }
            set
            {
                SystemNavigationManager.GetForCurrentView().BackRequested -= GoToEntryPage;
                _currentState = value;
                Bindings.Update();
                (Resources["DataSelectionAnimation"] as Storyboard).Begin();
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
                        FileViewPageOpenAsync();
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


        ObservableCollection<DeviceInformation> Ports { get; } = new ObservableCollection<DeviceInformation>();

        Boolean PortAvailable => Ports?.Count > 0;

        #endregion

        #region Render stuff

        async Task InitAsync()
        {
            InitializeComponent();
            var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            SizeChanged += (s, e) => Recompose(e.NewSize);

            Loaded += (s, e) =>
            {
                CurrentState = CurrentState;
                Recompose(DesiredSize);
            };
            dataAnimationTimer.Tick += (s, o) =>
            {
                if (TimeSlider.Value < Times.Length - 1)
                    TimeSlider.Value += 1;
                else if (LoopAnimation)
                    TimeSlider.Value = 0;
                else
                {
                    dataAnimationTimer.Stop();
                    Play.Content = "\uE102";
                    Play.Tag = "Play";
                }
            };
            nastyHackyTimerThingy.Tick += (s, o) =>
            {
                nastyHackyTimerThingy.Stop();
                Debug.WriteLine("Firing re-render");
                if (CurrentState == DataSelectionState.OpenView) try { (FileDataView.Content as Grid)?.Children.OfType<Chart2D>().ForEach(p => p.ReRender(p.RenderSize)); } catch (InvalidOperationException) { }
            };
            Ports.CollectionChanged += (s, e) => Bindings.Update();
            deviceWatcher = DeviceInformation.CreateWatcher(SerialDevice.GetDeviceSelector());
            deviceWatcher.Added += async (s, dinf) =>
            {
                if (Ports.Count == 0 || (!Ports.Contains(dinf))) await dispatcher.RunAsync(CoreDispatcherPriority.High, () => Ports.Add(dinf));
                if (Ports.Count == 1) await dispatcher.RunAsync(CoreDispatcherPriority.High, () => PortSelector.SelectedItem = dinf);
            };
            deviceWatcher.Removed += async (s, dinf) =>
            {
                if (Ports.Count > 0 && Ports.Count(p => p.Id == dinf.Id) != 0) await dispatcher.RunAsync(CoreDispatcherPriority.High, () => Ports.Remove(Ports.First(p => p.Id == dinf.Id)));
            };
            await HideStatBarAsync();
            ApplicationView.GetForCurrentView().TitleBar.ForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().TitleBar.ButtonPressedForegroundColor = Colors.White;
            ApplicationView.GetForCurrentView().FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Standard;
        }

#pragma warning disable IDE1006 // Naming Styles
        protected override async void OnNavigatedTo(NavigationEventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            await InitAsync();
            if (e.Parameter != null) CurrentState = (DataSelectionState)e.Parameter;
            else CurrentState = DataSelectionState.None;
        }

        async Task ShowStatBarAsync()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                await StatusBar.GetForCurrentView().ShowAsync();
            }
        }

        async Task HideStatBarAsync()
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
            if (deviceWatcher.Status == DeviceWatcherStatus.Started) deviceWatcher.Stop();
        }

        void OpenPageOpen()
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += GoToEntryPage;
        }

        void ConnectPageOpen()
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += GoToEntryPage;
            if (deviceWatcher.Status == DeviceWatcherStatus.Created ||
                deviceWatcher.Status == DeviceWatcherStatus.Aborted ||
                deviceWatcher.Status == DeviceWatcherStatus.Stopped) deviceWatcher.Start();
        }

        private void GoToEntryPage(Object sender, BackRequestedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(loadFileToken)) try { Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(loadFileToken); } catch { }
            if (!String.IsNullOrWhiteSpace(saveFileToken)) try { Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(saveFileToken); } catch { }
            CurrentState = DataSelectionState.None;
            SystemNavigationManager.GetForCurrentView().BackRequested -= GoToEntryPage;
        }


        void GoBackToDataSelect(Object o, BackRequestedEventArgs e)
        {
            CurrentState = CurrentState == DataSelectionState.OpenView ? DataSelectionState.Open : (CurrentState == DataSelectionState.ConnectView ? DataSelectionState.Connect : DataSelectionState.None);
            (Resources["OpeningFileIconAnimation"] as Storyboard).Stop();
            (Resources["ConnectingModuleIconAnimation"] as Storyboard).Stop();
            dataAnimationTimer.Stop();
            (FileDataView.Content as Grid).Children.OfType<Chart2D>().ForEach(p => p.Clear());
            if (serialCancelTokenSource != null && !serialCancelTokenSource.IsCancellationRequested) serialCancelTokenSource.Cancel();
            SystemNavigationManager.GetForCurrentView().BackRequested -= GoBackToDataSelect;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            e.Handled = true;
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

        private void HelpFileTapped(Object sender, TappedRoutedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            if (frame != null)
            {
                frame.Navigate(typeof(Pages.FileHelp));
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += GoBackToFileOptions;
            }
        }

        private void HelpModuleTapped(Object sender, TappedRoutedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            if (frame != null)
            {
                frame.Navigate(typeof(Pages.ModuleHelp));
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += GoBackToModuleOptions;
            }
        }

        private void GoBackToFileOptions(Object sender, BackRequestedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            if (frame != null && frame.CanGoBack)
            {
                frame.Navigate(typeof(EntryPage), DataSelectionState.Open);
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= GoBackToFileOptions;
            }
        }

        private void GoBackToModuleOptions(Object sender, BackRequestedEventArgs e)
        {
            CancelAllIOTasks();
            var frame = Window.Current.Content as Frame;
            if (frame != null && frame.CanGoBack)
            {
                frame.Navigate(typeof(EntryPage), DataSelectionState.Connect);
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= GoBackToModuleOptions;
            }
        }
        #endregion

        #region Serial stuff
        async void BrowseSaveTappedAsync(Object sender, TappedRoutedEventArgs e)
        {
            BrowseSaveButton.IsEnabled = false;
            SaveToFileLabelBorder.Tapped -= BrowseSaveTappedAsync;
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "Measurement",
                DefaultFileExtension = ".csv",
                FileTypeChoices =
                {
                    ["Comma Separated Values"] = new String[] {".csv"},
                    ["Text"] = new String[] {".txt"},
                    ["CanSat Measurement"] = new String[] {".csmes"},
                    ["Log"] = new String[] {".log"}
                }
            };
            var saveFile = await savePicker.PickSaveFileAsync();
            if (saveFile != null)
            {
                if (!String.IsNullOrWhiteSpace(saveFileToken)) Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(saveFileToken);
                saveFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(saveFile);
                SaveToFileLabel.Text = saveFile.Path;
                ToolTipService.SetToolTip(SaveToFileLabelBorder, saveFile.Path);
            }
            SaveToFileLabelBorder.Tapped += BrowseSaveTappedAsync;
            BrowseSaveButton.IsEnabled = true;
        }

        void ConnectModulePageOpen()
        {
            if (deviceWatcher.Status == DeviceWatcherStatus.Started) deviceWatcher.Stop();
            serialCancelTokenSource = new CancellationTokenSource();
            var reg = serialCancelTokenSource.Token.Register(() => { });
            (Resources["ConnectingModuleIconAnimation"] as Storyboard).Begin();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += GoBackToDataSelect;

            VisualStateManager.GoToState(this, "ModuleConnecting", false);

            //VisualStateManager.GoToState(this, "Module" + ((PortAvailable) ? (!String.IsNullOrWhiteSpace(saveFileToken) ? (await Communication.ConnectAsync(1000, (await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector()))[PortSelector.SelectedIndex].Id) ? (((await Communication.WriteAsync((Byte)'s')) && (await Communication.ReadAsync(1000)) == 0x06) ? (((await Communication.ReadAsync(1000)) == 0x07) ? "Success" : "SDFail") : "NotRecognised") : "NotConnectable") : "FileNotSelected") : "NotConnected"), false);

            String moduleStateAppend;

            if (PortAvailable)
            {
                //if (!String.IsNullOrWhiteSpace(saveFileToken))
                //{
                //    await FileIO.WriteLinesAsync(await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(saveFileToken), new String[] { "UTC Time [hhmmss.ss],Temperature [°C],Pressure [mB],X Acceleration [Gs],Y Acceleration [Gs],Z Acceleration [Gs],Latitude [dddmm.mm],N/S Indicator,Longitude [dddmm.mm],W/E Indicator,Altitude [m]" });
                //    using (Communication com = new Communication())
                //    {
                //        if (await com.ConnectAsync(3000, Ports[PortSelector.SelectedIndex].Id, 9600))
                //        {
                //            await com.WriteAsync(1500, 0x73);
                //            var inp = await com.ReadAsync(6000);
                //            if (!inp.HasValue) Debug.WriteLine("The module responded nothing");
                //            Debug.WriteLine($"X{inp:X}");
                //            if (inp == 0x06)
                //            {
                //                if (await com.ReadAsync(1500) == 0x07)
                //                {
                //                    moduleStateAppend = "Success";
                //                }
                //                else { moduleStateAppend = "SDFail"; }
                //            }
                //            else { moduleStateAppend = "NotRecognised"; }
                //        }
                //        else { moduleStateAppend = "NotConnectable"; }
                //    }
                //}
                //else { moduleStateAppend = "FileNotSelected"; }
                moduleStateAppend = "Success";
            }
            else { moduleStateAppend = "NotConnected"; }

            VisualStateManager.GoToState(this, "Module" + moduleStateAppend, false);

            (Resources["ConnectingModuleIconAnimation"] as Storyboard).Stop();
            if (((IEnumerable<VisualStateGroup>)VisualStateManager.GetVisualStateGroups(MainGrid)).FirstOrDefault(p => p.Name == "DataLoadStates").CurrentState.Name == "ModuleSuccess")
            {
                Debug.WriteLine("Listening");
                ListenAsync();
            }
        }

        CancellationTokenSource ReadCancellationTokenSource;
        CancellationTokenSource WriteCancellationTokenSource;

        Object ReadCancelLock = new Object();
        Object WriteCancelLock = new Object();

        DataReader DataReaderObject = null;
        DataWriter DataWriterObject = null;

        async void ListenAsync()
        {
            #region dead
            //const UInt16 treshold = 15;
            //using (var com = new Communication())
            //{
            //    await com.ConnectAsync(serialCancelTokenSource.Token, (PortSelector.SelectedItem as DeviceInformation).Id);
            //    if (await com.WriteAsync(serialCancelTokenSource.Token, 0x73))
            //    {
            //        UInt16 stat = 0;
            //        while (true)
            //        {
            //            var inp = await com.ReadAsync(serialCancelTokenSource.Token);
            //            if (inp.HasValue)
            //            {
            //                Debug.Write((Char)inp.Value);
            //            }
            //            else
            //            {
            //                Debug.WriteLine($"No data received, attempt #{stat}");
            //                stat++;
            //                if (stat > treshold || serialCancelTokenSource.IsCancellationRequested) break;
            //            }
            //        }
            //    }                //using (var com = new Communication((PortSelector.SelectedItem as DeviceInformation).Id))
            //{
            //    var connectCTS = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            //    if (await com.InitAsync(9600).AsTask(connectCTS.Token))
            //    {
            //        Debug.WriteLine("Device connected");
            //        if (await com.WriteAsync(new Byte[] { 0x73 }))
            //        {
            //            Debug.WriteLine("Payload written");
            //            var after = DateTime.Now.AddSeconds(5);
            //            while (after > DateTime.Now) ;

            //            UInt16 stat = 0;
            //            CancellationTokenSource readCTS;
            //            while (true)
            //            {
            //                readCTS = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
            //                try
            //                {
            //                    var inp = await com.ReadAsync().AsTask(readCTS.Token);
            //                    Debug.Write((Char) inp);
            //                }
            //                catch (Exception)/* when ((UInt32) e.HResult == 0x80004005 /*Failure exception)*/
            //                {
            //                    stat++;
            //                    Debug.WriteLine($"No data received, attempt #{stat}");
            //                    if (/*stat > treshold || readCTS.IsCancellationRequested || */serialCancelTokenSource.IsCancellationRequested) break;
            //                }
            //            }
            //        }
            //        else Debug.WriteLine("Writing failed");
            //    }
            //    else Debug.WriteLine("Connecting failed");
            //}
            //    else Debug.WriteLine("Data wasn't sent");
            //}
            //try
            //{
            //    var token = serialCancelTokenSource.Token;
            //    var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            //    token.ThrowIfCancellationRequested();
            //    using (var com = new Communication())
            //    {
            //        token.Register(com.CancelConnectTask);
            //        token.Register(com.CancelReadTask);
            //        token.Register(com.CancelWriteTask);

            //        await com.ConnectAsync((PortSelector.SelectedItem as DeviceInformation).Id);
            //        if (await com.WriteAsync(new Byte[] { 0x73 }))
            //        {
            //            timer.Tick += (s, e) =>
            //            {

            //            };
            //        }
            //    }
            //}
            //catch (OperationCanceledException) { }
            //catch (Exception e) { Debug.WriteLine(e.Message); }
            //Debug.WriteLine("Stopped listening");
            //using (var com = new Communication())
            //{
            //    var connected = await com.ConnectAsync((PortSelector.SelectedItem as DeviceInformation).Id);
            //    if (connected) Debug.WriteLine("Connected");
            //    else Debug.WriteLine("Not Connected");

            //    var inByte = new Byte[1];
            //    while (true)
            //    {
            //        if (msg != null)
            //        {
            //            await com.WriteAsync(msg, msg.Length, new CancellationTokenSource(5000).Token);
            //            msg = null;
            //        }

            //        if (await com.ReadAsync(inByte, 1, new CancellationTokenSource(1000).Token))
            //        {
            //            Debug.WriteLine(inByte[0]);
            //        }
            //    }
            //}
            #endregion
            Device = await SerialDevice.FromIdAsync((PortSelector.SelectedItem as DeviceInformation).Id);
            ResetReadCancellationTokenSource();
            ResetWriteCancellationTokenSource();
            if (Device != null)
            {
                Device.BaudRate = 9600;
            }
            else Debug.WriteLine("Not Connected");
        }

        private void ResetWriteCancellationTokenSource()
        {
            WriteCancellationTokenSource = new CancellationTokenSource();
            WriteCancellationTokenSource.Token.Register(() => Debug.WriteLine("Write operation cancelled"));
        }

        private void ResetReadCancellationTokenSource()
        {
            ReadCancellationTokenSource = new CancellationTokenSource();
            ReadCancellationTokenSource.Token.Register(() => Debug.WriteLine("Read operation cancelled"));
        }

        void CancelAllIOTasks()
        {
            CancelReadTask();
            CancelWriteTask();
        }

        void CancelReadTask()
        {
            lock (ReadCancelLock)
            {
                if (ReadCancellationTokenSource != null)
                {
                    if (!ReadCancellationTokenSource.IsCancellationRequested)
                    {
                        ReadCancellationTokenSource.Cancel();
                        ResetReadCancellationTokenSource();
                    }
                }
            }
        }

        void CancelWriteTask()
        {
            lock (WriteCancelLock)
            {
                if (WriteCancellationTokenSource != null)
                {
                    if (!WriteCancellationTokenSource.IsCancellationRequested)
                    {
                        WriteCancellationTokenSource.Cancel();
                        ResetWriteCancellationTokenSource();
                    }
                }
            }
        }

        void SetMessage()
        {
            WriteCommandAsync(SerialInput.Text);
            SerialInput.Text = "";
        }

        async void WriteCommandAsync(String command)
        {
            if (Device != null)
            {
                try
                {
                    DataWriterObject = new DataWriter(Device.OutputStream);
                    DataWriterObject.WriteString(command + "\r\n");
                    await WriteAsync(WriteCancellationTokenSource.Token);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    DataWriterObject?.DetachStream();
                    DataWriterObject = null;
                }
            }
        }

        async void GetMessageAsync()
        {
            if (Device != null)
            {
                try
                {
                    DataWriterObject = new DataWriter(Device.OutputStream);
                    DataWriterObject.WriteString("get\r\n");
                    await WriteAsync(WriteCancellationTokenSource.Token);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    DataWriterObject?.DetachStream();
                    DataWriterObject = null;
                }

                Byte[] firstBatch = null;
                Byte[] secondBatch = null;
                try
                {
                    DataReaderObject = new DataReader(Device.InputStream);
                    firstBatch = await ReadAsync(ReadCancellationTokenSource.Token, 5);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    DataReaderObject?.DetachStream();
                    DataReaderObject = null;
                }

                if (firstBatch != null)
                {
                    UInt16 version = (UInt16)((firstBatch[0] << 8) | firstBatch[1]);
                    UInt16 length = (UInt16)((firstBatch[2] << 8) | firstBatch[3]);
                    Debug.WriteLine($"Version: {version}, Length: {length}");
                }
            }
        }

        async Task WriteAsync(CancellationToken token)
        {
            Task<UInt32> storeAsyncTask;
            lock (WriteCancelLock)
            {
                token.ThrowIfCancellationRequested();
                storeAsyncTask = DataWriterObject.StoreAsync().AsTask(token);
            }
            UInt32 bytesWritten = await storeAsyncTask;
            Debug.WriteLine($"{bytesWritten} bytes written");
        }

        async Task<Byte[]> ReadAsync(CancellationToken token, UInt32 count)
        {
            Task<UInt32> loadAsyncTask;
            lock (ReadCancelLock)
            {
                token.ThrowIfCancellationRequested();
                DataReaderObject.InputStreamOptions = InputStreamOptions.ReadAhead;
                loadAsyncTask = DataReaderObject.LoadAsync(count).AsTask(token);
            }

            UInt32 bytesRead = await loadAsyncTask;
            Debug.WriteLine($"{bytesRead} bytes read");
            if (bytesRead > 0)
            {
                var bytes = new Byte[bytesRead];
                DataReaderObject.ReadBytes(bytes);
                for (UInt32 i = 0; i < bytesRead; i++)
                {
                    Debug.WriteLine($"[{i}] = {bytes[i]}");
                }
                return bytes;
            }
            return null;
        }

        #endregion

        #region File stuff

        async void BrowseOpenTappedAsync(Object sender, TappedRoutedEventArgs e)
        {
            BrowseOpenButton.IsEnabled = false;
            SelectedFileLabelBorder.Tapped -= BrowseOpenTappedAsync;
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

            if (loadFile != null)
            {
                if (!String.IsNullOrWhiteSpace(loadFileToken)) Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(loadFileToken);
                UpdateEnvironmentWithFile(loadFile);
            }
            SelectedFileLabelBorder.Tapped += BrowseOpenTappedAsync;
            BrowseOpenButton.IsEnabled = true;
        }

        public void UpdateEnvironmentWithFile(IStorageItem loadFile)
        {
            loadFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(loadFile);
            SelectedFileLabel.Text = loadFile.Path;
            ToolTipService.SetToolTip(SelectedFileLabelBorder, loadFile.Path);
        }

        async void FileViewPageOpenAsync()
        {
            TimeSlider.IsEnabled = Play.IsEnabled = Settings.IsEnabled = false;
            (Resources["OpeningFileIconAnimation"] as Storyboard).Begin();
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

            (Resources["OpeningFileIconAnimation"] as Storyboard).Stop();


            if (((IEnumerable<VisualStateGroup>)VisualStateManager.GetVisualStateGroups(MainGrid)).First(p => p.Name == "DataLoadStates").CurrentState.Name == "FileSuccess")
            {
                lines.Remove(lines[0]);
                //! File loaded successfully

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

                RawPacketView.Text = String.Join(Environment.NewLine, lines);
                TimeSlider.Maximum = Times.Count() - 1;
                TimeSlider.Value = 0;

                //TimeSlider.ThumbToolTipValueConverter = new TimeSliderConverter();

                TimeSlider.IsEnabled = Play.IsEnabled = Settings.IsEnabled = true;
                UpdateGraphs(0);
            }
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
            if (CenterMap)
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

        private void CopyDataToClipboard(Object sender, TappedRoutedEventArgs e)
        {
            DataPackage tempTextPackage = new DataPackage();
            tempTextPackage.SetText(RawPacketView.Text);
            Clipboard.Clear();
            Clipboard.SetContent(tempTextPackage);
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
            CenterMap = (sender as ToggleSwitch).IsOn;
        }

        private void AerialToggled(Object sender, RoutedEventArgs e)
        {
            GPSDataMap.Style = (sender as ToggleSwitch).IsOn ? MapStyle.AerialWithRoads : MapStyle.Terrain;
        }

        private void LoopToggled(Object sender, RoutedEventArgs e)
        {
            LoopAnimation = (sender as ToggleSwitch).IsOn;
        }

        #endregion

        #region Utilities
        public static String ReadableTimeFromGPSTime(Double toConvert)
        {
            var Hours = Math.Floor(toConvert / 10000) % 100 < 10 ? $"0{Math.Floor(toConvert / 10000) % 100}" : (Math.Floor(toConvert / 10000) % 100).ToString();
            var Minutes = Math.Floor(toConvert / 100) % 100 < 10 ? $"0{Math.Floor(toConvert / 100) % 100}" : (Math.Floor(toConvert / 100) % 100).ToString();
            var Seconds = Math.Floor(toConvert) % 100 < 10 ? $"0{Math.Floor(toConvert) % 100}" : (Math.Floor(toConvert) % 100).ToString();
            return $"{Hours}:{Minutes}:{Seconds}";
        }

        private void OpenFileDragOver(Object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
            e.DragUIOverride.Caption = e.DataView.Properties.Count > 1 ? "Drag only one file" : "Visualize the data file";
        }

        private async void OpenFileDropAsync(Object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count == 1)
                {
                    UpdateEnvironmentWithFile(items[0]);
                }
            }
        }
    }
    #endregion

    #region Enums and little helpers

    public enum DataSelectionState
    {
        None,
        Open,
        Connect,
        OpenView,
        ConnectView
    }

    enum LayoutState
    {
        None,
        Wide,
        Narrow
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
