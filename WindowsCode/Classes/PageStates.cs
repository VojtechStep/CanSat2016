using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Windows.Storage;
using Windows.UI.Xaml;

namespace WindowsCode.Classes
{
    public static class MesurementState
    {
        public static ObservableCollection<RecentItem> RecentItems { get; set; } = new ObservableCollection<RecentItem>();
        public static MesurementItem CurrentItem { get; set; }
    }

    public static class DataState
    {
        public static String OutputFileToken;
        public static CancellationTokenSource ReadCancellationTokenSource;
        public const UInt32 CommandLength = 34;
        public static ReadOnlyDictionary<String, Byte> SendCommands = new ReadOnlyDictionary<String, Byte>(new Dictionary<String, Byte>()
        {
            {"StartMesurement", 0x67 },
            {"EndMesurement", 0x68 },
            {"RequestSample", 0x69 }
        });
        public static ReadOnlyDictionary<String, Byte> ReceiveCommands = new ReadOnlyDictionary<String, Byte>(new Dictionary<String, Byte>()
        {
            {"Init", 0x04 },
            {"Start", 0x05 },
            {"Pause", 0x06 },
            {"End", 0x07 },
            {"PacketStart", 0x08 },
            {"PacketEnd", 0x09 }
        });
    }

    public static class SettingsState
    {
        public static ElementTheme AppCurrentTheme;
        public static Byte GRange = 16;

        public static void SaveTheme()
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            ApplicationDataContainer CanSatSettings = roamingSettings.CreateContainer("CanSatSettings", ApplicationDataCreateDisposition.Always);
            roamingSettings.Containers["CanSatSettings"].Values["IntTheme"] = AppCurrentTheme == ElementTheme.Default ? 0 : (AppCurrentTheme == ElementTheme.Dark ? 1 : 2);
        }
    }

}
