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
            StartMesurement(PortSelector.SelectedIndex);
            (Window.Current.Content as MainPage).DataTabVisibility = Visibility.Visible;
            (Window.Current.Content as MainPage).GoToPage(typeof(DataPage));
        }

        private async void HandleFile(String FilePath)
        {
            String[] PathComponents = FilePath.Split('\\');
            String Name = PathComponents[PathComponents.Length - 1];
            string Location = FilePath.Remove(FilePath.Length - Name.Length);
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

        private async void StartMesurement(Int32 deviceIndex)
        {
            DataState.ReadCancellationTokenSource?.Cancel();
            DataState.ReadCancellationTokenSource = new CancellationTokenSource();
            String deviceId = (await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector()))[deviceIndex].Id;
            await Communication.ConnectAsync(deviceId, 115200);
            Debug.WriteLine("Connected");
            try
            {
                byte[] InitBuffer = new Byte[1];
                while (InitBuffer[0] != 0x04)
                    await Communication.ReadAsync(DataState.ReadCancellationTokenSource.Token, InitBuffer);
                Debug.WriteLine(InitBuffer[0]);
                VisualStateManager.GoToState(Window.Current.Content as MainPage, "Connected", true);

                StringBuilder line = new StringBuilder();
                Byte[] DataBuffer = new Byte[1];

                while (true)
                {
                    await Communication.ReadAsync(DataState.ReadCancellationTokenSource.Token, DataBuffer);
                    if (DataBuffer[0] == 0x08)
                    {
                        line = new StringBuilder();
                    }
                    else if (DataBuffer[0] == 0x09)
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
            catch (TaskCanceledException) { }
            finally
            {
                Communication.Disconnect();
                VisualStateManager.GoToState(Window.Current.Content as MainPage, "Disconnected", false);
            }
        }
    }
}
