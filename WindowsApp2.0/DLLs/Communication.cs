using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace WindowsApp2._0.Utils
{
    class Communication : IDisposable
    {

        private SerialDevice serialPort;
        private DataWriter dataWriter;
        private DataReader dataReader;

        #region Connection morphs
        /// <summary>
        /// Initialize a connection with a USB device
        /// </summary>
        /// <param name="Token">Token to cancel the connection attempt</param>
        /// <param name="DeviceId">ID of the device you want to connect to</param>
        /// <param name="BaudRate">Baud rate of the connection</param>
        /// <param name="Parity">Parity of the connection</param>
        /// <param name="DataBits">Data bits of the connection</param>
        /// <param name="Handshake">Handshake of the connection</param>
        /// <param name="StopBits">Stop bits of the connection</param>
        /// <returns>Whether the connection was established</returns>
        public async Task<Boolean> ConnectAsync(CancellationToken Token, String DeviceId, UInt32 BaudRate, SerialParity Parity, UInt16 DataBits, SerialHandshake Handshake, SerialStopBitCount StopBits)
        {
            Token.ThrowIfCancellationRequested();
            serialPort = await SerialDevice.FromIdAsync(DeviceId);
            if (serialPort != null)
            {
                serialPort.BaudRate = BaudRate;
                serialPort.Parity = Parity;
                serialPort.DataBits = DataBits;
                serialPort.Handshake = Handshake;
                serialPort.StopBits = StopBits;

                dataWriter = new DataWriter(serialPort.OutputStream);
                dataReader = new DataReader(serialPort.InputStream);
                return true;
            }
            return false;

        }
        /// <summary>
        /// Initialize a connection with a USB device
        /// </summary>
        /// <param name="Token">Token to cancel the connection attempt</param>
        /// <param name="DeviceId">ID of the device you want to connect to</param>
        /// <param name="BaudRate">Baud rate of the connection</param>
        /// <returns>Whether the connection was established</returns>
        public async Task<Boolean> ConnectAsync(CancellationToken Token, String DeviceId, UInt32 BaudRate) => await ConnectAsync(Token, DeviceId, BaudRate, SerialParity.None, 8, SerialHandshake.None, SerialStopBitCount.One);
        /// <summary>
        /// Initialize a connection with a USB device
        /// </summary>
        /// <param name="Token">Token to cancel the connection attempt</param>
        /// <param name="DeviceId">ID of the device you want to connect to</param>
        /// <returns>Whether the connection was established</returns>
        public async Task<Boolean> ConnectAsync(CancellationToken Token, String DeviceId) => await ConnectAsync(Token, DeviceId, 9600);
        /// <summary>
        /// Initialize a connection with a USB device
        /// </summary>
        /// <param name="Timeout">Number of milliseonds to cancel the connection attempt after</param>
        /// <param name="DeviceId">ID of the device you want to connect to</param>
        /// <param name="BaudRate">Baud rate of the connection</param>
        /// <returns>Whether the connection was established</returns>
        public async Task<Boolean> ConnectAsync(Int32 Timeout, String DeviceId, UInt32 BaudRate) => await ConnectAsync(new CancellationTokenSource(Timeout).Token, DeviceId, BaudRate);
        /// <summary>
        /// Initialize a connection with a USB device
        /// </summary>
        /// <param name="Timeout">Number of milliseconds to cancel the connection attempt after</param>
        /// <param name="DeviceId">ID of the device you want to connect to</param>
        /// <returns>Whether the connection was established</returns>
        public async Task<Boolean> ConnectAsync(Int32 Timeout, String DeviceId) => await ConnectAsync(new CancellationTokenSource(Timeout).Token, DeviceId);
        /// <summary>
        /// Initialize a connection with a USB device
        /// </summary>
        /// <param name="Timeout">Time span to cancel the connection attempt after</param>
        /// <param name="DeviceId">ID of the device you want to connect to</param>
        /// <param name="BaudRate">Baud rate of the connection</param>
        /// <returns>Whether the connection was established</returns>
        public async Task<Boolean> ConnectAsync(TimeSpan Timeout, String DeviceId, UInt32 BaudRate) => await ConnectAsync(new CancellationTokenSource(Timeout).Token, DeviceId, BaudRate);
        /// <summary>
        /// Initialize a connection with a USB device
        /// </summary>
        /// <param name="Timeout">Time span to timeout after if the connection is taking too long to establish</param>
        /// <param name="DeviceId">ID of the device you want to connect to</param>
        /// <returns>Whether the connection was established</returns>
        public async Task<Boolean> ConnectAsync(TimeSpan Timeout, String DeviceId) => await ConnectAsync(new CancellationTokenSource(Timeout).Token, DeviceId);
        #endregion

        #region Stream attachment functions
        /// <summary>
        /// Use to reattach the data reader
        /// </summary>
        /// <returns>Whether the data reader was reattached</returns>
        public Boolean AttachReader()
        {
            dataReader = new DataReader(serialPort.InputStream);
            return true;
        }
        /// <summary>
        /// Use to reattach the data writer
        /// </summary>
        /// <returns>Whether the data writer was reattached</returns>
        public Boolean AttachWriter()
        {
            dataWriter = new DataWriter(serialPort.OutputStream);
            return true;
        }
        #endregion

        #region Reading morphs
        /// <summary>
        /// Read data from the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the reading operation</param>
        /// <param name="InBuffer">Buffer to fill with read data</param>
        /// <param name="Count">Number of bytes to read</param>
        /// <param name="ShouldDetachBuffer">Whether the function should detach the data reader after the operation</param>
        /// <returns>Whether the data was read correctly</returns>
        public async Task<Boolean> ReadAsync(CancellationToken Token, Byte[] InBuffer, UInt32 Count, Boolean ShouldDetachBuffer)
        {
            if (Count == 0)
                throw new ArgumentException("Buffer length must be greater than 0", nameof(Count));
            try
            {
                Token.ThrowIfCancellationRequested();
                dataReader.InputStreamOptions = InputStreamOptions.Partial;

                if (await dataReader.LoadAsync(Count).AsTask(Token) == Count)
                {
                    dataReader.ReadBytes(InBuffer);
                    if (ShouldDetachBuffer)
                    {
                        dataReader.DetachBuffer();
                        dataReader = null;
                    }
                    return true;
                }
                return false;
            }
            catch (OperationCanceledException) { return false; }
        }
        /// <summary>
        /// Read data from the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the reading operation</param>
        /// <param name="InBuffer">Buffer to fill with read data</param>
        /// <param name="Count">Number of bytes to read</param>
        /// <returns>Whether the data was read correctly</returns>
        public async Task<Boolean> ReadAsync(CancellationToken Token, Byte[] InBuffer, UInt32 Count) => await ReadAsync(Token, InBuffer, Count, false);
        /// <summary>
        /// Read data form the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the reading operation</param>
        /// <param name="InBuffer">Buffer to fill entirely with the data</param>
        /// <returns>Whether the data was read correctly</returns>
        public async Task<Boolean> ReadAsync(CancellationToken Token, Byte[] InBuffer) => await ReadAsync(Token, InBuffer, (UInt32)InBuffer.Length);
        /// <summary>
        /// Read data form the serial port
        /// </summary>
        /// <param name="Timeout">Time span to cancel the reading operation after</param>
        /// <param name="InBuffer">Buffer to fill entirely with the data</param>
        /// <returns>Whether the data was read correctly</returns>
        public async Task<Boolean> ReadAsync(TimeSpan Timeout, Byte[] InBuffer) => await ReadAsync(new CancellationTokenSource(Timeout).Token, InBuffer);
        /// <summary>
        /// Read data form the serial port
        /// </summary>
        /// <param name="Timeout">Number of milliseconds to cancel the reading operation after</param>
        /// <param name="InBuffer">Buffer to fill entirely with the data</param>
        /// <returns>Whether the data was read correctly</returns>
        public async Task<Boolean> ReadAsync(Int32 Timeout, Byte[] InBuffer) => await ReadAsync(new CancellationTokenSource(Timeout).Token, InBuffer);
        /// <summary>
        /// Read a single byte from the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the reading operation</param>
        /// <param name="ShouldDetachBuffer">Whether the function should detach the data reader after the operation</param>
        /// <returns>The byte read from the port</returns>
        public async Task<Byte?> ReadAsync(CancellationToken Token, Boolean ShouldDetachBuffer)
        {
            try
            {
                Token.ThrowIfCancellationRequested();
                dataReader.InputStreamOptions = InputStreamOptions.Partial;
                Byte? returnByte = null;
                if (await dataReader.LoadAsync(1).AsTask(Token) == 1)
                {
                    returnByte = dataReader.ReadByte();
                    if (ShouldDetachBuffer)
                    {
                        dataReader.DetachBuffer();
                        dataReader = null;
                    }
                }
                return returnByte;
            }
            catch (OperationCanceledException) { return null; }
        }
        /// <summary>
        /// Read a single byte from the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the reading operation</param>
        /// <returns>The byte read from the port</returns>
        public async Task<Byte?> ReadAsync(CancellationToken Token) => await ReadAsync(Token, false);
        /// <summary>
        /// Read a single byte from the serial port
        /// </summary>
        /// <param name="Timeout">Time span to cancel the reading operation  after</param>
        /// <returns>The byte read from the port</returns>
        public async Task<Byte?> ReadAsync(TimeSpan Timeout) => await ReadAsync(new CancellationTokenSource(Timeout).Token);
        /// <summary>
        /// Read a single byte from the serial port
        /// </summary>
        /// <param name="Timeout">Number of milliseconds to cancel the reading operation after</param>
        /// <returns>The byte read from the port</returns>
        public async Task<Byte?> ReadAsync(Int32 Timeout) => await ReadAsync(new CancellationTokenSource(Timeout).Token);
        #endregion

        #region Writing morphs
        /// <summary>
        /// Write a byte buffer to the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the writing operation</param>
        /// <param name="Command">Byte buffer to write to the port</param>
        /// <param name="Count">Number of bytes to write from the buffer</param>
        /// <param name="ShouldDetachBuffer">Whether the function should detach the data writer after the operation</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(CancellationToken Token, Byte[] Command, UInt32 Count, Boolean ShouldDetachBuffer)
        {
            if (Count <= 0 || Count > Command.Length)
                throw new ArgumentException("Count must be greater than 0 and not longer than the command length", nameof(Count));

            try
            {
                Token.ThrowIfCancellationRequested();
                dataWriter.WriteBytes(Command.Take((Int32)Count).ToArray());

                UInt32 BytesWritten = await dataWriter.StoreAsync().AsTask(Token);
                if (ShouldDetachBuffer)
                {
                    dataWriter.DetachBuffer();
                    dataWriter = null;
                }
                return BytesWritten == Count;
            }
            catch (OperationCanceledException) { return false; }
        }
        /// <summary>
        /// Write a byte buffer to the serial port
        /// </summary>
        /// <param name="Token">Token tho cancel the writing operation</param>
        /// <param name="Command">Byte buffer to write to the port</param>
        /// <param name="Count">Number of bytes to write from the buffer</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(CancellationToken Token, Byte[] Command, UInt32 Count) => await WriteAsync(Token, Command, Count, false);
        /// <summary>
        /// Write a byte buffer to the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the writing operation</param>
        /// <param name="Command">The byte buffer to write to the port</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(CancellationToken Token, Byte[] Command) => await WriteAsync(Token, Command, (UInt32)Command.Length);
        /// <summary>
        /// Write a byte buffer to the serial port
        /// </summary>
        /// <param name="Timeout">Time span to cancel the writing operation after</param>
        /// <param name="Command">Byte buffer to write to the port</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(TimeSpan Timeout, Byte[] Command) => await WriteAsync(new CancellationTokenSource(Timeout).Token, Command);
        /// <summary>
        /// Write a byte buffer to the serial port
        /// </summary>
        /// <param name="Timeout">Number of milliseconds to cancel the writing operation after</param>
        /// <param name="Command">Byte buffer to write to the port</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(Int32 Timeout, Byte[] Command) => await WriteAsync(new CancellationTokenSource(Timeout).Token, Command);
        /// <summary>
        /// Write a byte to the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the operation</param>
        /// <param name="Command">Byte to write to the port</param>
        /// <param name="ShouldDetachBuffer">Whether the function should detach the data writer after the operation</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(CancellationToken Token, Byte Command, Boolean ShouldDetachBuffer)
        {
            Token.ThrowIfCancellationRequested();

            try
            {
                dataWriter.WriteByte(Command);
                UInt32 ByteWritten = await dataWriter.StoreAsync().AsTask();
                if (ShouldDetachBuffer)
                {
                    dataWriter.DetachBuffer();
                    dataWriter = null;
                }
                return ByteWritten == 1;
            }
            catch (OperationCanceledException) { return false; }
        }
        /// <summary>
        /// Write a byte to the serial port
        /// </summary>
        /// <param name="Token">Token to cancel the operation</param>
        /// <param name="Command">Byte to write to the port</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(CancellationToken Token, Byte Command) => await WriteAsync(Token, Command, false);
        /// <summary>
        /// Write a byte to the serial port
        /// </summary>
        /// <param name="Timeout">Time span to cancel the operation after</param>
        /// <param name="Command">Byte to write to the port</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(TimeSpan Timeout, Byte Command) => await WriteAsync(new CancellationTokenSource(Timeout).Token, Command);
        /// <summary>
        /// Write a byte to the serial port
        /// </summary>
        /// <param name="Timeout">Number of milliseconds to cancel the operation after</param>
        /// <param name="Command">Byte to write to the port</param>
        /// <returns>Whether the data was written correctly</returns>
        public async Task<Boolean> WriteAsync(Int32 Timeout, Byte Command) => await WriteAsync(new CancellationTokenSource(Timeout).Token, Command);
        #endregion

        public void Dispose()
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
