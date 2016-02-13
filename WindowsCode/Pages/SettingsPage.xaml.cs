using WindowsCode.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
namespace WindowsCode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        private Boolean _signedIn = false;

        public Boolean SignedIn
        {
            get { return _signedIn; }
            set
            {
                _signedIn = value;
                Bindings.Update();
            }
        }
        

        public SettingsPage()
        {
            this.InitializeComponent();

            SignedIn = _signedIn;
            ThemePicker.SelectedIndex = (Window.Current.Content as MainPage).RequestedTheme == ElementTheme.Default ? 0 : ((Window.Current.Content as MainPage).RequestedTheme == ElementTheme.Dark ? 1 : 2);
            Version.Text = $"Version: {Resources["VerMajor"]}.{Resources["VerMinor"]}.{Resources["BuildNum"]}";
        }
        

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (Window.Current.Content as MainPage).RequestedTheme = (Theme)((ComboBox)sender).SelectedIndex == Theme.Dark ? ElementTheme.Dark : ((Theme)((ComboBox)sender).SelectedIndex == Theme.Light ? ElementTheme.Light : ElementTheme.Default);
        }

        private void SignInOD_Click(object sender, RoutedEventArgs e)
        {
            SignedIn = true;
        }

        private void SignOutOD_Click(object sender, RoutedEventArgs e)
        {
            SignedIn = false;
        }
    }
}
