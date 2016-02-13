using WindowsCode.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
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
using Windows.Media.Audio;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WindowsCode.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CSSyncPage : Page
    {

        SerialDevice serialPort;
        public List<CSVInfo> Data;
        DateTime receptionStart;
        DateTime receptionEnd;
        Int32 deviceIndex;
        AudioDeviceOutputNode audioOut;

        public CSSyncPage()
        {
            InitializeComponent();
        }

        private void AddData(String rawCSV)
        {
            Data.Add(new CSVInfo(rawCSV));
        }

        public async void StartTest()
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
                            Data = new List<CSVInfo>();
                            receptionStart = DateTime.UtcNow;
                            listening = true;
                        }
                        else if (listening)
                        {
                            if (line.ToString().Contains("END"))
                            {
                                receptionEnd = DateTime.UtcNow;
                                listening = false;
                                serialPort.Dispose();
                                break;
                            }
                            else
                            {
                                AddData(line.ToString());
                            }
                        }
                        line = new StringBuilder();
                    }
                }
            }
            Debug.WriteLine($"Reception time: {(receptionEnd - receptionStart).Seconds} sec");
            Debug.WriteLine($"Received: {Data.Count} packets");
            Debug.WriteLine($"Transfer speed: {Data.Count / (receptionEnd - receptionStart).Seconds} transfers/sec");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            deviceIndex = (Int32)e.Parameter;
            StartTest();
        }
    }
}
