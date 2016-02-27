using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace WindowsCode.Classes
{
    class Communication
    {

        private static SerialDevice serialPort;
        private static DataWriter dataWriter;
        private static DataReader dataReader;


        public static async Task ConnectAsync(String DeviceId, UInt32 BaudRate, TimeSpan WriteTimeout, TimeSpan ReadTimeout)
        {
            serialPort = await SerialDevice.FromIdAsync(DeviceId);
            serialPort.BaudRate = BaudRate;
            serialPort.WriteTimeout = WriteTimeout;
            serialPort.ReadTimeout = ReadTimeout;

            if (serialPort != null)
            {
                dataWriter = new DataWriter(serialPort.OutputStream);
                dataReader = new DataReader(serialPort.InputStream);
            }
        }
        public static async Task ConnectAsync(String DeviceId, UInt32 BaudRate) => await ConnectAsync(DeviceId, BaudRate, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));
        public static async Task ConnectAsync(String DeviceId) => await ConnectAsync(DeviceId, 9600, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));
        
        public static void AttachReader()
        {
            dataReader = new DataReader(serialPort.InputStream);
        }

        public static void AttachWriter()
        {
            dataWriter = new DataWriter(serialPort.OutputStream);
        }

        public static async Task ReadAsync(CancellationToken CancelToken, Byte[] InBuffer, UInt32 Count)
        {
            if (Count == 0)
                throw new ArgumentException("Buffer length must be greater than 0", "Count");
            Task<UInt32> LoadAsyncTask;

            CancelToken.ThrowIfCancellationRequested();
            dataReader.InputStreamOptions = InputStreamOptions.Partial;
            LoadAsyncTask = dataReader.LoadAsync(Count).AsTask(CancelToken);

            UInt32 BytesLoaded = await LoadAsyncTask;
            if(BytesLoaded == Count)
            {
                dataReader.ReadBytes(InBuffer);
            }
        }
        public static async Task ReadAsync(CancellationToken CancelToken, Byte[] InBuffer) => await ReadAsync(CancelToken, InBuffer, (UInt32)InBuffer.Length);

        public static async Task<Boolean> WriteAsync(Byte[] Command, UInt32 Count, Boolean ShouldDetachBuffer)
        {
            if (Count == 0 || Count > Command.Length)
                throw new ArgumentException("Count must be greater than 0 and not longer than the command length", "Count");

            Task<UInt32> StoreAsyncTask;
            dataWriter.WriteBytes(Command.Take((Int32)Count).ToArray());
            StoreAsyncTask = dataWriter.StoreAsync().AsTask();

            UInt32 BytesWritten = await StoreAsyncTask;
            if(ShouldDetachBuffer)
            {
                dataWriter.DetachBuffer();
                dataWriter = null;
            }
            return BytesWritten == Count;
        }
        public static async Task<Boolean> WriteAsync(Byte[] Command, UInt32 Count) => await WriteAsync(Command, Count, false);
        public static async Task<Boolean> WriteAsync(Byte[] Command) => await WriteAsync(Command, (UInt32)Command.Length, false);
        public static async Task<Boolean> WriteAsync(Byte Command, Boolean ShouldDetachBuffer) => await WriteAsync(new Byte[] { Command }, 1, ShouldDetachBuffer);
        public static async Task<Boolean> WriteAsync(Byte Command) => await WriteAsync(new Byte[] { Command }, 1, false);

        public static void Disconnect()
        {
            try
            {
                serialPort?.Dispose();
                serialPort = null;
                dataReader?.DetachBuffer();
                dataReader?.Dispose();
                dataReader = null;
                dataWriter?.DetachBuffer();
                dataWriter?.Dispose();
                dataWriter = null;
            }
            catch { }
        }
    }
}
