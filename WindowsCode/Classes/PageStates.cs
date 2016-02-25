using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace WindowsCode.Classes
{
    public static class MesurementState
    {
        public static ObservableCollection<RecentItem> RecentItems { get; set; } = new ObservableCollection<RecentItem>();
        public static MesurementItem CurrentItem { get; set; }
    }

    public static class DataState
    {
        public static ObservableCollection<CSVData> Data = new ObservableCollection<CSVData>();
        public static String OutputFileToken;
        public static IBuffer ConnectionInitializationCommand = (new byte[] { (byte)'S' }).AsBuffer();
    }

    public static class MapState
    {

    }

    public static class SettingsState
    {

    }

}
