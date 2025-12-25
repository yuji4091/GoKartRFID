using System;
using UHF;

namespace GoKartLapCounter
{
    /// <summary>
    /// CF-815 RFID Reader Wrapper - Based on Official Demo V6.2
    /// </summary>
    public class CF815Reader
    {
        // Global variables (reference official example)
        private static byte fComAdr = 0xff;
        private static int frmcomportindex = 0;

        // Two different callbacks (reference official example line 153-154)
        private RFIDCallBack rfidCallback;   // For InitRFIDCallBack
        private RfidTagCallBack tagCallback;  // For StartInventory
        
        private bool isConnected = false;
        private string currentPort = "";

        // Event: Tag Detected
        public event EventHandler<TagDetectedEventArgs> TagDetected;

        public bool IsConnected => isConnected;
        public string CurrentPort => currentPort;

        public CF815Reader()
        {
            // Initialize both callbacks (reference official Form1 constructor)
            rfidCallback = new RFIDCallBack(OnTagDetectedIntPtr);
            tagCallback = new RfidTagCallBack(OnTagDetected);
        }

        /// <summary>
        /// RFID callback (for InitRFIDCallBack) - signature: (IntPtr, Int32)
        /// </summary>
        private void OnTagDetectedIntPtr(IntPtr p, Int32 nEvt)
        {
            // This is called by InitRFIDCallBack, but we don't use it for now
            // We use tagCallback in StartInventory instead
        }

        /// <summary>
        /// Auto search and connect to reader
        /// </summary>
        public bool AutoConnect()
        {
            // Try common VCP ports (COM3-COM10)
            for (int portNum = 3; portNum <= 10; portNum++)
            {
                System.Diagnostics.Debug.WriteLine($"Trying COM{portNum}...");
                if (Connect(portNum))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Connected to COM{portNum}!");
                    return true;
                }
                System.Diagnostics.Debug.WriteLine($"❌ COM{portNum} failed");
            }
            return false;
        }

        /// <summary>
        /// Connect to specified port (reference official btConnect232_Click)
        /// </summary>
        public bool Connect(int portNum)
        {
            try
            {
                // If already connected, disconnect first
                if (isConnected)
                {
                    Disconnect();
                }

                // Try Index 4 first (115200bps, official default)
                byte baud = 4;  // Direct index value, let SDK handle it
                fComAdr = 255;  // Broadcast address, auto find device

                System.Diagnostics.Debug.WriteLine($"  → Trying baud index {baud} (115200bps)...");
                int result = RWDev.OpenComPort(portNum, ref fComAdr, baud, ref frmcomportindex);
                System.Diagnostics.Debug.WriteLine($"  → OpenComPort returned: {result}, frmcomportindex: {frmcomportindex}, fComAdr: {fComAdr}");

                // If failed, try Index 3 (57600bps)
                if (result != 0)
                {
                    baud = 3;  // 57600bps
                    fComAdr = 255;
                    System.Diagnostics.Debug.WriteLine($"  → Retry with baud index {baud} (57600bps)...");
                    result = RWDev.OpenComPort(portNum, ref fComAdr, baud, ref frmcomportindex);
                    System.Diagnostics.Debug.WriteLine($"  → OpenComPort returned: {result}, frmcomportindex: {frmcomportindex}, fComAdr: {fComAdr}");
                }

                if (result == 0) // Success
                {
                    isConnected = true;
                    currentPort = $"COM{portNum}";
                    
                    System.Diagnostics.Debug.WriteLine($"  ✅ Connection established!");
                    
                    // **Key**: Initialize callback after connection (official line 1788)
                    if (frmcomportindex > 0)
                    {
                        RWDev.InitRFIDCallBack(rfidCallback, true, frmcomportindex);
                    }
                    
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  ❌ All baud rates failed, error code: {result}");
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  ❌ Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Connect to specified port name (e.g. "COM3")
        /// </summary>
        public bool Connect(string portName)
        {
            try
            {
                int portNum = int.Parse(portName.Replace("COM", ""));
                return Connect(portNum);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disconnect
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (isConnected && frmcomportindex > 0)
                {
                    StopScanning();
                    RWDev.CloseSpecComPort(frmcomportindex);
                    isConnected = false;
                    currentPort = "";
                    frmcomportindex = 0;
                }
            }
            catch { }
        }

        /// <summary>
        /// Start scanning tags (using RWDev.StartInventory)
        /// </summary>
        public bool StartScanning()
        {
            if (!isConnected) return false;

            try
            {
                byte Target = 0;
                int result = RWDev.StartInventory(ref fComAdr, Target, tagCallback, frmcomportindex);
                return result == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stop scanning (using RWDev.StopInventory)
        /// </summary>
        public bool StopScanning()
        {
            if (!isConnected) return false;

            try
            {
                int result = RWDev.StopInventory(ref fComAdr, frmcomportindex);
                return result == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tag detection callback (reference official GetEPC method, line 174)
        /// </summary>
        private void OnTagDetected(RFIDTag tag)
        {
            if (string.IsNullOrEmpty(tag.UID))
                return;

            // Trigger event, notify upper layer application
            TagDetected?.Invoke(this, new TagDetectedEventArgs
            {
                EPC = tag.UID,
                RSSI = tag.RSSI.ToString(),
                DetectedTime = DateTime.Now
            });
        }

        /// <summary>
        /// Get available serial ports
        /// </summary>
        public static string[] GetAvailablePorts()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }
    }

    /// <summary>
    /// Tag detection event parameters
    /// </summary>
    public class TagDetectedEventArgs : EventArgs
    {
        public string EPC { get; set; }         // Tag EPC/UID
        public string RSSI { get; set; }        // Signal strength
        public DateTime DetectedTime { get; set; } // Detection time
    }
}
