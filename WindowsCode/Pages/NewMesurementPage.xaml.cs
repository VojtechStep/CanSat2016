using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WindowsCode.Classes;
using Windows.Storage;
using Windows.System;
using Windows.Storage.Pickers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsCode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewMesurementPage : Page
    {
        private ObservableCollection<DeviceInformation> _ports = new ObservableCollection<DeviceInformation>();
        SerialDevice serialPort;
        private Boolean _connectionsAvailable = false;
        private DateTime receptionStart;
        private DateTime receptionEnd;

        private Int32 sessionPackageCount;

        public NewMesurementPage()
        {
            this.InitializeComponent();
            GetSerialPorts();
        }

        private async void GetSerialPorts()
        {
            Ports = new ObservableCollection<DeviceInformation>();
            var selector = SerialDevice.GetDeviceSelector();
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

            foreach (DeviceInformation di in devices)
            {
                Ports.Add(di);
            }
            CheckIfValid();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetSerialPorts();
        }

        public ObservableCollection<DeviceInformation> Ports
        {
            get { return _ports; }
            set { _ports = value; }
        }

        public Boolean ConnectionsAvailable
        {
            get { return _connectionsAvailable; }
            set { _connectionsAvailable = value; }
        }

        private async void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker fpicker = new FolderPicker();
            fpicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fpicker.FileTypeFilter.Add(".txt");
            StorageFolder selectedFolder = await fpicker.PickSingleFolderAsync();
            if (selectedFolder != null)
            {
                FilePathSelector.Text = selectedFolder.Path;
            }
            CheckIfValid();
        }

        private void CheckIfValid()
        {
            Boolean valid = true;
            if (FilePathSelector.Text == null || FilePathSelector.Text == "") valid = false;
            if (FileNameSelector.Text == null || FileNameSelector.Text == "") valid = false;
            if (Ports.Count <= 0) valid = false;
            Done.IsEnabled = valid;
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
 //           StartTest(PortSelector.SelectedIndex);
            OpenFile((FileNameSelector.Text != "" ? FileNameSelector.Text : "NewMesurement") + ((TextBlock)FileExtensionSelector.SelectedItem).Text, FilePathSelector.Text ?? "");
            (Window.Current.Content as MainPage).DataTabVisibility = Visibility.Visible;
            (Window.Current.Content as MainPage).GoToPage(typeof(DataPage));
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).GoBack();
        }

        public void OpenFile(String FileName, String Path)
        {
            Debug.WriteLine($"{Path}\\{FileName}");
        }

        public async void StartTest(Int32 deviceIndex)
        {
            var selector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);
            var device = devices[deviceIndex];

            try
            {
                serialPort = await SerialDevice.FromIdAsync(device.Id.ToString());
            }
            catch (Exception) { throw; }

            StringBuilder line = new StringBuilder();
            Boolean listening = false;

            while (true)
            {
                var rBuffer = (new byte[1]).AsBuffer();
                await serialPort.InputStream.ReadAsync(rBuffer, 1, InputStreamOptions.Partial);

                if (rBuffer.Length > 0)
                {
                    if ((char)rBuffer.ToArray()[0] != '\n')
                    {
                        line.Append((char)rBuffer.ToArray()[0]);
                    }
                    else
                    {
                        if (line.ToString().Contains("START"))
                        {
                            receptionStart = DateTime.UtcNow;
                            sessionPackageCount = 0;
                            listening = true;
                        }
                        else if (listening)
                        {
                            if (line.ToString().Contains("END"))
                            {
                                listening = false;
                                serialPort.Dispose();
                                break;
                            }
                            else if (line.ToString().Contains("PAUSE"))
                            {
                                receptionEnd = DateTime.UtcNow;
                                listening = false;
                                Debug.WriteLine($"Reception time: {(receptionEnd - receptionStart).Seconds} sec");
                                Debug.WriteLine($"Received: {sessionPackageCount} packets");
                                Debug.WriteLine($"Transfer speed: {sessionPackageCount / (receptionEnd - receptionStart).Seconds} transfers/sec");
                            }
                            else
                            {
                                (Window.Current.Content as MainPage).DataState.Data.Add(new CSVInfo(line.ToString()));
                                sessionPackageCount++;
                            }
                        }
                        line = new StringBuilder();
                    }
                }
            }

        }
    }
}
