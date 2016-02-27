using System;
using System.Collections.Generic;
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
        public static CancellationTokenSource ReadCancellationTokenSource;
        public const UInt32 CommandLength = 32;
        public static String SerialReadyCall = "BOOT";
        public static ReadOnlyDictionary<String, Byte> SendCommands = new ReadOnlyDictionary<String, Byte>(new Dictionary<String, Byte>()
        {
            {"StartMesurement", 0x67 },
            {"EndMesurement", 0x68 },
            {"RequestSample", 0x69 }
        });
        public static ReadOnlyDictionary<String, Byte> ReceiveCommands = new ReadOnlyDictionary<String, Byte>(new Dictionary<String, Byte>()
        {
            {"Start", 0x05 },
            {"Pause", 0x06 },
            {"End", 0x07 }
        });
    }

    public static class SettingsState
    {
        public static Byte GRange = 4;
    }

}
