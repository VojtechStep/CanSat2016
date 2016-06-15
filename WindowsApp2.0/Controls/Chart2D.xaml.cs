using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WindowsApp2._0.Controls
{
    public sealed partial class Chart2D
    {
        #region Properties
        List<Double> Points;



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
            

        }

        public void Push(Double Value) => Points.Add(Value);
        public Double Pop() {
            Double output = Points[0];
            Points.Remove(Points[0]);
            return output;
        } 

    }
}
