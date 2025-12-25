using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;

namespace GoKartLapCounter
{
    public partial class MainForm : Form
    {
        private DatabaseManager db;
        private CF815Reader reader;
        private LapCounterEngine lapEngine;
        private Timer scanTimer;

        public MainForm()
        {
            InitializeComponent();
            InitializeSystem();
            InitializeKartList();
            
            // Hack: Increase row height
            System.Windows.Forms.ImageList imgList = new System.Windows.Forms.ImageList();
            imgList.ImageSize = new System.Drawing.Size(1, 48); // Height 48px to fit 20 items
            lvRunning.SmallImageList = imgList;

            // Start auto-connect timer
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        }

        private void InitializeSystem()
        {
            // 初始化数据库
            db = new DatabaseManager();

            // 初始化 RFID 读卡器
            reader = new CF815Reader();
            reader.TagDetected += Reader_TagDetected;

            // 初始化计圈引擎
            lapEngine = new LapCounterEngine(db);
            lapEngine.LapRecorded += Engine_LapRecorded;
            lapEngine.SessionCompleted += LapEngine_SessionCompleted;
            lapEngine.AntiDuplicateSeconds = 5; // Set to 5s for testing

            // 初始化扫描定时器
            scanTimer = new Timer();
            scanTimer.Interval = 1000; // 每秒扫描一次
            scanTimer.Tick += ScanTimer_Tick;

            // 初始化卡丁车列表 (AA01-AA20)
            for (int i = 1; i <= 20; i++)
            {
                string kartID = $"AA{i:D2}";
                cbKartID.Items.Add(kartID);
                
                // 自动注册到数据库 (如果还没有)
                // RFID 设置为与 KartID 相同,后续可以用工具修改
                db.AddKart(kartID, kartID, "Auto-registered");
            }
            if (cbKartID.Items.Count > 0)
                cbKartID.SelectedIndex = 0;

            // 初始化 UI
            UpdateConnectionStatus();
            
            // 预先添加所有卡丁车到列表 (AA01-AA20),默认红色
            InitializeKartList();
        }

        // ========== RFID 读卡器事件 ==========

        private void Reader_TagDetected(object sender, TagDetectedEventArgs e)
        {
            // 在 UI 线程中处理
            if (InvokeRequired)
            {
                Invoke(new Action(() => Reader_TagDetected(sender, e)));
                return;
            }

            // Always log first for debugging
            string displayID = e.EPC.Length >= 8 ? HexToAscii(e.EPC.Substring(0, 8)) : e.EPC;
            AddLog($"[{DateTime.Now:HH:mm:ss}] RAW TAG: {displayID} ({e.EPC})");

            // 交给计圈引擎处理
            lapEngine.ProcessTagDetection(e.EPC);
        }

        // ========== 计圈引擎事件 ==========

        private void Engine_LapRecorded(object sender, LapRecordedEventArgs e)
        {
            // Play strong beep sound (Use ThreadPool for .NET 3.5)
            System.Threading.ThreadPool.QueueUserWorkItem(s => Console.Beep(2000, 200));

            if (InvokeRequired)
            {
                Invoke(new Action(() => Engine_LapRecorded(sender, e)));
                return;
            }

            // Update UI display
            UpdateSessionInList(e.KartID, e.LapNumber, e.TargetLaps, e.StartTime); 
            
            // Add log
            string message = $"[{DateTime.Now:HH:mm:ss}] {e.KartID} completed lap {e.LapNumber} (time: {e.ElapsedSeconds:F1}s)";
            AddLog(message);
        }

