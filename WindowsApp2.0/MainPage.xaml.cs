using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Utils;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WindowsApp2._0
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private Size _internalWindowSize;

        private DataSelectionState CurrentState { get; set; } = DataSelectionState.None;
        private LayoutState CurrentLayout => DesiredSize.Width >= 720 ? LayoutState.Wide : (DesiredSize.Width > 0 ? LayoutState.Narrow : LayoutState.None);
        private LayoutState _previousLayout;
        private Boolean firstWideExperience = true;
        private Boolean firstNarrowExperience = true;
        private Double? OpenGridTranslateX => CurrentLayout == LayoutState.Wide
                                                ? (CurrentState == DataSelectionState.Open
                                                    ? 0
                                                    : (CurrentState == DataSelectionState.None
                                                        ? -_internalWindowSize.Width / 2
                                                        : -_internalWindowSize.Width))
                                                : 0;

        private Double? ConnectGridTranslateX => CurrentLayout == LayoutState.Wide
                                                    ? (CurrentState == DataSelectionState.Connect
                                                        ? 0
                                                        : (CurrentState == DataSelectionState.None
                                                            ? _internalWindowSize.Width / 2
                                                            : _internalWindowSize.Width))
                                                    : 0;

        private Double? OpenGridTranslateY => CurrentLayout == LayoutState.Narrow
                                                ? (CurrentState == DataSelectionState.Open
                                                    ? 0
                                                    : (CurrentState == DataSelectionState.None
                                                        ? -_internalWindowSize.Height / 2
                                                        : -_internalWindowSize.Height))
                                                : 0;

        private Double? ConnectGridTranslateY => CurrentLayout == LayoutState.Narrow
                                                     ? (CurrentState == DataSelectionState.Connect
                                                         ? 0
                                                         : (CurrentState == DataSelectionState.None
                                                             ? _internalWindowSize.Height / 2
                                                             : _internalWindowSize.Height))
                                                     : 0;

        private Double? SwipeLeftTranslateXOrigin => _internalWindowSize.Width / 2;
        private Double? SwipeLeftTranslateXEnd => -_internalWindowSize.Width / 2;
        private Double? SwipeRightTranslateXOrigin => -_internalWindowSize.Width / 2;
        private Double? SwipeRightTranslateXEnd => _internalWindowSize.Width / 2;
        private Double? SwipeUpTranslateYOrigin => _internalWindowSize.Height / 2;
        private Double? SwipeUpTranslateYEnd => -_internalWindowSize.Height / 2;
        private Double? SwipeDownTranslateYOrigin => -_internalWindowSize.Height / 2;
        private Double? SwipeDownTranslateYEnd => _internalWindowSize.Height / 2;

        public MainPage()
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
            //if (SwipeLeftHighlight.Visibility == Visibility.Visible)
            //    (SwipeLeftHighlight.RenderTransform as TranslateTransform).X = SwipeLeftTranslateXOrigin.Value;
            //if (SwipeRightHighlight.Visibility == Visibility.Visible)
            //    (SwipeRightHighlight.RenderTransform as TranslateTransform).X = SwipeRightTranslateXOrigin.Value;
            //if (SwipeUpHighlight.Visibility == Visibility.Visible)
            //    (SwipeUpHighlight.RenderTransform as TranslateTransform).Y = SwipeUpTranslateYOrigin.Value;
            //if (SwipeDownHighlight.Visibility == Visibility.Visible)
            //    (SwipeDownHighlight.RenderTransform as TranslateTransform).Y = SwipeDownTranslateYOrigin.Value;
            if (CurrentLayout != _previousLayout)
            {
                VisualStateManager.GoToState(this, CurrentLayout.ToString(), true);
                if (CurrentLayout == LayoutState.Wide && firstWideExperience)
                {
                    Bindings.Update();
                    SwipeLeftHighlight.Visibility = SwipeRightHighlight.Visibility = Visibility.Visible;
                    SwipeHorizontalHint.Begin();
                    //firstWideExperience = false;
                }
                if (CurrentLayout == LayoutState.Narrow && firstNarrowExperience)
                {
                    Bindings.Update();
                    SwipeUpHighlight.Visibility = SwipeDownHighlight.Visibility = Visibility.Visible;
                    SwipeVerticalHint.Begin();
                    //firstNarrowExperience = false;
                }
            }
            _previousLayout = CurrentLayout;
        }

        private void GridControlManipulationCompleted(Object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Grid controlGrid = sender as Grid;
            if (controlGrid == null) return;
            if (controlGrid.Name == "OpenGridControl")
            {
                if ((CurrentLayout == LayoutState.Wide && e.Velocities.Linear.X > 0) || (CurrentLayout == LayoutState.Narrow && e.Velocities.Linear.Y > 0)) CurrentState = DataSelectionState.Open;
                else
                    if (CurrentState == DataSelectionState.Open && ((CurrentLayout == LayoutState.Wide && e.Velocities.Linear.X < 0) || (CurrentLayout == LayoutState.Narrow && e.Velocities.Linear.Y < 0)))
                    CurrentState = DataSelectionState.None;
            }
            else
            {
                if ((CurrentLayout == LayoutState.Wide && e.Velocities.Linear.X < 0) || (CurrentLayout == LayoutState.Narrow && e.Velocities.Linear.Y < 0)) CurrentState = DataSelectionState.Connect;
                else
                    if (CurrentState == DataSelectionState.Connect && ((CurrentLayout == LayoutState.Wide && e.Velocities.Linear.X > 0) || (CurrentLayout == LayoutState.Narrow && e.Velocities.Linear.Y > 0)))
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
