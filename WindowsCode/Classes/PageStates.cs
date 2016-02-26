using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

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
        public static IBuffer InitBuffer = (new byte[] { 0x67 }).AsBuffer();
        public static DataStreamState CurrentStreamState;
    }

    public static class MapState
    {

    }

    public static class SettingsState
    {

    }

}
