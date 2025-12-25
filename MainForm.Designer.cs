namespace GoKartLapCounter
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblStatus = new System.Windows.Forms.Label();
            this.btConnect = new System.Windows.Forms.Button();
            this.btResetSession = new System.Windows.Forms.Button();
            this.gbStartLap = new System.Windows.Forms.GroupBox();
            this.btStartLapCounting = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.nudTargetLaps = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.cbKartID = new System.Windows.Forms.ComboBox();
            this.lvRunning = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.gbStartLap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTargetLaps)).BeginInit();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Location = new System.Drawing.Point(1530, 18);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(115, 17);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "○ Not Connected";
            // 
            // btConnect
            // 
            this.btConnect.Location = new System.Drawing.Point(1430, 12);
            this.btConnect.Name = "btConnect";
            this.btConnect.Size = new System.Drawing.Size(75, 28);
            this.btConnect.TabIndex = 1;
            this.btConnect.Text = "Connect";
            this.btConnect.UseVisualStyleBackColor = true;
            this.btConnect.Click += new System.EventHandler(this.btConnect_Click);
            // 
            // btResetSession
            // 
            this.btResetSession = new System.Windows.Forms.Button();
            this.btResetSession.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btResetSession.Location = new System.Drawing.Point(1430, 60);
            this.btResetSession.Name = "btResetSession";
            this.btResetSession.Size = new System.Drawing.Size(160, 45);
            this.btResetSession.TabIndex = 5;
            this.btResetSession.Text = "Reset Selection";
            this.btResetSession.UseVisualStyleBackColor = true;
            this.btResetSession.Click += new System.EventHandler(this.btResetSession_Click);
            // 
            // gbStartLap
            // 
            this.gbStartLap.Controls.Add(this.btStartLapCounting);
            this.gbStartLap.Controls.Add(this.label2);
            this.gbStartLap.Controls.Add(this.nudTargetLaps);
            this.gbStartLap.Controls.Add(this.label1);
            this.gbStartLap.Controls.Add(this.cbKartID);
            this.gbStartLap.Visible = false;
            this.gbStartLap.Location = new System.Drawing.Point(0, 0);
            this.gbStartLap.Size = new System.Drawing.Size(0, 0);
            // 
            // cbKartID
            // 
            this.cbKartID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbKartID.FormattingEnabled = true;
            this.cbKartID.Location = new System.Drawing.Point(90, 30);
            this.cbKartID.Name = "cbKartID";
            this.cbKartID.Size = new System.Drawing.Size(121, 21);
            this.cbKartID.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Kart ID:";
            // 
            // nudTargetLaps
            // 
            this.nudTargetLaps.Location = new System.Drawing.Point(320, 30);
            this.nudTargetLaps.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.nudTargetLaps.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudTargetLaps.Name = "nudTargetLaps";
            this.nudTargetLaps.Size = new System.Drawing.Size(80, 20);
            this.nudTargetLaps.TabIndex = 2;
            this.nudTargetLaps.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(240, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Target Laps:";
            // 
            // btStartLapCounting
            // 
            this.btStartLapCounting.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.btStartLapCounting.Location = new System.Drawing.Point(450, 22);
            this.btStartLapCounting.Name = "btStartLapCounting";
            this.btStartLapCounting.Size = new System.Drawing.Size(120, 35);
            this.btStartLapCounting.TabIndex = 4;
            this.btStartLapCounting.Text = "Start";
            this.btStartLapCounting.UseVisualStyleBackColor = true;
            this.btStartLapCounting.Click += new System.EventHandler(this.btStartLapCounting_Click);
            // 
            // lvRunning
            // 
            this.lvRunning.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.lvRunning.FullRowSelect = true;
            this.lvRunning.GridLines = true;
            this.lvRunning.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F);
            this.lvRunning.Location = new System.Drawing.Point(12, 12);
            this.lvRunning.Name = "lvRunning";
            this.lvRunning.Size = new System.Drawing.Size(1400, 1056);
            this.lvRunning.TabIndex = 3;
            this.lvRunning.UseCompatibleStateImageBehavior = false;
            this.lvRunning.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Kart ID";
            this.columnHeader1.Width = 350;
            this.columnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Current Lap";
            this.columnHeader2.Width = 350;
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Target Laps";
            this.columnHeader3.Width = 350;
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Start Time";
            this.columnHeader4.Width = 350;
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 12F);
            this.txtLog.ForeColor = System.Drawing.Color.Lime;
            this.txtLog.Location = new System.Drawing.Point(1430, 150);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(460, 918);
            this.txtLog.TabIndex = 4;
            this.txtLog.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Controls.Add(this.btResetSession);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lvRunning);
            this.Controls.Add(this.gbStartLap);
            this.Controls.Add(this.btConnect);
            this.Controls.Add(this.lblStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GoKart Lap Counter v1.0";
            this.gbStartLap.ResumeLayout(false);
            this.gbStartLap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTargetLaps)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btConnect;
        private System.Windows.Forms.GroupBox gbStartLap;
        private System.Windows.Forms.Button btStartLapCounting;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudTargetLaps;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbKartID;
        private System.Windows.Forms.ListView lvRunning;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Button btResetSession;
    }
}