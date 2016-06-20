﻿using System;
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

        Int32? _currentData;

        public Int32? CurrentData
        {
            get { return _currentData; }
            set
            {
                if (value >= Points.Keys.First() && value <= Points.Keys.Last())
                {
                    _currentData = value;
                    RenderCurrentPointer();
                }
                else { _currentData = null; }
            }
        }

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

        Boolean ReRenderEnabled { get; set; } = false;
        #endregion

        public Chart2D()
        {
            InitializeComponent();
            //SizeChanged += ReRenderWrapper;
            Loaded += (s, e) =>
            {
                ReRenderEnabled = true;
            };
        }
        

        void ReRender(Size newSize)
        {
            if (Visibility == Visibility.Visible && ReRenderEnabled)
            {
                PlotArea.Width = 3 * newSize.Width / 4;
                PlotArea.Height = 3 * newSize.Height / 4;

                if (Points.Count > 0)
                {
                    VisualTreeHelper.DisconnectChildrenRecursive(PlotArea);
                    VisualTreeHelper.DisconnectChildrenRecursive(XAxisLabels);
                    VisualTreeHelper.DisconnectChildrenRecursive(YAxisLabels);

                    var xStep = PlotArea.Width / (MathUtils.Limit(Points.Keys.Last() - Points.Keys.First(), 5, Points.Keys.Last() - Points.Keys.First()));
                    var yStep = PlotArea.Height / (MathUtils.Limit(Points.Values.Max() - Points.Values.Min(), 5, Points.Values.Max() - Points.Values.Min()));

                    for (Double i = Points.Keys.First(); i < Points.Keys.First() + 5 || i <= Points.Keys.Last(); i += (Points.Keys.Last() - Points.Keys.First() < 5) ? 1 : MathUtils.NearestToMultipleOf((Points.Keys.Last() - Points.Keys.First()) / 5, 5))
                    {
                        PlotArea.Children.Add(new Line
                        {
                            X1 = (i - Points.Keys.First()) * xStep,
                            Y1 = 0,
                            X2 = (i - Points.Keys.First()) * xStep,
                            Y2 = PlotArea.Height,
                            Stroke = new SolidColorBrush(Colors.LightGray),
                            StrokeThickness = 0.75
                        });

                        XAxisLabels.Children.Add(new TextBlock
                        {
                            Text = i.ToString(),
                            Margin = new Thickness((i - Points.Keys.First()) * xStep, 0, 0, 0)
                        });
                    }

                    for (Double i = Points.Values.Min(); i < Points.Values.Min() + 5 || i <= Points.Values.Max(); i += (Points.Values.Max() - Points.Values.Min() < 5) ? 1 : MathUtils.NearestToMultipleOf((Points.Values.Max() - Points.Values.Min()) / 5, 5))
                    {
                        PlotArea.Children.Add(new Line
                        {
                            X1 = 0,
                            Y1 = PlotArea.Height - (i - Points.Values.Min()) * yStep,
                            X2 = PlotArea.Width,
                            Y2 = PlotArea.Height - (i - Points.Values.Min()) * yStep,
                            Stroke = new SolidColorBrush(Colors.LightGray),
                            StrokeThickness = 0.75
                        });
                        YAxisLabels.Children.Add(new TextBlock
                        {
                            Text = i.ToString(),
                            VerticalAlignment = VerticalAlignment.Bottom,
                            RenderTransform = new TranslateTransform { Y = -(i - Points.Values.Min()) * yStep }
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
        }

        void RenderCurrentPointer()
        {

            try
            {
                PlotArea.Children.Remove(PlotArea.Children.OfType<Line>().First(p => p.Name == "CurrentDataPointer"));
                PlotArea.Children.Remove(PlotArea.Children.OfType<Line>().First(p => p.Name == "CurrentDataPointerY"));
            }
            catch (InvalidOperationException) { }

            if (CurrentData != null)
            {
                Int32 DesiredKey = Points.Keys.OrderBy(p => Math.Abs(p - (Double) CurrentData)).First();
                var xStep = PlotArea.Width / (MathUtils.Limit(Points.Keys.Last() - Points.Keys.First(), 5, Points.Keys.Last() - Points.Keys.First()));
                var yStep = PlotArea.Height / (MathUtils.Limit(Points.Values.Max() - Points.Values.Min(), 5, Points.Values.Max() - Points.Values.Min()));
                PlotArea.Children.Add(new Line
                {
                    Name = "CurrentDataPointer",
                    X1 = (DesiredKey - Points.Keys.First()) * xStep,
                    Y1 = 0,
                    X2 = (DesiredKey - Points.Keys.First()) * xStep,
                    Y2 = PlotArea.Height,
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 2
                });
                VisualTreeHelper.DisconnectChildrenRecursive(CurrentDataPointerLabelSpace);
                CurrentDataPointerLabelSpace.Children.Add(new TextBlock
                {
                    Text = DesiredKey.ToString(),
                    RenderTransform = new TranslateTransform { X = (DesiredKey - Points.Keys.First()) * xStep }
                });
                PlotArea.Children.Add(new Line
                {
                    Name = "CurrentDataPointerY",
                    X1 = 0,
                    Y1 = PlotArea.Height - (Points[DesiredKey] - Points.Values.Min()) * yStep,
                    X2 = PlotArea.Width,
                    Y2 = PlotArea.Height - (Points[DesiredKey] - Points.Values.Min()) * yStep,
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 1
                });
                VisualTreeHelper.DisconnectChildrenRecursive(CurrentDataPointerLabelSpaceY);
                CurrentDataPointerLabelSpaceY.Children.Add(new TextBlock
                {
                    Text = Points[DesiredKey].ToString(),
                    RenderTransform = new TranslateTransform { Y = PlotArea.Height - (Points[DesiredKey] - Points.Values.Min()) * yStep }
                });
            }
        }

        public void Clear()
        {
            Points.Clear();
            //ReRender(DesiredSize);
        }

        public void Push(Int32 x, Double y)
        {
            if (Points.Count < 1 || x > Points.Keys.Last()) Points.Add(x, y);
            ReRender(DesiredSize);
        }

        public void Push(Int32[] xs, Double[] ys)
        {
            if (xs.Length == ys.Length && xs.Length != 0 && (Points.Count < 1 || xs[0] > Points.Keys.Last()))
            {
                for (int i = 0; i < xs.Length; i++) if (Points.Count < 1 || xs[i] > Points.Keys.Last()) Points.Add(xs[i], ys[i]);
                ReRender(RenderSize);
            }
        }
        public void Push(IDictionary<Int32, Double> dict)
        {
            if (dict.Count > 0 && dict.First().Key > Points.Last().Key && dict.OrderByDescending(p => p.Key) == dict)
            {
                foreach (var pair in dict) Points.Add(pair.Key, pair.Value);
                ReRender(DesiredSize);
            }
        }

        public Double? Get(Int32 key)
        {
            Double output;
            return Points.TryGetValue(key, out output) ? (Double?) output : null;
        }

        public Double? Pop()
        {
            if (Points.Count == 0) return null;
            Double output = Points.Values.First();
            Points.Remove(Points.Keys.First());
            ReRender(DesiredSize);
            return output;
        }

    }
}
