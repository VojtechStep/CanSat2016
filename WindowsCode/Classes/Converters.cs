using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace WindowsCode.Classes
{
    public class DoubleToGridLength : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new GridLength((double)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StringVerifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(String))
            {
                String output = value?.ToString();
                if(!String.IsNullOrWhiteSpace(output))
                {
                    return output;
                }
                else
                {
                    output = parameter.ToString();
                    if(!String.IsNullOrWhiteSpace(output))
                    {
                        return output;
                    }
                    else
                    {
                        return "Default";
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Incompatible types of {targetType} and {typeof(String)}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class Int32Verifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(Int32))
            {
                Int32 output;
                bool success = true;
                if (!Int32.TryParse(value.ToString(), out output))
                { if (!Int32.TryParse(parameter.ToString(), out output)) success = false; }
                if (success) return output;
                return 0;
            }
            else
            {
                throw new ArgumentException($"Incompatible types of {targetType} and {typeof(Int32)}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleVerifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(Double))
            {
                Double output;
                Boolean success = true;
                if (!Double.TryParse(value.ToString(), out output))
                { if (!Double.TryParse(parameter.ToString(), out output)) success = false; }
                if (success) return output;
                return 0D;
            }
            else
            {
                throw new ArgumentException($"Incompatible types of {targetType} and {typeof(Int32)}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(targetType == typeof(Brush))
            {
                Color color;
                if (value != null && (color = (Color)value) != null)
                {
                    return new SolidColorBrush(color);
                }
                else return new SolidColorBrush(Colors.White);
            }
            else
            {
                throw new ArgumentException($"Incompatible types of {targetType} and {typeof(SolidColorBrush)}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanNegate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(targetType == typeof(Boolean))
            {
                Boolean output;
                Boolean success = true;
                if (!Boolean.TryParse(value.ToString(), out output))
                { if (!Boolean.TryParse(parameter.ToString(), out output)) success = false; }
                if (success) return !output;
                return false;
            }
            else
            {
                throw new ArgumentException($"Incompatible types of {targetType} and {typeof(Boolean)}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(targetType == typeof(Visibility))
            {
                if ((Boolean)value)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            else
            {
                throw new ArgumentException($"Incompatible types of {targetType} and {typeof(Visibility)}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityN : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(Visibility))
            {
                if (!(Boolean)value)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            else
            {
                throw new ArgumentException($"Incompatible types of {targetType} and {typeof(Visibility)}");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
