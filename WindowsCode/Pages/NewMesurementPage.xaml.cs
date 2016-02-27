using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WindowsCode.Classes;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
        SerialDevice serialPort;
        DataReader dataReaderObject;
        DataWriter dataWriterObject;

        public NewMesurementPage()
        {
            this.InitializeComponent();
            GetSerialPorts();
            DataState.ReadCancellationTokenSource?.Cancel();
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
            DataState.ReadCancellationTokenSource = new CancellationTokenSource();
            await Communication.ConnectAsync((await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelector()))[deviceIndex].Id);

            try
            {
                Byte[] InitBuffer = new Byte[DataState.SerialReadyCall.Length];
                await Communication.ReadAsync(DataState.ReadCancellationTokenSource.Token, InitBuffer);
                String ReceivedString = new String(InitBuffer.Select(p => (Char)p).ToArray());

                if (ReceivedString == DataState.SerialReadyCall)
                {
                    VisualStateManager.GoToState(Window.Current.Content as MainPage, "Connected", true);

                    Boolean listening = false;
                    while (true)
                    {
                        Byte[] DataBuffer = new Byte[DataState.CommandLength];
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
                        else
                        {

                        }
                    }
                }
            } catch(TaskCanceledException) { }
            finally
            {
                Communication.Disconnect();
                VisualStateManager.GoToState(Window.Current.Content as MainPage, "Disconnected", true);
            }
        }
    }
}
