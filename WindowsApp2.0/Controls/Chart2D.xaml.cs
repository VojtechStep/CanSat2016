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

            if (Points.Count > 0)
            {

                VisualTreeHelper.DisconnectChildrenRecursive(PlotArea);
                VisualTreeHelper.DisconnectChildrenRecursive(XAxisLabels);
                VisualTreeHelper.DisconnectChildrenRecursive(YAxisLabels);

                var xStep = PlotArea.Width / (MathUtils.Limit(Points.Keys.Last() - Points.Keys.First(), 5, Points.Keys.Last() - Points.Keys.First()));
                var yStep = PlotArea.Height / (MathUtils.Limit(Points.Values.Max() - Points.Values.Min(), 5, Points.Values.Max() - Points.Values.Min()));
                
                for (Double i = 0; i < 5 || i <= Points.Keys.Last() - Points.Keys.First(); i += (Points.Keys.Last() - Points.Keys.First() < 5) ? 1 : MathUtils.NearestToMultipleOf((Points.Keys.Last() - Points.Keys.First()) / 5, 5))
                {
                    PlotArea.Children.Add(new Line
                    {
                        X1 = i * xStep,
                        Y1 = 0,
                        X2 = i * xStep,
                        Y2 = PlotArea.Width,
                        Stroke = new SolidColorBrush(Colors.LightGray),
                        StrokeThickness = 0.75
                    });

                    XAxisLabels.Children.Add(new TextBlock
                    {
                        Text = i.ToString(),
                        Margin = new Thickness(i * xStep, 0, 0, 0)
                    });
                }

                for (Double i = 0; i < 5 || i <= Points.Values.Max() - Points.Values.Min(); i += (Points.Values.Max() - Points.Values.Min() < 5) ? 1 : MathUtils.NearestToMultipleOf((Points.Values.Max() - Points.Values.Min()) / 5, 5))
                {
                    PlotArea.Children.Add(new Line
                    {
                        X1 = 0,
                        Y1 = PlotArea.Height - i * yStep,
                        X2 = PlotArea.Width,
                        Y2 = PlotArea.Height - i * yStep,
                        Stroke = new SolidColorBrush(Colors.LightGray),
                        StrokeThickness = 0.75
                    });
                    YAxisLabels.Children.Add(new TextBlock
                    {
                        Text = i.ToString(),
                        Margin = new Thickness(0, YAxisLabels.RenderSize.Height - i * yStep, 0, 0)
                    });
                }

                var pointsEnumerator = Points.GetEnumerator();
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

        Int32 GetLittleDelta(Int32 value)
        {
            Int32 output = 0;
            value /= 5;
            while (value >= 1)
            {
                value /= 5;
                output++;
            }
            return (Int32) MathUtils.Limit(output * 5, 1, output * 5);
        }

        Int32 GetLittleDelta2(Int32 value)
        {
            if (value < 5) return 1;
            return MathUtils.NearestToMultipleOf(value / 5, 5);
        }

        public void Clear() => Points.Clear();

        public void Push(Int32 x, Double y)
        {
            if (Points.Count < 1 || x > Points.Keys.Last()) Points.Add(x, y);
            ReRender();
        }

        public void Push(Int32[] xs, Double[] ys)
        {
            if (xs.Length == ys.Length && xs.Length != 0 && (Points.Count < 1 || xs[0] > Points.Keys.Last()))
            {
                for (int i = 0; i < xs.Length; i++) Points.Add(xs[i], ys[i]);
                ReRender();
            }
        }
        public void Push(IDictionary<Int32, Double> dict)
        {
            if (dict.Count > 0 && dict.First().Key > Points.Last().Key && dict.OrderByDescending(p => p.Key) == dict)
            {
                foreach (var pair in dict) Points.Add(pair.Key, pair.Value);
                ReRender();
            }
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
