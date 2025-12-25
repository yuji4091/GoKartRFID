using System;
using System.Collections.Generic;
using System.Linq;

namespace GoKartLapCounter
{
    /// <summary>
    /// 计圈引擎 - 核心业务逻辑
    /// </summary>
    public class LapCounterEngine
    {
        private DatabaseManager db;
        private Dictionary<string, RunningSession> runningSessions; // KartID -> Session
        private Dictionary<string, DateTime> lastDetectionTime;     // EPC -> LastTime (防重复)

        public int AntiDuplicateSeconds { get; set; } = 10; // 防重复间隔(秒)

        // 事件: 记录了新的一圈
        public event EventHandler<LapRecordedEventArgs> LapRecorded;

        // 事件: 会话完成
        public event EventHandler<SessionCompletedEventArgs> SessionCompleted;

        public LapCounterEngine(DatabaseManager database)
        {
            this.db = database;
            this.runningSessions = new Dictionary<string, RunningSession>();
            this.lastDetectionTime = new Dictionary<string, DateTime>();
        }

        /// <summary>
        /// 开始一个新的计圈会话
        /// </summary>
        public bool StartSession(string kartID, int targetLaps = 20)
        {
            try
            {
                // 如果已有运行中的会话,不允许重复开始
                if (runningSessions.ContainsKey(kartID))
                {
                    return false;
                }

                // 在数据库中创建会话
                int sessionID = db.StartSession(kartID, targetLaps);

                // 添加到运行中的会话
                runningSessions[kartID] = new RunningSession
                {
                    SessionID = sessionID,
                    KartID = kartID,
                    TargetLaps = targetLaps,
                    CurrentLaps = 0,
                    StartTime = DateTime.Now,
                    LastLapTime = null
                };

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 将 EPC HEX 字符串转换为 ASCII (例如: "41413031" → "AA01")
        /// </summary>
        private string ParseEPCToKartID(string epcHex)
        {
            try
            {
                // 如果 EPC 长度小于8，直接返回原值
                if (string.IsNullOrEmpty(epcHex) || epcHex.Length < 8)
                    return epcHex;

                // 取前8个字符 (4个字节 = 4个ASCII字符)
                string hexPart = epcHex.Substring(0, 8);
                
                // 转换为 ASCII
                string result = "";
                for (int i = 0; i < hexPart.Length; i += 2)
                {
                    string hexByte = hexPart.Substring(i, 2);
                    int value = Convert.ToInt32(hexByte, 16);
                    result += (char)value;
                }
                
                return result;
            }
            catch
            {
                // 转换失败，返回原值
                return epcHex;
            }
        }

        /// <summary>
        /// 处理 RFID 标签检测 (核心计圈逻辑)
        /// </summary>
        public void ProcessTagDetection(string epc)
        {
            // 1. 防重复检测
            if (!IsValidDetection(epc))
            {
                return;
            }

            // 2. 解析 EPC 为 KartID (HEX → ASCII)
            string kartID = ParseEPCToKartID(epc);

            // 3. 检查是否为有效的 KartID (AA01-AA20)
            if (string.IsNullOrEmpty(kartID) || kartID.Length < 4)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid tag format: {epc}");
                return;
            }

            // 4. 如果没有活动会话，自动创建一个 (默认 20 圈)
            if (!runningSessions.ContainsKey(kartID))
            {
                int defaultTargetLaps = 20;
                StartSession(kartID, defaultTargetLaps);
                System.Diagnostics.Debug.WriteLine($"Auto-started session for {kartID}, target: {defaultTargetLaps} laps");
            }

            // 5. 记录这一圈
            RecordLap(kartID);
        }

        /// <summary>
        /// 检查是否为有效检测 (防重复)
        /// </summary>
        private bool IsValidDetection(string epc)
        {
            DateTime now = DateTime.Now;

            if (lastDetectionTime.ContainsKey(epc))
            {
                TimeSpan interval = now - lastDetectionTime[epc];
                if (interval.TotalSeconds < AntiDuplicateSeconds)
                {
                    // 间隔太短,认为是重复检测
                    return false;
                }
            }

            // 更新最后检测时间
            lastDetectionTime[epc] = now;
            return true;
        }

        /// <summary>
        /// 记录一圈
        /// </summary>
        private void RecordLap(string kartID)
        {
            var session = runningSessions[kartID];
            DateTime lapTime = DateTime.Now;

            // 计算圈数
            session.CurrentLaps++;
            int lapNumber = session.CurrentLaps;

            // 计算圈时
            double elapsedSeconds = 0;
            if (session.LastLapTime.HasValue)
            {
                elapsedSeconds = (lapTime - session.LastLapTime.Value).TotalSeconds;
            }
            session.LastLapTime = lapTime;

            // 保存到数据库
            db.RecordLap(session.SessionID, kartID, lapNumber, lapTime, elapsedSeconds);

            // 触发事件
            LapRecorded?.Invoke(this, new LapRecordedEventArgs
            {
                KartID = kartID,
                LapNumber = lapNumber,
                LapTime = lapTime,
                ElapsedSeconds = elapsedSeconds,
                TargetLaps = session.TargetLaps,
                IsCompleted = lapNumber >= session.TargetLaps,
                StartTime = session.StartTime
            });

            // 检查是否完成
            if (lapNumber >= session.TargetLaps)
            {
                CompleteSession(kartID);
            }
        }

        /// <summary>
        /// 完成会话
        /// </summary>
        private void CompleteSession(string kartID)
        {
            if (!runningSessions.ContainsKey(kartID))
                return;

            var session = runningSessions[kartID];

            // 更新数据库
            db.EndSession(session.SessionID);

            // 触发完成事件
            SessionCompleted?.Invoke(this, new SessionCompletedEventArgs
            {
                KartID = kartID,
                TotalLaps = session.CurrentLaps,
                StartTime = session.StartTime,
                EndTime = DateTime.Now
            });

            // 从运行中移除
            runningSessions.Remove(kartID);
        }

        /// <summary>
        /// 手动停止会话
        /// </summary>
        public void StopSession(string kartID)
        {
            if (runningSessions.ContainsKey(kartID))
            {
                var session = runningSessions[kartID];
                db.EndSession(session.SessionID);
                runningSessions.Remove(kartID);
            }
        }

        /// <summary>
        /// 获取所有运行中的会话
        /// </summary>
        public List<RunningSession> GetRunningSessions()
        {
            return runningSessions.Values.ToList();
        }

        /// <summary>
        /// 获取指定卡丁车的会话
        /// </summary>
        public RunningSession GetSession(string kartID)
        {
            return runningSessions.ContainsKey(kartID) ? runningSessions[kartID] : null;
        }

        /// <summary>
        /// 清空所有会话
        /// </summary>
        public void ClearAllSessions()
        {
            foreach (var kartID in runningSessions.Keys.ToList())
            {
                StopSession(kartID);
            }
        }

        /// <summary>
        /// 重置会话 (仅内存移除)
        /// </summary>
        public void ResetSession(string kartID)
        {
            if (runningSessions.ContainsKey(kartID))
            {
                runningSessions.Remove(kartID);
            }
        }
    }

    /// <summary>
    /// 运行中的会话数据
    /// </summary>
    public class RunningSession
    {
        public int SessionID { get; set; }
        public string KartID { get; set; }
        public int TargetLaps { get; set; }
        public int CurrentLaps { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? LastLapTime { get; set; }
    }

    /// <summary>
    /// 记录圈数事件参数
    /// </summary>
    public class LapRecordedEventArgs : EventArgs
    {
        public string KartID { get; set; }
        public int LapNumber { get; set; }
        public DateTime LapTime { get; set; }
        public double ElapsedSeconds { get; set; }
        public int TargetLaps { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime StartTime { get; set; }
    }

    /// <summary>
    /// 会话完成事件参数
    /// </summary>
    public class SessionCompletedEventArgs : EventArgs
    {
        public string KartID { get; set; }
        public int TotalLaps { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
