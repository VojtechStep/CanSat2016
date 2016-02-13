using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace WindowsCode.Classes
{
    public class MesurementState
    {
        public ObservableCollection<RecentItem> RecentItems { get; set; } = new ObservableCollection<RecentItem>();
        public SolidColorBrush Background { get; set; } = new SolidColorBrush(Colors.White);
    }

    public class DataState
    {
        public SolidColorBrush Background { get; set; } = new SolidColorBrush(Colors.White);

    }

    public class MapState
    {
        public SolidColorBrush Background { get; set; } = new SolidColorBrush(Colors.White);

    }

    public class SettingsState
    {
        public SolidColorBrush Background { get; set; } = new SolidColorBrush(Colors.White);

    }

}
