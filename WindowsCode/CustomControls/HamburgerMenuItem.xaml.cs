using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WindowsCode.CustomControls
{
    public sealed partial class HamburgerMenuItem : UserControl
    {
        public HamburgerMenuItem()
        {
            InitializeComponent();
            HamburgerMenuButton.Loaded += HamburgerMenuButton_Loaded;
            FontSize = (Double)Resources["DefaultFontSize"];
        }

        private void HamburgerMenuButton_Loaded(object sender, RoutedEventArgs e)
        {
            HamburgerMenuButton.Click += this.Click;
            HamburgerMenuButton.Click += HamburgerMenuButton_Click;
        }

        private void HamburgerMenuButton_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as MainPage).IsPaneOpen = false;
        }

        public String MenuItemIcon
        {
            get { return (String)GetValue(IconTextProperty); }
            set { SetValue(IconTextProperty, value); }
        }

        public String MenuItemLabel
        {
            get { return (String)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public String GroupName
        {
            get { return (String)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public Double MenuFontSize
        {
            get { return (Double)GetValue(MenuFontSizeProperty); }
            set { SetValue(MenuFontSizeProperty, value); }
        }

        public bool Selected
        {
            get { return (bool)HamburgerMenuButton.IsChecked; }
            set { HamburgerMenuButton.IsChecked = value; }
        }


        public static readonly DependencyProperty IconTextProperty = DependencyProperty.Register("IconText", typeof(String), typeof(HamburgerMenuItem), null);
        
        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText", typeof(String), typeof(HamburgerMenuItem), null);

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(String), typeof(HamburgerMenuItem), null);

        public static readonly DependencyProperty ConverterParameter = DependencyProperty.Register("ConverterParameter", typeof(object), typeof(Binding), null);

        public static readonly DependencyProperty MenuFontSizeProperty = DependencyProperty.Register("MenuFontSize", typeof(Double), typeof(HamburgerMenuItem), new PropertyMetadata(0));
        
        public event RoutedEventHandler Click;
    }
}
