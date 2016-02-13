using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static WindowsCode.Classes.Utils;
using WindowsCode.Classes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsCode.Pages
{

    public sealed partial class MesurementsPage : Page
    {

        ObservableCollection<RecentItem> RecentItems;


        public void AddRecent(RecentItem item)
        {
            if (item.Removable && !item.HasRemoveHandler) item.Remove += RemoveRecent;
            if (item.Pinnable && !item.HasPinHandler) item.PinnedToggle += PinToggle;

            if ((Window.Current.Content as MainPage).MesurementState.RecentItems.Contains(item))
                (Window.Current.Content as MainPage).MesurementState.RecentItems.Remove(item);
            InsertRecent(item, (Window.Current.Content as MainPage).MesurementState.RecentItems);
            this.RecentItems = (Window.Current.Content as MainPage).MesurementState.RecentItems;
            Bindings.Update();
        }

        public void RemoveRecent(object sender, EventArgs e)
        {
            RecentItem item = (RecentItem)sender;
            if ((Window.Current.Content as MainPage).MesurementState.RecentItems.Contains(item))
                (Window.Current.Content as MainPage).MesurementState.RecentItems.Remove(item);
            this.RecentItems = (Window.Current.Content as MainPage).MesurementState.RecentItems;
            Bindings.Update();
        }

        public void PinToggle(object sender, EventArgs e)
        {
            RecentItem item = (RecentItem)sender;
            item.Position = item.Pinned ? BufferPosition.Start : BufferPosition.Normal;
            AddRecent(item);
        }

        public static void InsertRecent(RecentItem item, ObservableCollection<RecentItem> col)
        {
            if (item.Position == BufferPosition.End) col.Insert(col.Count - col.Count(p => p.Position == BufferPosition.End && p.Fixed), item);
            else
            {
                Int32 startItemsCount = col.Count(p => p.Position == BufferPosition.Start);
                if (item.Position == BufferPosition.Start) col.Insert(startItemsCount, item);
                else col.Insert(startItemsCount + col.Count(p => p.Position == BufferPosition.Normal), item);
            }
        }

        public MesurementsPage()
        {
            this.InitializeComponent();

            if ((Window.Current.Content as MainPage).MesurementState.RecentItems.Count == 0)
            {
                (Window.Current.Content as MainPage).MesurementState.RecentItems = new ObservableCollection<RecentItem>();
                setupRecentItems();
            }

            foreach (RecentItem item in (Window.Current.Content as MainPage).MesurementState.RecentItems)
                item.Bg = AbsoluteRequestedTheme() == ElementTheme.Dark ? Colors.Yellow : Colors.Navy;
            RecentItems = (Window.Current.Content as MainPage).MesurementState.RecentItems;
            Bindings.Update();
        }

        private void setupRecentItems()
        {
            RecentItem NewRecentItem = new RecentItem
            {
                Name = "Add New",
                Icon = "\uE109",
                Position = BufferPosition.End,
                Fixed = true,
                Removable = false,
                Pinnable = false,
                Bg = AbsoluteRequestedTheme() == ElementTheme.Dark ? Colors.Yellow : Colors.Navy
            };
            NewRecentItem.Click += (s, a) =>
            {
                (Window.Current.Content as MainPage).GoToPage(typeof(NewMesurementPage));
            };

            AddRecent(NewRecentItem);
        }
    }

}


