using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Utils;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace WindowsApp2._0.Controls
{
    public sealed partial class Chart2D
    {
        #region Properties

        Dictionary<Int32, Double> Points = new Dictionary<Int32, Double>();

        public Int32 DataLength => Points.Count;

        public Brush PlotAreaBackground
        {
            get { return (Brush) PlotArea.GetValue(Panel.BackgroundProperty); }
            set { PlotArea.SetValue(Panel.BackgroundProperty, value); }
        }

        public new Brush Background
        {
            get { return (Brush) MainGrid.GetValue(Panel.BackgroundProperty); }
            set { MainGrid.SetValue(Panel.BackgroundProperty, value); }
        }
        #endregion

        public Chart2D()
        {
            InitializeComponent();
            SizeChanged += (s, e) => ReRender();
            Loaded += (s, e) =>
            {
                ReRender();
            };
        }

        void ReRender()
        {
            PlotArea.Width = 3 * RenderSize.Width / 4;
            PlotArea.Height = 3 * RenderSize.Height / 4;

            //foreach (var pair in Points)
            //{
            //    Debug.WriteLine($"f({pair.Key}) = {pair.Value}");
            //}

            if (Points.Count > 0)
            {

                VisualTreeHelper.DisconnectChildrenRecursive(PlotArea);
                VisualTreeHelper.DisconnectChildrenRecursive(XAxisLabels);
                VisualTreeHelper.DisconnectChildrenRecursive(YAxisLabels);

                Double xStep = PlotArea.Width / (MathUtils.Limit(Points.Keys.Last() - Points.Keys.First(), 5, Points.Keys.Last() - Points.Keys.First()));
                Double yStep = PlotArea.Width / (MathUtils.Limit(Points.Values.Max() - Points.Values.Min(), 5, Points.Values.Max() - Points.Values.Min()));
                Double xAddStep = MathUtils.NearestToMultipleOf(Points.Keys.Last() - Points.Keys.First(), 5);
                Double yAddStep = MathUtils.NearestToMultipleOf(Points.Values.Max() - Points.Values.Min(), 5);

                for(Double i = 0; i < MathUtils.Limit(Points.Keys.Last() - Points.Keys.First(), 5, Points.Keys.Last() - Points.Keys.First()); i += xAddStep/5)
                {
                    PlotArea.Children.Add(new Line
                    {
                        X1 = i * xStep,
                        Y1 = 0,
                        X2 = i * xStep,
                        Y2 = PlotArea.RenderSize.Height,
                        Stroke = new SolidColorBrush(Colors.LightGray),
                        StrokeThickness = 0.75
                    });
                    XAxisLabels.Children.Add(new TextBlock
                    {
                        Text = i.ToString(),
                        Margin = new Thickness(i * xStep, 0, 0, 0)
                    });
                }

                for(Double i = 0; i < MathUtils.Limit(Points.Values.Max() - Points.Values.Min(), 5, Points.Values.Max() - Points.Values.Min()); i += yAddStep / 5)
                {
                    PlotArea.Children.Add(new Line
                    {
                        X1 = 0,
                        Y1 = PlotArea.RenderSize.Height - i * yStep,
                        X2 = PlotArea.RenderSize.Width,
                        Y2 = PlotArea.RenderSize.Height - i * yStep,
                        Stroke = new SolidColorBrush(Colors.LightGray),
                        StrokeThickness = 0.75
                    });
                    YAxisLabels.Children.Add(new TextBlock
                    {
                        Text = i.ToString(),
                        Margin = new Thickness(0, YAxisLabels.RenderSize.Height - i * yStep, 0, 0)
                    });
                }

                IEnumerator<KeyValuePair<Int32, Double>> pointsEnumerator = Points.GetEnumerator();
                pointsEnumerator.MoveNext();
                KeyValuePair<Int32, Double> nextPair, currentPair = pointsEnumerator.Current;
                while (pointsEnumerator.MoveNext())
                {
                    nextPair = pointsEnumerator.Current;

                    PlotArea.Children.Add(new Line
                    {
                        X1 = (currentPair.Key - Points.Keys.First()) * xStep,
                        Y1 = PlotArea.Height - (currentPair.Value - Points.Values.Min()) * yStep,
                        X2 = (nextPair.Key - Points.Keys.First()) * xStep,
                        Y2 = PlotArea.Height - (nextPair.Value - Points.Values.Min()) * yStep,
                        Stroke = new SolidColorBrush(Colors.White),
                        StrokeThickness = 3
                    });

                    currentPair = nextPair;
                }
            }
        }

        public void Clear() => Points.Clear();

        public void Push(Int32 x, Double y)
        {
            if (Points.Count < 1 || x > Points.Keys.Last()) Points.Add(x, y);
            ReRender();
        }
        public Double? Pop()
        {
            if (Points.Count == 0) return null;
            Double output = Points.Values.First();
            Points.Remove(Points.Keys.First());
            ReRender();
            return output;
        }

    }
}
