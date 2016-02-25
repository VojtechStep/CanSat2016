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
        private Boolean _connectionsAvailable = false;

        private Int32 iteration;

        public NewMesurementPage()
        {
            this.InitializeComponent();
            GetSerialPorts();
            FilePathSelector.RegisterPropertyChangedCallback(TextBox.TextProperty, CheckIfValid);
            iteration = 0;
        }

        private void CheckIfValid(DependencyObject sender, DependencyProperty dp)
        {
            CheckIfValid();
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
            Bindings.Update();
            PortSelector.SelectedIndex = PortSelector.Items.Count - 1;
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
            FileSavePicker fpicker = new FileSavePicker();
            fpicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            fpicker.SuggestedFileName = "MyMesurement";
            fpicker.DefaultFileExtension = ".csv";
            fpicker.FileTypeChoices.Add("Comma Separated Values", new String[] { ".csv" });
            fpicker.FileTypeChoices.Add("Text", new String[] { ".txt" });
            fpicker.FileTypeChoices.Add("CanSat Mesurement", new String[] { ".csmes" });
            StorageFile output = await fpicker.PickSaveFileAsync();
            if (output != null)
            {
                DataState.OutputFileToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(output);
                FilePathSelector.Text = output.Path;
            }
            CheckIfValid();
        }

        private void CheckIfValid()
        {
            Boolean valid = true;
            if (String.IsNullOrWhiteSpace(FilePathSelector.Text)) valid = false;
            if (valid && Ports.Count <= 0) valid = false;

            Done.IsEnabled = valid;
            PortSelector.IsEnabled = Ports.Count > 0;
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            HandleFile(FilePathSelector.Text);
            StartMesurement(PortSelector.SelectedIndex);
            (Window.Current.Content as MainPage).DataTabVisibility = Visibility.Visible;
            (Window.Current.Content as MainPage).GoToPage(typeof(DataPage));
        }

        private void HandleFile(String FilePath)
        {
            String Location = FilePath.Remove(FilePath.LastIndexOf("\\") + 1);
            String Name = FilePath.Remove(0, FilePath.LastIndexOf("\\") - 1);
            MesurementItem item = new MesurementItem()
            {
                Location = Location,
                Name = Name
            };
            RecentItem.AddItem(item);
            MesurementState.CurrentItem = item;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).GoBack();
        }

        public async void StartMesurement(Int32 deviceIndex)
        {
            var selector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);
            var device = devices[deviceIndex];


            using (SerialDevice serialPort = await SerialDevice.FromIdAsync(device.Id.ToString()))
            {
                serialPort.BaudRate = 115200;
                await serialPort.OutputStream.WriteAsync((new byte[] { 0x67 }).AsBuffer());


                StringBuilder line = new StringBuilder();
                Boolean listening = false;

                while (true)
                {
                    var rBuffer = (new byte[1]).AsBuffer();
                    await serialPort.InputStream.ReadAsync(rBuffer, 1, InputStreamOptions.Partial);
                    try
                    {
                        if ((char)rBuffer.ToArray()[0] != '\n')
                        {
                            line.Append((char)rBuffer.ToArray()[0]);
                        }
                        else
                        {
                            if (line.ToString().Contains("START"))
                            {
                                listening = true;
                                iteration++;
                            }
                            else if (listening)
                            {
                                if (line.ToString().Contains("END"))
                                {
                                    listening = false;
                                    break;
                                }
                                else if (line.ToString().Contains("PAUSE"))
                                {
                                    listening = false;
                                }
                                else
                                {
                                    DataState.Data.Add(new CSVData(line.ToString()));
                                }
                            }
                            line = new StringBuilder();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

    }
}
