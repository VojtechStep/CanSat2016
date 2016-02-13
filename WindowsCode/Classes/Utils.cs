using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
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

        public static SolidColorBrush Invert (this SolidColorBrush brush)
        {
            return new SolidColorBrush(Invert(brush.Color));
        }

        public static ElementTheme AbsoluteRequestedTheme()
        {
            if ((Window.Current.Content as MainPage).RequestedTheme == ElementTheme.Default)
            {
                return Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
            }
            else return (Window.Current.Content as MainPage).RequestedTheme;
        }

        public static Windows.Storage.ApplicationDataCompositeValue ToDataCompositeValue(this RecentItem ri)
        {
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();

            return composite;
        }

    }

    public enum Theme : int
    {
        Automatic,
        Dark,
        Light,
    }
    
    public enum BufferPosition
    {
        Normal,
        Start,
        End
    }
}
