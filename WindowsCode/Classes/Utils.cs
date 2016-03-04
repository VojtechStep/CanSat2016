using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace WindowsCode.Classes
{
    public static class Utils
    {
        public static Boolean IsColorLight(this Color color)
        {
            Double brightness = ((color.R * 299) + (color.G * 587) + (color.B * 114)) / 1000;
            return (brightness >= 0.5);
        }

        public static Color Darken(this Color color, Int32 byHowMuch)
        {
            Byte Red = (Byte)(color.R - byHowMuch < 0 ? 0 : (color.R - byHowMuch > 255 ? 255 : color.R - byHowMuch));
            Byte Green = (Byte)(color.G - byHowMuch < 0 ? 0 : (color.G - byHowMuch > 255 ? 255 : color.G - byHowMuch));
            Byte Blue = (Byte)(color.B - byHowMuch < 0 ? 0 : (color.B - byHowMuch > 255 ? 255 : color.B - byHowMuch));
            return Color.FromArgb(color.A, Red, Green, Blue);
        }

        public static Color Lighten(this Color color, Int32 byHowMuch)
        {
            return Darken(color, -byHowMuch);
        }

        public static Color Invert(this Color color)
        {
            return Color.FromArgb(color.A, (Byte)(255 - color.R), (Byte)(255 - color.G), (Byte)(255 - color.B));
        }

        public static SolidColorBrush Invert(this SolidColorBrush brush)
        {
            return new SolidColorBrush(Invert(brush.Color));
        }

        public static ElementTheme AbsoluteRequestedTheme()
        {
            if ((Window.Current.Content as MainPage).RequestedTheme == ElementTheme.Default)
            {
                return Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
            }
            else
                return (Window.Current.Content as MainPage).RequestedTheme;
        }

        public static Windows.Storage.ApplicationDataCompositeValue ToDataCompositeValue(this MesurementItem ri)
        {
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();

            return composite;
        }

        public static Visibility ToVisibility(this Boolean vis)
        {
            return vis ? Visibility.Visible : Visibility.Collapsed;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var current in enumerable)
                action(current);
        }

        public static ObservableCollection<Point> GetTemp(this ObservableCollection<CSVData> db)
        {
            ObservableCollection<Point> points = new ObservableCollection<Point>();
            for (int i = 0; i < db.Count; i++)
            {
                points.Add(new Point(i, db.ElementAt(i).Temperature));
            }
            return points;
        }

        public static ObservableCollection<Point> GetPress(this ObservableCollection<CSVData> db)
        {
            ObservableCollection<Point> points = new ObservableCollection<Point>();
            for (int i = 0; i < db.Count; i++)
            {
                points.Add(new Point(i, db.ElementAt(i).Pressure));
            }
            return points;
        }

        public static Thickness Blow(this Thickness Original, Double AmountLeft, Double AmountTop, Double AmountRight, Double AmountBottom)
        {
            return new Thickness(Original.Left + AmountLeft, Original.Top + AmountTop, Original.Right + AmountRight, Original.Bottom + AmountBottom);
        }

        public static Thickness Blow(this Thickness Original, Double Amount)
        {
            return Original.Blow(Amount, Amount, Amount, Amount);
        }

        public static Thickness Shrink(this Thickness Original, Double AmountLeft, Double AmountTop, Double AmountRight, Double AmountBottom)
        {
            return Original.Blow(-AmountLeft, -AmountTop, -AmountRight, -AmountTop);
        }

        public static Thickness Shrink(this Thickness Original, Double Amount)
        {
            return Original.Shrink(Amount);
        }

        public static Int32 NearestToMultipleOf(Double Value, Int32 Multiple)
        {
            Double rest = Value % Multiple;
            if (rest < Multiple / 2 && Value != rest)
                return (Int32)(Value - rest);
            return (Int32)(Value - rest + Multiple);
        }

    }

    public enum Theme : int
    {
        Automatic,
        Dark,
        Light,
    }

    public enum QueryPosition
    {
        Normal,
        Start,
        End,
    }

    public enum DataStreamState
    {
        Receive,
        Close,
    }
}
