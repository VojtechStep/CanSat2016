using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowsCode.Pages;

namespace WindowsCode.Classes
{

    public class RecentItem
    {
        public String Icon { get; set; }
        public String Name { get; set; }
        public QueryPosition Position { get; set; }

        public virtual void Clicked() { }

        public static void AddItem(RecentItem item)
        {
            if (item.Position == QueryPosition.End) MesurementState.RecentItems.Insert(MesurementState.RecentItems.Count, item);
            else
            {
                Int32 startItemsCount = MesurementState.RecentItems.Count(p => p.Position == QueryPosition.Start);
                if (item.Position == QueryPosition.Start) MesurementState.RecentItems.Insert(startItemsCount, item);
                else MesurementState.RecentItems.Insert(startItemsCount + MesurementState.RecentItems.Count(p => p.Position == QueryPosition.Normal), item);
            }
        }

        public static void RemoveItem(RecentItem item)
        {
            MesurementState.RecentItems.Remove(item);
        }

    }

    public class MesurementItem : RecentItem
    {
        public String Location { get; set; }
        public ObservableCollection<CSVInfo> Data { get; set; }
        public Boolean IsPinVisible { get; set; } = true;
        public Boolean IsTrashCanVisible { get; set; } = true;

        public override void Clicked()
        {
            //TODO Load data to the frame
        }

        public MesurementItem()
        {
            Icon = "\uE155";
            Name = "Mesurement";
            Position = QueryPosition.Normal;
        }
    }

    public class AddRecentItem : RecentItem
    {
        public override void Clicked()
        {
            (Window.Current.Content as MainPage).GoToPage(typeof(NewMesurementPage));
        }

        public AddRecentItem()
        {
            Icon = "\uE109";
            Name = "Add New";
            Position = QueryPosition.End;
        }
    }
}
