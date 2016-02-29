using System;
using System.Collections.ObjectModel;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowsCode.Classes;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

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

        public NewMesurementPage()
        {
            this.InitializeComponent();
            GetSerialPorts();
            FilePathSelector.RegisterPropertyChangedCallback(TextBox.TextProperty, CheckIfValid);
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
            if (String.IsNullOrWhiteSpace(FilePathSelector.Text))
                valid = false;
            if (valid && Ports.Count <= 0)
                valid = false;
            if (String.IsNullOrWhiteSpace(DataState.OutputFileToken))
                valid = false;
            if (!String.IsNullOrWhiteSpace(DataState.OutputFileToken) && !Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(DataState.OutputFileToken))
                valid = false;

            Done.IsEnabled = valid;
            PortSelector.IsEnabled = Ports.Count > 0;
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            HandleFile(FilePathSelector.Text);
            //StartMesurement(PortSelector.SelectedIndex);
            StartMesurement2(PortSelector.SelectedIndex);
            (Window.Current.Content as MainPage).DataTabVisibility = Visibility.Visible;
            (Window.Current.Content as MainPage).GoToPage(typeof(DataPage));
        }

        private async void HandleFile(String FilePath)
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
            MesurementState.CurrentItem.Data = new ObservableCollection<CSVData>();
            await FileIO.WriteTextAsync(await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(DataState.OutputFileToken), "UTC Time [hhmmss.ss],Temperature [\u00B0C],Pressure [mB],X Acceleration [Gs],Y Acceleration [Gs],Z Acceleration [Gs],Latitude [dddmm.mm],N/S Indicator,Longitude [dddmm.mm],W/E Indicator,Altitude [m]\n");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).GoBack();
        }

        private async void StartMesurement2(Int32 deviceIndex)
        {
            DataState.ReadCancellationTokenSource?.Cancel();
            DataState.ReadCancellationTokenSource = new CancellationTokenSource();
            await Communication.ConnectAsync((await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector()))[deviceIndex].Id, 115200);

            try
            {
                Byte[] InitBuffer = new Byte[1];
                await Communication.ReadAsync(DataState.ReadCancellationTokenSource.Token, InitBuffer);

                if (InitBuffer[0] == DataState.ReceiveCommands["Init"])
                {
                    VisualStateManager.GoToState(Window.Current.Content as MainPage, "Connected", false);

                    StringBuilder line = new StringBuilder();
                    Byte[] DataBuffer = new Byte[1];

                    while (true)
                    {
                        await Communication.ReadAsync(DataState.ReadCancellationTokenSource.Token, DataBuffer);
                        
                        if(DataBuffer[0] == DataState.ReceiveCommands["PacketStart"])
                        {
                            line = new StringBuilder();
                        } else if(DataBuffer[0] == DataState.ReceiveCommands["PacketEnd"])
                        {
                            MesurementState.CurrentItem.Data.Add(new CSVData(line.ToString()));
                            Debug.WriteLine(line.ToString());
                        }
                        else
                        {
                            line.Append((Char)DataBuffer[0]);
                        }
                    }
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                Communication.Disconnect();
                VisualStateManager.GoToState(Window.Current.Content as MainPage, "Disconnected", true);
            }
        }

        public async void StartMesurement(Int32 deviceIndex)
        {
            DataState.ReadCancellationTokenSource?.Cancel();
            DataState.ReadCancellationTokenSource = new CancellationTokenSource();
            await Communication.ConnectAsync((await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector()))[deviceIndex].Id);

            try
            {
                Byte[] InitBuffer = new Byte[1];
                await Communication.ReadAsync(DataState.ReadCancellationTokenSource.Token, InitBuffer);

                if (InitBuffer[0] == DataState.ReceiveCommands["Init"])
                {
                    VisualStateManager.GoToState(Window.Current.Content as MainPage, "Connected", true);

                    Boolean listening = false;
                    Boolean inPacket = false;
                    Byte[] Packet = new Byte[DataState.CommandLength - 2];
                    Byte DataPointer = 0;
                    while (true)
                    {
                        Byte[] DataBuffer = new Byte[1];
                        await Communication.ReadAsync(DataState.ReadCancellationTokenSource.Token, DataBuffer);
                        if (DataBuffer[0] == DataState.ReceiveCommands["Start"])
                        {
                            listening = true;
                            Debug.WriteLine("START");
                        }
                        else if (DataBuffer[0] == DataState.ReceiveCommands["Pause"])
                        {
                            listening = false;
                            Debug.WriteLine("PAUSE");
                        }
                        else if (DataBuffer[0] == DataState.ReceiveCommands["End"])
                        {
                            listening = false;
                            Debug.WriteLine("PAUSE");
                            if (!DataState.ReadCancellationTokenSource.IsCancellationRequested)
                                DataState.ReadCancellationTokenSource.Cancel();
                        }
                        else if (DataBuffer[0] == DataState.ReceiveCommands["PacketStart"])
                        {
                            inPacket = true;
                            Debug.WriteLine("In packet");
                            Packet = new Byte[DataState.CommandLength - 2];
                            DataPointer = 0;
                        }
                        else if (DataBuffer[0] == DataState.ReceiveCommands["PacketEnd"])
                        {
                            inPacket = false;
                            Debug.WriteLine("Outta packet");
                            Single UTCTime = BitConverter.ToSingle(Packet, 0);
                            Single Temp = BitConverter.ToSingle(Packet, 4);
                            Single Pres = BitConverter.ToSingle(Packet, 8);
                            Int16 XAcc = BitConverter.ToInt16(Packet, 12);
                            Int16 YAcc = BitConverter.ToInt16(Packet, 14);
                            Int16 ZAcc = BitConverter.ToInt16(Packet, 16);
                            Single Lat = BitConverter.ToSingle(Packet, 18);
                            Char NSIndicator = BitConverter.ToChar(Packet, 22);
                            Single Long = BitConverter.ToSingle(Packet, 23);
                            Char EWIndicator = BitConverter.ToChar(Packet, 27);
                            Single Alt = BitConverter.ToSingle(Packet, 28);
                            Debugger.Break();
                        }
                        else if (inPacket)
                        {
                            Packet[DataPointer++] = DataBuffer[0];
                        }
                    }
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                Communication.Disconnect();
                VisualStateManager.GoToState(Window.Current.Content as MainPage, "Disconnected", true);
            }
        }
    }
}
