using System;
using System.Collections.Generic;
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
        private DataSelectionState _currentState = DataSelectionState.None;
        private LayoutState _currentLayout;

        private Double? OpenGridTranslateX => _currentLayout == LayoutState.Wide
                                                  ? (_currentState == DataSelectionState.Open
                                                         ? 0
                                                         : (_currentState == DataSelectionState.None
                                                                ? -DesiredSize.Width/2
                                                                : -DesiredSize.Width))
                                                  : 0;

        private Double? ConnectGridTranslateX => _currentLayout == LayoutState.Wide
                                                     ? (_currentState == DataSelectionState.Connect
                                                            ? 0
                                                            : (_currentState == DataSelectionState.None
                                                                   ? DesiredSize.Width/2
                                                                   : DesiredSize.Width))
                                                     : 0;

        public MainPage()
        {
            InitializeComponent();
            SizeChanged += (s, e) => Recompose(e.NewSize);
            Loaded += (s, e) =>
            {
                Recompose(DesiredSize);
            };
        }



        private void Recompose(Size newSize)
        {
            if (newSize.Width >= 720)
            {
                VisualStateManager.GoToState(this, "Wide", true);
                OpenGrid.Width = ConnectGrid.Width = newSize.Width;
                (ConnectGrid.RenderTransform as TranslateTransform).X =
                    _currentState == DataSelectionState.None
                        ? newSize.Width / 2
                        : (_currentState == DataSelectionState.Connect ? 0 : newSize.Width);

                (OpenGrid.RenderTransform as TranslateTransform).X =
                    _currentState == DataSelectionState.None
                        ? -newSize.Width/2
                        : (_currentState == DataSelectionState.Open ? 0 : -newSize.Width);

            }
            else
            {
                VisualStateManager.GoToState(this, "Narrow", true);

            }
        }

        private void GridControlManipulationCompleted(Object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Grid controlGrid = sender as Grid;
            if (controlGrid == null) return;
            if (controlGrid.Name == "OpenGridControl")
            {
                if (e.Velocities.Linear.X > 0) _currentState = DataSelectionState.Open;
                else
                    if (_currentState == DataSelectionState.Open && e.Velocities.Linear.X < 0)
                    _currentState = DataSelectionState.None;
            }
            else
            {
                if (e.Velocities.Linear.X < 0) _currentState = DataSelectionState.Connect;
                else
                    if (_currentState == DataSelectionState.Connect && e.Velocities.Linear.X > 0)
                    _currentState = DataSelectionState.None;
            }
            Bindings.Update();
            OpenOpenAnimation.Begin();
        }

        private void GridControlManipulationDelta(Object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Grid controlGrid = sender as Grid;
            if (controlGrid == null) return;
            if (controlGrid.Name == "OpenGridControl")
            {
                (OpenGrid.RenderTransform as TranslateTransform).X =
                    MathUtils.Limit((OpenGrid.RenderTransform as TranslateTransform).X + e.Delta.Translation.X,
                        -DesiredSize.Width / 2, 0);
                (ConnectGrid.RenderTransform as TranslateTransform).X =
                    MathUtils.Limit((ConnectGrid.RenderTransform as TranslateTransform).X + e.Delta.Translation.X,
                        DesiredSize.Width / 2, DesiredSize.Width);
            }
            else
            {
                (ConnectGrid.RenderTransform as TranslateTransform).X =
                    MathUtils.Limit((ConnectGrid.RenderTransform as TranslateTransform).X + e.Delta.Translation.X,
                        0, DesiredSize.Width / 2);
                (OpenGrid.RenderTransform as TranslateTransform).X =
                    MathUtils.Limit((OpenGrid.RenderTransform as TranslateTransform).X + e.Delta.Translation.X,
                        -DesiredSize.Width, -DesiredSize.Width / 2);
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
            _currentState = (sender as Grid)?.Name == "OpenGridControl"
                               ? (_currentState == DataSelectionState.None
                                      ? DataSelectionState.Open
                                      : DataSelectionState.None)
                               : (_currentState == DataSelectionState.None
                                      ? DataSelectionState.Connect
                                      : DataSelectionState.None);
            Bindings.Update();
            OpenOpenAnimation.Begin();
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
        Wide,
        Narrow
    }
}
