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

            try
            {
                serialPort = await SerialDevice.FromIdAsync(device.Id);
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;

                DataState.ReadCancellationTokenSource = new CancellationTokenSource();

                Listen();
            }
            catch (Exception e) { Debug.WriteLine(e.Message); }
        }

        private async void Listen()
        {
            try
            {
                if (serialPort != null)
                {
                    dataWriterObject = new DataWriter(serialPort.OutputStream);
                    dataReaderObject = new DataReader(serialPort.InputStream);

                    Byte[] InitBuffer = new Byte[DataState.SerialReadyCall.Length];

                    await ReadAsync(DataState.ReadCancellationTokenSource.Token, InitBuffer);
                    String InitBufferString = new String(InitBuffer.Select(p => (char)p).ToArray());

                    if (InitBufferString == DataState.SerialReadyCall)
                    {
                        if (await WriteAsync(DataState.InitByte, true))
                            while (true)
                            {
                                Byte[] DataBuffer = new Byte[DataState.CommandLength];
                                await ReadAsync(DataState.ReadCancellationTokenSource.Token, DataBuffer);
                                String InCommand = new String(DataBuffer.Select(p => (char)p).ToArray());
                            }
                        else Debug.WriteLine("Could not send init byte");
                    }
                }
            }
            catch (Exception e)
            {
                if (e.GetType().Name == "TaskCanceledException")
                {
                    ClosePort();
                }
            }
            finally
            {
                dataReaderObject?.DetachBuffer();
                dataReaderObject = null;
            }
        }

        private async Task ReadAsync(CancellationToken cancelToken, Byte[] InBuffer) => await ReadAsync(cancelToken, InBuffer, InBuffer.Length);

        private async Task ReadAsync(CancellationToken cancelToken, Byte[] InBuffer, Int32 Count)
        {
            if (Count <= 0) throw new ArgumentException("Count must be greater than 0", "Count");

            Task<UInt32> LoadAsyncTask;

            UInt32 ReadBufferLength = UInt32.Parse(Count.ToString());
            cancelToken.ThrowIfCancellationRequested();
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
            LoadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancelToken);

            UInt32 bytesRead = await LoadAsyncTask;
            if (bytesRead == Count)
            {
                dataReaderObject.ReadBytes(InBuffer);
            }
        }

        private void ClosePort()
        {
            serialPort?.Dispose();
            serialPort = null;
        }


        private async Task<Boolean> WriteAsync(Byte command) => await WriteAsync(new Byte[] { command }, false);

        private async Task<Boolean> WriteAsync(Byte command, Boolean shouldDetachBuffer) => await WriteAsync(new Byte[] { command }, shouldDetachBuffer);

        private async Task<Boolean> WriteAsync(Byte[] command) => await WriteAsync(command, false);

        private async Task<Boolean> WriteAsync(Byte[] command, Boolean shouldDetachBuffer)
        {
            if (command.Length <= 0) throw new ArgumentException("Command length must be greater than 0", "command.Length");
            Task<UInt32> StoreAsyncTask;
            dataWriterObject.WriteBytes(command);
            StoreAsyncTask = dataWriterObject.StoreAsync().AsTask();
            UInt32 bytesWritten = await StoreAsyncTask;
            if (shouldDetachBuffer)
            {
                dataWriterObject.DetachStream();
                dataWriterObject = null;
            }
            return bytesWritten == command.Length;
        }

    }
}