        private void LapEngine_SessionCompleted(object sender, SessionCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LapEngine_SessionCompleted(sender, e)));
                return;
            }

            AddLog($"🎉 {e.KartID} finished {e.TotalLaps} laps!");
            
            // Long beep for finish
             System.Threading.ThreadPool.QueueUserWorkItem(s => {
                Console.Beep(1000, 150);
                Console.Beep(1000, 150);
                Console.Beep(1500, 400); 
            });

            MessageBox.Show($"Kart {e.KartID} Finished!\n\nTotal Laps: {e.TotalLaps}\nTime: {(e.EndTime - e.StartTime).TotalMinutes:F1} minutes",  
                "Completion Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateSessionInList(string kartID, int currentLap, int targetLaps, DateTime startTime)
        {
            foreach (ListViewItem item in lvRunning.Items)
            {
                if (item.Text == kartID)
                {
                    item.SubItems[1].Text = currentLap.ToString();
                    item.SubItems[2].Text = targetLaps.ToString();
                    item.SubItems[3].Text = startTime.ToString("HH:mm:ss");
                    
                    // Color Logic
                    if (currentLap >= targetLaps)
                    {
                        // Finished
                        item.BackColor = Color.Cyan;
                        item.ForeColor = Color.Black;
                    }
                    else if (currentLap == targetLaps - 1)
                    {
                        // Last Lap (Yellow)
                        item.BackColor = Color.Yellow;
                        item.ForeColor = Color.Black;
                    }
                    else
                    {
                        // Normal Running
                        item.BackColor = Color.LightGreen;
                        item.ForeColor = Color.Black;
                    }
                    
                    item.EnsureVisible(); // Auto scroll to latest
                    break;
                }
            }
        }

        // ========== 连接/断开 ==========

        private void btConnect_Click(object sender, EventArgs e)
        {
            if (reader.IsConnected)
            {
                Disconnect();
            }
            else
            {
                Connect();
            }
        }

        private void Connect()
        {
            AddLog("Connecting to RFID reader...");

            if (reader.AutoConnect())
            {
                AddLog($"✅ Connected! Port: {reader.CurrentPort}");
                reader.StartScanning(); // Start continuous scanning once
                scanTimer.Start(); // Keep timer for other potential uses
                UpdateConnectionStatus();
            }
            else
            {
                MessageBox.Show("Connection Failed!\n\nPlease check:\n1. CF-815 device is connected\n2. Driver is installed\n3. Device is powered on",
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Disconnect()
        {
            scanTimer.Stop();
            reader.Disconnect();
            AddLog("Disconnected");
            UpdateConnectionStatus();
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            if (reader.IsConnected)
            {
                // 不要重复调用 StartScanning，它可能会重置阅读器状态
                // reader.StartScanning(); 
            }
        }

        // ========== 开始计圈 ==========

        private void btStartLapCounting_Click(object sender, EventArgs e)
        {
            string kartID = cbKartID.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(kartID))
            {
                MessageBox.Show("Please select a kart!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int targetLaps = (int)nudTargetLaps.Value;

            if (lapEngine.StartSession(kartID, targetLaps))
            {
                AddLog($"✅ {kartID} started, target: {targetLaps} laps");
                AddSessionToList(kartID, 0, targetLaps);
            }
            else
            {
                MessageBox.Show($"{kartID} is already running!", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ========== UI 辅助方法 ==========

        private void UpdateConnectionStatus()
        {
            if (reader.IsConnected)
            {
                lblStatus.Text = $"● Connected ({reader.CurrentPort})";
                lblStatus.ForeColor = Color.Green;
                btConnect.Text = "Disconnect";
            }
            else
            {
                lblStatus.Text = "○ Not Connected";
                lblStatus.ForeColor = Color.Red;
                btConnect.Text = "Connect";
            }
        }

        private void AddLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AddLog(message)));
                return;
            }

            // Determine color based on content
            Color msgColor = Color.Lime; // Default
            
            if (message.Contains("RAW TAG"))
                msgColor = Color.Gray;
            else if (message.Contains("completed lap"))
                msgColor = Color.Lime;
            else if (message.Contains("finished") || message.Contains("Finished"))
                msgColor = Color.Cyan;
            else if (message.Contains("Connected"))
                msgColor = Color.White;
            else if (message.Contains("Disconnected") || message.Contains("Error"))
                msgColor = Color.Red;
            else if (message.Contains("session reset"))
                msgColor = Color.Yellow;

            // Append colored text
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = msgColor;
            txtLog.AppendText(message + Environment.NewLine);
            
            // Limit log length
            if (txtLog.TextLength > 50000)
            {
                txtLog.Select(0, txtLog.Text.IndexOf('\n', 10000) + 1);
                txtLog.SelectedText = "";
            }

            txtLog.ScrollToCaret();
        }

        private void InitializeKartList()
        {
            lvRunning.Items.Clear();
            
            // 添加所有 AA01-AA20,默认红色背景
            for (int i = 1; i <= 20; i++)
            {
                string kartID = $"AA{i:D2}";
                ListViewItem item = new ListViewItem(kartID);
                item.SubItems.Add("0");        // Current Lap
                item.SubItems.Add("20");       // Target Laps
                item.SubItems.Add("-");        // Start Time
                item.Name = kartID;
                item.BackColor = Color.LightCoral;  // 红色 (未活动)
                item.ForeColor = Color.White;
                lvRunning.Items.Add(item);
            }
        }

        private void LoadRunningSessionsToUI()
        {
            // TODO: 从数据库加载未完成的会话
        }

        private void AddSessionToList(string kartID, int currentLaps, int targetLaps)
        {
            ListViewItem item = new ListViewItem(kartID);
            item.SubItems.Add(currentLaps.ToString());
            item.SubItems.Add(targetLaps.ToString());
            item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss"));
            item.Name = kartID;
            lvRunning.Items.Add(item);
        }



        private void RemoveSessionFromList(string kartID)
        {
            foreach (ListViewItem item in lvRunning.Items)
            {
                if (item.Text == kartID)
                {
                    lvRunning.Items.Remove(item);
                    return;
                }
            }
        }

        private void btResetSession_Click(object sender, EventArgs e)
        {
            if (lvRunning.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a kart to reset.", "Tip");
                return;
            }

            foreach (ListViewItem item in lvRunning.SelectedItems)
            {
                string kartID = item.Text;
                
                // Clear engine state
                lapEngine.ResetSession(kartID);

                // Reset UI
                item.SubItems[1].Text = "0"; // Lap
                item.SubItems[3].Text = "-"; // StartTime
                item.BackColor = Color.LightCoral; // Red
                item.ForeColor = Color.White; // White text on red
                
                AddLog($"[{DateTime.Now:HH:mm:ss}] Session reset for {kartID}");
            }
            lvRunning.SelectedItems.Clear();
        }

        private string HexToAscii(string hex)
        {
            try
            {
                string result = "";
                for (int i = 0; i < hex.Length; i += 2)
                {
                    string hexByte = hex.Substring(i, 2);
                    int value = Convert.ToInt32(hexByte, 16);
                    result += (char)value;
                }
                return result;
            }
            catch
            {
                return hex;
            }
        }



        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            scanTimer?.Stop();
            reader?.Disconnect();
        }
    }
}