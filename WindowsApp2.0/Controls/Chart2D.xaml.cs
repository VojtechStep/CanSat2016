using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WindowsApp2._0.Controls
{
    public sealed partial class Chart2D
    {
        #region Properties

        SortedDictionary<Int32, Double> Points;
    
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
            SizeChanged += (s, e) => ReRender(e.NewSize);
            Loaded += (s, e) =>
            {
                ReRender(DesiredSize);
            };
        }

        void ReRender(Size newSize)
        {
            PlotArea.Width = 3 * newSize.Width / 4;
            PlotArea.Height = 3 * newSize.Height / 4;

            //foreach(var x in Points.Keys)
            //{
            //    Debug.WriteLine($"f({x}) = {Points[x]}");
            //}

        }

        //public void Push(Int32 x, Double y) => Points.Add(x, y);
        public Double? Pop() {
            if (Points.Count == 0) return null;
            Double output = Points[Points.Keys.GetEnumerator().Current];
            Points.Remove(Points.Keys.GetEnumerator().Current);
            return output;
        } 

    }
}
