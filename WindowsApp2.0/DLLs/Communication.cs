using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace WindowsApp2._0.Utils
{
    class Communication
    {

        private static SerialDevice serialPort;
        private static DataWriter dataWriter;
        private static DataReader dataReader;


        public static async Task<Boolean> ConnectAsync(CancellationToken Token, String DeviceId, UInt32 BaudRate, TimeSpan WriteTimeout, TimeSpan ReadTimeout)
        {
            try
            {
                Token.ThrowIfCancellationRequested();
                serialPort = await SerialDevice.FromIdAsync(DeviceId);
                serialPort.BaudRate = BaudRate;
                serialPort.WriteTimeout = WriteTimeout;
                serialPort.ReadTimeout = ReadTimeout;

                if (serialPort != null)
                {
                    dataWriter = new DataWriter(serialPort.OutputStream);
                    dataReader = new DataReader(serialPort.InputStream);
                    return true;
                }
                return false;
            }
            catch (OperationCanceledException) { }
            Debug.WriteLine("Connected");
            return false;

        }
        public static async Task<Boolean> ConnectAsync(CancellationToken Token, String DeviceId, UInt32 BaudRate) => await ConnectAsync(Token, DeviceId, BaudRate, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));
        public static async Task<Boolean> ConnectAsync(CancellationToken Token, String DeviceId) => await ConnectAsync(Token, DeviceId, 9600, TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1000));

        public static async Task<Boolean> ConnectAsync(Int32 Timeout, String DeviceId) => await ConnectAsync(new CancellationTokenSource(Timeout).Token, DeviceId);
        public static async Task<Boolean> ConnectAsync(TimeSpan Timeout, String DeviceId) => await ConnectAsync(new CancellationTokenSource(Timeout).Token, DeviceId);

        public static Boolean AttachReader()
        {
            dataReader = new DataReader(serialPort.InputStream);
            return true;
        }

        public static Boolean AttachWriter()
        {
            dataWriter = new DataWriter(serialPort.OutputStream);
            return true;
        }

        public static async Task<Boolean> ReadAsync(CancellationToken CancelToken, Byte[] InBuffer, UInt32 Count)
        {
            if (Count == 0)
                throw new ArgumentException("Buffer length must be greater than 0", nameof(Count));

            try
            {
                CancelToken.ThrowIfCancellationRequested();
                dataReader.InputStreamOptions = InputStreamOptions.Partial;

                if (await dataReader.LoadAsync(Count).AsTask(CancelToken) == Count)
                {
                    dataReader.ReadBytes(InBuffer);
                    return true;
                }
                return false;
            }
            catch (OperationCanceledException) { }
            Debug.WriteLine("Read");
            return false;
        }
        public static async Task<Boolean> ReadAsync(CancellationToken CancelToken, Byte[] InBuffer) => await ReadAsync(CancelToken, InBuffer, (UInt32) InBuffer.Length);

        public static async Task<Boolean> ReadAsync(TimeSpan timeout, Byte[] InBuffer) => await ReadAsync(new CancellationTokenSource(timeout).Token, InBuffer);

        public static async Task<Boolean> ReadAsync(Int32 timeout, Byte[] InBuffer) => await ReadAsync(new CancellationTokenSource(timeout).Token, InBuffer);

        public static async Task<Byte?> ReadAsync(CancellationToken CancelToken)
        {
            try
            {
                CancelToken.ThrowIfCancellationRequested();
                dataReader.InputStreamOptions = InputStreamOptions.Partial;
                if (await dataReader.LoadAsync(1).AsTask(CancelToken) == 1)
                {
                    return dataReader.ReadByte();
                }
                return null;
            } catch (OperationCanceledException) { }
            Debug.WriteLine("Written");
            return null;
        }

        public static async Task<Byte?> ReadAsync(TimeSpan timeout) => await ReadAsync(new CancellationTokenSource(timeout).Token);

        public static async Task<Byte?> ReadAsync(Int32 timeout) => await ReadAsync(new CancellationTokenSource(timeout).Token);

        public static async Task<Boolean> WriteAsync(CancellationToken Token, Byte[] Command, UInt32 Count, Boolean ShouldDetachBuffer)
        {
            if (Count == 0 || Count > Command.Length)
                throw new ArgumentException("Count must be greater than 0 and not longer than the command length", nameof(Count));

            try
            {
                Token.ThrowIfCancellationRequested();
                Task<UInt32> StoreAsyncTask;
                dataWriter.WriteBytes(Command.Take((Int32) Count).ToArray());
                StoreAsyncTask = dataWriter.StoreAsync().AsTask(Token);

                UInt32 BytesWritten = await StoreAsyncTask;
                if (ShouldDetachBuffer)
                {
                    dataWriter.DetachBuffer();
                    dataWriter = null;
                }
                return BytesWritten == Count;
            } catch (OperationCanceledException) { }
            Debug.WriteLine("Written");
            return false;
        }
        public static async Task<Boolean> WriteAsync(CancellationToken Token, Byte[] Command, UInt32 Count) => await WriteAsync(Token, Command, Count, false);
        public static async Task<Boolean> WriteAsync(CancellationToken Token, Byte[] Command) => await WriteAsync(Token, Command, (UInt32) Command.Length, false);
        public static async Task<Boolean> WriteAsync(CancellationToken Token, Byte Command, Boolean ShouldDetachBuffer) => await WriteAsync(Token, new Byte[] { Command }, 1, ShouldDetachBuffer);
        public static async Task<Boolean> WriteAsync(CancellationToken Token, Byte Command) => await WriteAsync(Token, new Byte[] { Command }, 1, false);

        public static async Task<Boolean> WriteAsync(Int32 Timeout, Byte Command) => await WriteAsync(new CancellationTokenSource(Timeout).Token, Command);
        public static async Task<Boolean> WriteAsync(TimeSpan Timeout, Byte Command) => await WriteAsync(new CancellationTokenSource(Timeout).Token, Command);

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
            catch { Debug.WriteLine("Exception in Disconnect"); }
            Debug.WriteLine("Disconnected");
        }
    }
}
