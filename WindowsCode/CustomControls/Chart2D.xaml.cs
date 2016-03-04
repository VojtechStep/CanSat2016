using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using static WindowsCode.Classes.Utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WindowsCode.CustomControls
{
    public sealed partial class Chart2D : UserControl
    {
        public List<Double> Data { get; }

        public Int32 XMax { get; set; }
        public Int32 YMax { get; set; }
        public Int32 XMin { get; set; } = 0;
        public Int32 YMin { get; set; } = 0;

        public Double XUnit { get; set; } = 10;
        public Double YUnit { get; set; } = 10;

        public Double XUnitMax { get; set; } = 10;
        public Double YUnitMax { get; set; } = 10;

        public Int32 XAdditionalLinesCount { get; set; } = 5;
        public Int32 YAdditionalLinesCount { get; set; } = 5;

        private Double PlotAreaWidth { get; set; }
        private Double PlotAreaHeight { get; set; }

        public Boolean Autoscale { get; set; } = true;


        public Brush PlotAreaBackground { get; set; } = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
        public String Header { get; set; }
        public Thickness PlotAreaMargin { get; set; }
        public Double GraphStrokeThickness { get; set; } = 2;

        public String XAxisLabel { get; set; }
        public String YAxisLabel { get; set; }

        public Chart2D()
        {
            this.InitializeComponent();
            Data = new List<Double>();
            Loaded += (s, a) =>
            {
                Background = Background ?? new SolidColorBrush(Color.FromArgb(255, 20, 20, 20));
                PlotAreaMargin = PlotAreaMargin.Blow(20);
                PlotAreaWidth = Width - (PlotAreaMargin.Left + PlotAreaMargin.Right);
                PlotAreaHeight = Height - (PlotAreaMargin.Top + PlotAreaMargin.Bottom + (GraphLabel?.ActualHeight ?? 0) + (GraphLabel?.Margin.Top ?? 0) + (GraphLabel?.Margin.Bottom ?? 0));
                XMax = (Int32)(PlotAreaWidth / XUnit);
                YMax = (Int32)(PlotAreaHeight / YUnit);
                Bindings.Update();
                ReRender();
            };
        }

        public void Push(Double Val)
        {
            Data.Add(Val);

            XMax = (Int32)(PlotAreaWidth / XUnit);
            YMax = (Int32)(PlotAreaHeight / YUnit);

            if (Data.Count > XMax)
            {
                XMax = Data.Count;
                XUnit = Math.Min(PlotAreaWidth / XMax, XUnitMax);
            }
            if (Val > YMax)
            {
                YMax = (Int32)Math.Ceiling(Val);
                YUnit = Math.Min(PlotAreaHeight / YMax, YUnitMax);
            }
            ReRender();
        }

        private void ReRender()
        {
            MainCanvas.Children.Clear();
            PlotArea.Children.Clear();
            for (Int32 i = 1; i < Data.Count(); i++)
            {
                Line l = new Line();
                l.X1 = (i - 1) * XUnit;
                l.X2 = i * XUnit;
                l.Y1 = PlotAreaHeight - Data.ElementAt(i - 1) * YUnit;
                l.Y2 = PlotAreaHeight - Data.ElementAt(i) * YUnit;
                l.Stroke = new SolidColorBrush(Colors.White);
                l.Fill = new SolidColorBrush(Colors.White);
                l.StrokeEndLineCap = PenLineCap.Round;
                l.StrokeThickness = GraphStrokeThickness;
                PlotArea.Children.Add(l);
            }
            AddAdditionalLines();
            MainCanvas.Children.Add(GraphLabel);
            MainCanvas.Children.Add(PlotArea);
        }

        private void AddAdditionalLines()
        {
            Int32 XAdditionalLineSpacing = NearestToMultipleOf(XMax / XAdditionalLinesCount, 5);
            Int32 YAdditionalLineSpacing = NearestToMultipleOf(YMax / YAdditionalLinesCount, 5);
            for (Int32 xadd = 0; xadd < XMax; xadd += XAdditionalLineSpacing)
            {
                Line l = new Line();
                l.X1 = xadd * XUnit;
                l.X2 = xadd * XUnit;
                l.Y1 = PlotAreaHeight;
                l.Y2 = 0;
                l.Stroke = new SolidColorBrush(Colors.White);
                l.Fill = new SolidColorBrush(Colors.White);
                l.StrokeEndLineCap = PenLineCap.Flat;
                l.StrokeThickness = 0.5;
                TextBlock LineLabel = new TextBlock();
                LineLabel.Text = xadd.ToString();
                Double OffsetTop = PlotAreaHeight + PlotAreaMargin.Top + PlotAreaMargin.Bottom + 10;
                Double OffsetLeft = l.X1 + LineLabel.DesiredSize.Width + 20;
                LineLabel.Margin = new Thickness(OffsetLeft, OffsetTop, 0, 0);
                PlotArea.Children.Add(l);
                MainCanvas.Children.Add(LineLabel);
            }
            for (Int32 yadd = 0; yadd < YMax; yadd += YAdditionalLineSpacing)
            {
                Line l = new Line();
                l.X1 = 0;
                l.X2 = PlotAreaWidth;
                l.Y1 = PlotAreaHeight - yadd * YUnit;
                l.Y2 = PlotAreaHeight - yadd * YUnit;
                l.Stroke = new SolidColorBrush(Colors.White);
                l.Fill = new SolidColorBrush(Colors.White);
                l.StrokeEndLineCap = PenLineCap.Flat;
                l.StrokeThickness = 0.5;
                TextBlock LineLabel = new TextBlock();
                LineLabel.Text = yadd.ToString();
                LineLabel.TextAlignment = TextAlignment.Right;
                Double OffsetTop = PlotAreaMargin.Top + PlotAreaMargin.Bottom + l.Y1 -10;
                Double OffsetRight = PlotAreaMargin.Right + PlotAreaWidth;
                LineLabel.Margin = new Thickness(0, OffsetTop, OffsetRight, 0);
                PlotArea.Children.Add(l);
                //LineLabel.SizeChanged += (s, a) =>
                //{
                //    Double TextWidth = ((TextBlock)s).FontSize * ((TextBlock)s).Text.Length * 2 / 3;
                //    if (TextWidth > PlotAreaMargin.Left)
                //    {
                //        PlotAreaMargin = new Thickness(TextWidth, PlotAreaMargin.Top, PlotAreaMargin.Right, PlotAreaMargin.Bottom);
                //    }
                //    Bindings.Update();
                //};
                MainCanvas.Children.Add(LineLabel);
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            Push(Math.Pow(Data.Count(), 2));
        }

    }
}
