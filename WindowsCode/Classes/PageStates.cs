using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
        public static DataStreamState CurrentStreamState;
        public static CancellationTokenSource ReadCancellationTokenSource;
        public static UInt32 CommandLength = 32;
        public static String SerialReadyCall = "BOOT";
        public static Byte InitByte = 0x67;
        public static Byte EndByte = 0x68;
    }

    public static class MapState
    {

    }

    public static class SettingsState
    {

    }

}
