using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace GoKartLapCounter
{
    /// <summary>
    /// 简化的数据库管理类 - 专注计圈核心功能
    /// </summary>
    public class DatabaseManager
    {
        private string dbPath;
        private string connectionString;

        public DatabaseManager()
        {
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GoKartLapCounter.db");
            connectionString = $"Data Source={dbPath};Version=3;";
            InitializeDatabase();
        }

        /// <summary>
        /// 初始化数据库 - 只创建3张核心表
        /// </summary>
        private void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 表1: 卡丁车-RFID映射表
                string createKartsTable = @"
                    CREATE TABLE IF NOT EXISTS Karts (
                        KartID TEXT PRIMARY KEY,        -- 车号 (AA01-AA99)
                        RFIDTag TEXT UNIQUE,            -- RFID 标签 EPC
                        Notes TEXT,                     -- 备注
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";

                // 表2: 计圈会话表 (每次开始计圈创建一个会话)
                string createSessionsTable = @"
                    CREATE TABLE IF NOT EXISTS Sessions (
                        SessionID INTEGER PRIMARY KEY AUTOINCREMENT,
                        KartID TEXT NOT NULL,           -- 车号
                        TargetLaps INTEGER DEFAULT 20,  -- 目标圈数
                        CurrentLaps INTEGER DEFAULT 0,  -- 当前圈数
                        StartTime DATETIME NOT NULL,    -- 开始时间
                        EndTime DATETIME,               -- 结束时间
                        Status TEXT DEFAULT 'Running',  -- Running/Completed
                        FOREIGN KEY (KartID) REFERENCES Karts(KartID)
                    )";

                // 表3: 通过记录表 (每次通过起点线的记录)
                string createLapsTable = @"
                    CREATE TABLE IF NOT EXISTS Laps (
                        LapID INTEGER PRIMARY KEY AUTOINCREMENT,
                        SessionID INTEGER NOT NULL,     -- 会话ID
                        KartID TEXT NOT NULL,           -- 车号
                        LapNumber INTEGER NOT NULL,     -- 圈数
                        LapTime DATETIME NOT NULL,      -- 通过时间
                        ElapsedSeconds REAL,            -- 圈时(秒)
                        FOREIGN KEY (SessionID) REFERENCES Sessions(SessionID),
                        FOREIGN KEY (KartID) REFERENCES Karts(KartID)
                    )";

                ExecuteNonQuery(connection, createKartsTable);
                ExecuteNonQuery(connection, createSessionsTable);
                ExecuteNonQuery(connection, createLapsTable);
            }
        }

        private void ExecuteNonQuery(SQLiteConnection connection, string commandText)
        {
            using (var command = new SQLiteCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(connectionString);
        }

        // ========== 卡丁车管理 ==========

        /// <summary>
        /// 添加卡丁车
        /// </summary>
        public bool AddKart(string kartID, string rfidTag, string notes = "")
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string sql = "INSERT INTO Karts (KartID, RFIDTag, Notes) VALUES (@KartID, @RFIDTag, @Notes)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@KartID", kartID);
                        cmd.Parameters.AddWithValue("@RFIDTag", rfidTag ?? "");
                        cmd.Parameters.AddWithValue("@Notes", notes);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 更新卡丁车的 RFID 标签
        /// </summary>
        public bool UpdateKartRFID(string kartID, string rfidTag)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string sql = "UPDATE Karts SET RFIDTag = @RFIDTag WHERE KartID = @KartID";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@KartID", kartID);
                        cmd.Parameters.AddWithValue("@RFIDTag", rfidTag);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 根据 RFID 标签获取卡丁车ID
        /// </summary>
        public string GetKartIDByRFID(string rfidTag)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT KartID FROM Karts WHERE RFIDTag = @RFIDTag";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RFIDTag", rfidTag);
                        object result = cmd.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// 获取所有卡丁车
        /// </summary>
        public DataTable GetAllKarts()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "SELECT KartID, RFIDTag, Notes FROM Karts ORDER BY KartID";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        // ========== 会话管理 ==========

        /// <summary>
        /// 开始新的计圈会话
        /// </summary>
        public int StartSession(string kartID, int targetLaps = 20)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Sessions (KartID, TargetLaps, StartTime, Status) 
                               VALUES (@KartID, @TargetLaps, datetime('now', 'localtime'), 'Running')";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@KartID", kartID);
                    cmd.Parameters.AddWithValue("@TargetLaps", targetLaps);
                    cmd.ExecuteNonQuery();
                }
                return (int)conn.LastInsertRowId;
            }
        }

        /// <summary>
        /// 记录一圈
        /// </summary>
        public void RecordLap(int sessionID, string kartID, int lapNumber, DateTime lapTime, double elapsedSeconds)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                
                // 插入圈记录
                string sqlLap = @"INSERT INTO Laps (SessionID, KartID, LapNumber, LapTime, ElapsedSeconds) 
                                  VALUES (@SessionID, @KartID, @LapNumber, @LapTime, @ElapsedSeconds)";
                using (var cmd = new SQLiteCommand(sqlLap, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionID", sessionID);
                    cmd.Parameters.AddWithValue("@KartID", kartID);
                    cmd.Parameters.AddWithValue("@LapNumber", lapNumber);
                    cmd.Parameters.AddWithValue("@LapTime", lapTime);
                    cmd.Parameters.AddWithValue("@ElapsedSeconds", elapsedSeconds);
                    cmd.ExecuteNonQuery();
                }

                // 更新会话的当前圈数
                string sqlUpdate = "UPDATE Sessions SET CurrentLaps = @LapNumber WHERE SessionID = @SessionID";
                using (var cmd = new SQLiteCommand(sqlUpdate, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionID", sessionID);
                    cmd.Parameters.AddWithValue("@LapNumber", lapNumber);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 结束会话
        /// </summary>
        public void EndSession(int sessionID)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Sessions SET Status = 'Completed', EndTime = datetime('now', 'localtime') WHERE SessionID = @SessionID";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionID", sessionID);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 获取正在运行的会话
        /// </summary>
        public DataTable GetRunningSessions()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = @"SELECT SessionID, KartID, TargetLaps, CurrentLaps, StartTime 
                               FROM Sessions WHERE Status = 'Running' ORDER BY StartTime DESC";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// 获取今天的所有通过记录
        /// </summary>
        public DataTable GetTodayLaps()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                string sql = @"SELECT KartID, LapNumber, LapTime, ElapsedSeconds 
                               FROM Laps 
                               WHERE DATE(LapTime) = DATE('now', 'localtime') 
                               ORDER BY LapTime DESC";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// 获取卡丁车的活动会话ID
        /// </summary>
        public int? GetActiveSessionID(string kartID)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT SessionID FROM Sessions WHERE KartID = @KartID AND Status = 'Running' ORDER BY StartTime DESC LIMIT 1";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                    cmd.Parameters.AddWithValue("@KartID", kartID);
                        object result = cmd.ExecuteScalar();
                        return result != null ? (int?)(long)result : null;
                    }
                }
            }
            catch { return null; }
        }
    }
}
