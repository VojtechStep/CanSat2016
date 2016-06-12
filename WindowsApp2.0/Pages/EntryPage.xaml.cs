using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Utils;

namespace WindowsApp2._0
{
    public sealed partial class EntryPage
    {
        private Size _internalWindowSize;

        private DataSelectionState _currentState = DataSelectionState.None;

        private DataSelectionState CurrentState
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
                switch (value)
                {
                    case DataSelectionState.Open:
                        OpenPageOpen();
                        break;
                    case DataSelectionState.Connect:
                        ConnectPageOpen();
                        break;
                }
            }
        }

        private LayoutState CurrentLayout
            =>
                DesiredSize.Width >= 720
                    ? LayoutState.Wide
                    : (DesiredSize.Width > 0 ? LayoutState.Narrow : LayoutState.None);

        private LayoutState _previousLayout;

        private Double? OpenGridTranslateX => CurrentLayout == LayoutState.Wide
                                                  ? (CurrentState == DataSelectionState.Open
                                                         ? 0
                                                         : (CurrentState == DataSelectionState.None
                                                                ? -_internalWindowSize.Width/2
                                                                : -_internalWindowSize.Width))
                                                  : 0;


        private Double? ConnectGridTranslateX => CurrentLayout == LayoutState.Wide
                                                     ? (CurrentState == DataSelectionState.Connect
                                                            ? 0
                                                            : (CurrentState == DataSelectionState.None
                                                                   ? _internalWindowSize.Width/2
                                                                   : _internalWindowSize.Width))
                                                     : 0;

        private Double? OpenGridTranslateY => CurrentLayout == LayoutState.Narrow
                                                  ? (CurrentState == DataSelectionState.Open
                                                         ? 0
                                                         : (CurrentState == DataSelectionState.None
                                                                ? -_internalWindowSize.Height/2
                                                                : -_internalWindowSize.Height))
                                                  : 0;

        private Double? ConnectGridTranslateY => CurrentLayout == LayoutState.Narrow
                                                     ? (CurrentState == DataSelectionState.Connect
                                                            ? 0
                                                            : (CurrentState == DataSelectionState.None
                                                                   ? _internalWindowSize.Height/2
                                                                   : _internalWindowSize.Height))
                                                     : 0;

        private List<DeviceInformation> Ports { get; set; } = new List<DeviceInformation>();
        private Boolean PortAvailable => Ports?.Count > 0;

    public EntryPage()
        {
            InitializeComponent();
            SizeChanged += (s, e) => Recompose(e.NewSize);
            Loaded += (s, e) =>
            {
                Recompose(DesiredSize);
            };
        }



        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void Recompose(Size newSize)
        {
            OpenGrid.Width = ConnectGrid.Width = _internalWindowSize.Width = newSize.Width;
            OpenGrid.Height = ConnectGrid.Height = _internalWindowSize.Height = newSize.Height;
            (OpenGrid.RenderTransform as TranslateTransform).X = OpenGridTranslateX.Value;
            (ConnectGrid.RenderTransform as TranslateTransform).X = ConnectGridTranslateX.Value;
            (OpenGrid.RenderTransform as TranslateTransform).Y = OpenGridTranslateY.Value;
            (ConnectGrid.RenderTransform as TranslateTransform).Y = ConnectGridTranslateY.Value;
            VisualStateManager.GoToState(this, CurrentLayout.ToString(), true);
            if (CurrentLayout != _previousLayout)
            {
                
            }
            _previousLayout = CurrentLayout;
        }

        private void GridControlManipulationCompleted(Object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Grid controlGrid = sender as Grid;
            if (controlGrid == null) return;
            if (controlGrid.Name == "OpenGridControl")
            {
                if ((CurrentLayout == LayoutState.Wide && e.Velocities.Linear.X > 0)
                    || (CurrentLayout == LayoutState.Narrow && e.Velocities.Linear.Y > 0)
                    || (CurrentLayout == LayoutState.Wide && e.Velocities.Linear.X == 0 && (OpenGrid.RenderTransform as TranslateTransform).X >= -_internalWindowSize.Width / 4)
                    || (CurrentLayout == LayoutState.Narrow && e.Velocities.Linear.Y == 0 && (OpenGrid.RenderTransform as TranslateTransform).Y >= -_internalWindowSize.Height / 4))
                        CurrentState = DataSelectionState.Open;
                else
                    CurrentState = DataSelectionState.None;
            }
            else
            {
                if ((CurrentLayout == LayoutState.Wide && e.Velocities.Linear.X < 0)
                    || (CurrentLayout == LayoutState.Narrow && e.Velocities.Linear.Y < 0)
                    || (CurrentLayout == LayoutState.Wide && e.Velocities.Linear.X == 0 && (ConnectGrid.RenderTransform as TranslateTransform).X <= _internalWindowSize.Width / 4)
                    || (CurrentLayout == LayoutState.Narrow && e.Velocities.Linear.Y == 0 && (ConnectGrid.RenderTransform as TranslateTransform).Y <= _internalWindowSize.Height / 4))
                        CurrentState = DataSelectionState.Connect;
                else
                    CurrentState = DataSelectionState.None;
            }
            Bindings.Update();
            DataSelectionAnimation.Begin();
        }

        private void GridControlManipulationDelta(Object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Grid controlGrid = sender as Grid;
            if (controlGrid == null) return;
            if (CurrentLayout == LayoutState.Wide)
            {
                (OpenGrid.RenderTransform as TranslateTransform).X =
                    MathUtils.Limit((OpenGrid.RenderTransform as TranslateTransform).X + e.Delta.Translation.X,
                        controlGrid.Name == "OpenGridControl" ? -_internalWindowSize.Width / 2 : -_internalWindowSize.Width, controlGrid.Name == "OpenGridControl" ? 0 : -_internalWindowSize.Width / 2);
                (ConnectGrid.RenderTransform as TranslateTransform).X =
                    MathUtils.Limit((ConnectGrid.RenderTransform as TranslateTransform).X + e.Delta.Translation.X,
                        controlGrid.Name == "OpenGridControl" ? _internalWindowSize.Width / 2 : 0, controlGrid.Name == "OpenGridControl" ? _internalWindowSize.Width : _internalWindowSize.Width / 2);
            }
            else
            {
                (OpenGrid.RenderTransform as TranslateTransform).Y =
                    MathUtils.Limit((OpenGrid.RenderTransform as TranslateTransform).Y + e.Delta.Translation.Y,
                        controlGrid.Name == "OpenGridControl" ? -_internalWindowSize.Height / 2 : -_internalWindowSize.Height, controlGrid.Name == "OpenGridControl" ? 0 : -_internalWindowSize.Height / 2);
                (ConnectGrid.RenderTransform as TranslateTransform).Y =
                    MathUtils.Limit((ConnectGrid.RenderTransform as TranslateTransform).Y + e.Delta.Translation.Y,
                        controlGrid.Name == "OpenGridControl" ? _internalWindowSize.Height / 2 : 0, controlGrid.Name == "OpenGridControl" ? _internalWindowSize.Height : _internalWindowSize.Height / 2);
            }

            if (e.IsInertial) e.Complete();
            e.Handled = true;
        }

        private void GridControlManipulationStarted(Object sender, ManipulationStartedRoutedEventArgs e)
        {

            OpenGrid.SetValue(Canvas.ZIndexProperty, (sender as Grid)?.Name == "OpenGridControl" ? 1 : 0);
            ConnectGrid.SetValue(Canvas.ZIndexProperty, (sender as Grid)?.Name == "OpenGridControl" ? 0 : 1);
        }

        private void GridControlTapped(Object sender, TappedRoutedEventArgs e)
        {
            CurrentState = (sender as Grid)?.Name == "OpenGridControl"
                               ? (CurrentState == DataSelectionState.None
                                      ? DataSelectionState.Open
                                      : DataSelectionState.None)
                               : (CurrentState == DataSelectionState.None
                                      ? DataSelectionState.Connect
                                      : DataSelectionState.None);
            Bindings.Update();
            DataSelectionAnimation.Begin();
        }

        private void OpenPageOpen()
        {
            
        }

        private void ConnectPageOpen()
        {
            LoadSerialPorts();
        }

        private void CancelDataSelection(Object sender, RoutedEventArgs e)
        {
            CurrentState = DataSelectionState.None;
            Bindings.Update();
            DataSelectionAnimation.Begin();
        }

        private void RefreshRequested(Object sender, RoutedEventArgs e)
        {
            LoadSerialPorts();
        }

        private async void LoadSerialPorts()
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
    }

    enum DataSelectionState
    {
        Open,
        Connect,
        None
    }

    enum LayoutState
    {
        None,
        Wide,
        Narrow
    }
}
