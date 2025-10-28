using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScaleDriver.Core
{
    /// <summary>
    /// Reads data from serial port (RS232)
    /// </summary>
    public class SerialReader : IDisposable
    {
        private SerialPort _serialPort;
        private StringBuilder _buffer;
        private bool _isReading;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        public string PortName { get; private set; }
        public int BaudRate { get; private set; }
        public bool IsConnected => _serialPort?.IsOpen ?? false;

        public SerialReader()
        {
            _buffer = new StringBuilder();
        }

        /// <summary>
        /// Opens the serial port with specified settings
        /// </summary>
        public void Open(string portName, int baudRate = 9600, Parity parity = Parity.None, 
            int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                Close();
            }

            PortName = portName;
            BaudRate = baudRate;

            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = 500,
                WriteTimeout = 500,
                Encoding = Encoding.ASCII
            };

            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;

            _serialPort.Open();
        }

        /// <summary>
        /// Starts reading data asynchronously
        /// </summary>
        public void StartReading()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Serial port is not open");

            if (_isReading)
                return;

            _isReading = true;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Stops reading data
        /// </summary>
        public void StopReading()
        {
            _isReading = false;
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Closes the serial port
        /// </summary>
        public void Close()
        {
            StopReading();

            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!_isReading)
                return;

            try
            {
                var data = _serialPort.ReadExisting();
                _buffer.Append(data);

                // Check for complete frames (lines ending with newline or carriage return)
                var bufferContent = _buffer.ToString();
                var lines = bufferContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (bufferContent.EndsWith("\r") || bufferContent.EndsWith("\n"))
                {
                    // We have complete lines
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            OnDataReceived(line.Trim());
                        }
                    }
                    _buffer.Clear();
                }
                else if (lines.Length > 1)
                {
                    // Process all complete lines, keep the last incomplete one
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(lines[i]))
                        {
                            OnDataReceived(lines[i].Trim());
                        }
                    }
                    _buffer.Clear();
                    _buffer.Append(lines[lines.Length - 1]);
                }
            }
            catch (Exception ex)
            {
                OnErrorOccurred(ex);
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            OnErrorOccurred(new Exception($"Serial port error: {e.EventType}"));
        }

        protected virtual void OnDataReceived(string data)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs { Data = data });
        }

        protected virtual void OnErrorOccurred(Exception ex)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs { Exception = ex });
        }

        public void Dispose()
        {
            Close();
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public string Data { get; set; }
    }

    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
    }
}
