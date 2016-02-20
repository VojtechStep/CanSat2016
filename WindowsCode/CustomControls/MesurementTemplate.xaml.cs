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
using WindowsCode.Classes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WindowsCode.CustomControls
{
    public sealed partial class MesurementTemplate : UserControl
    {

        private RecentItem StoredItem { get { return this.DataContext as RecentItem; } }

        private double PinRotation
        {
            get
            {
                return StoredItem != null && StoredItem.Position == QueryPosition.Start ? 90 : 0;
            }
        }

        public MesurementTemplate()
        {
            this.InitializeComponent();
            DataContextChanged += (s, a) =>
            {
                Remove.Visibility = (StoredItem as MesurementItem)?.IsTrashCanVisible.ToVisibility() ?? Visibility.Collapsed;
                Pin.Visibility = (StoredItem as MesurementItem)?.IsPinVisible.ToVisibility() ?? Visibility.Collapsed;
                ToolTip tt = new ToolTip();
                tt.Content = (StoredItem as MesurementItem)?.Location ?? ((StoredItem as AddRecentItem) != null ? "Add new Mesurement" : null);
                WrapperPanel.SetValue(ToolTipService.ToolTipProperty, tt);
                Bindings.Update();
            };
            WrapperPanel.PointerReleased += PtrReleased;
        }


        private void PtrReleased(object sender, PointerRoutedEventArgs e)
        {
            StoredItem.Clicked();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            RecentItem.RemoveItem((RecentItem)((Button)sender).DataContext);
        }

        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            RecentItem item = ((Button)sender).DataContext as RecentItem;
            RecentItem.RemoveItem(item);
            item.Position = item.Position == QueryPosition.Normal ? QueryPosition.Start : QueryPosition.Normal;
            RecentItem.AddItem(item);
        }
    }
}
