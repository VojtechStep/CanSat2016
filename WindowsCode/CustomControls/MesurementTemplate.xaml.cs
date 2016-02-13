using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WindowsCode.CustomControls
{
    public sealed partial class MesurementTemplate : UserControl
    {

        private Classes.RecentItem RecentItem { get { return this.DataContext as Classes.RecentItem; } }

        private double PinRotation
        {
            get
            {
                if (RecentItem != null && RecentItem.Pinned) return 90;
                return 0;
            }
        }

        public MesurementTemplate()
        {
            this.InitializeComponent();
            DataContextChanged += (s, a) =>
            {
                if (RecentItem != null && !RecentItem.Removable) Remove.Visibility = Visibility.Collapsed;
                else Remove.Visibility = Visibility.Visible;
                if (RecentItem != null && !RecentItem.Pinnable) Pin.Visibility = Visibility.Collapsed;
                else Pin.Visibility = Visibility.Visible;
                Bindings.Update();
            };
            WrapperPanel.PointerReleased += PtrReleased;
        }


        private void PtrReleased(object sender, PointerRoutedEventArgs e)
        {
            RecentItem.OnClick(EventArgs.Empty);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            RecentItem.OnRemoveRequested(EventArgs.Empty);
        }

        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            RecentItem.Pinned = !RecentItem.Pinned;
            Bindings.Update();
        }
    }
}
