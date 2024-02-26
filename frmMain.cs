using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using YunYan;

namespace DBMonitorFileTracker
{
    public partial class frmMain : Form
    {
        private string _filePath = "FilePath.set";
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFilePath.Text))
            {
                SaveFilePath();
            }
            else
            {
                MessageBox.Show("請輸入輸出檔案位址");
                return;
            }

            if (btnStart.Text == "啟動")
            {
                ButtonState(true);
            }
            else if (btnStart.Text == "運作中")
            {
                ButtonState(false);
            }
        }

        private void SaveFilePath()
        {
            try
            {
                Program.CreateOrUpdateConfigFile(_filePath, txtFilePath.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Log.LogMsg(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void tmrMain_Tick(object sender, EventArgs e)
        {
            CheckDB();

            //5分鐘檢查一次有沒有產生檔案
            if (DateTime.Now.Minute % 5 == 0)
                CheckFile();
        }

        private void CheckDB()
        {
            try
            {
                using (MySQL db = new MySQL())
                {
                    var time = DateTime.Now.AddMinutes(-1);
                    var dateString = time.ToString("yyyy-MM-dd");
                    var hourString = time.Hour;
                    var minuteString = time.Minute;

                    var query = $"SELECT * FROM sensor_data WHERE DATE(sd_time) = '{dateString}' AND HOUR(sd_time) = {hourString} AND MINUTE(sd_time) = {minuteString}";
                    var rows = db.SelectTable(query).Rows;

                    if (rows.Count == 0)
                    {
                        var msg = $"{time} 未記錄到資料庫";
                        Program.LineNotify(msg);
                        Log.LogMsg(msg);
                    }
                }

                if (!string.IsNullOrEmpty(txtURL.Text))
                {
                    using (MySQL sql = new MySQL())
                    {
                        var query = "SELECT sd_time FROM sensor_data ORDER BY sd_id DESC LIMIT 1";
                        var dt = sql.SelectTable(query);
                        if (dt != null)
                        {
                            DateTime dateTime = Convert.ToDateTime(dt.Rows[0]["sd_time"]);
                            Program.IMAliveNotify(dateTime, txtURL.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogMsg(ex.Message + "\n" + ex.StackTrace);
                MessageBox.Show(ex.Message);
                ButtonState(false);
            }
        }

        private void CheckFile()
        {
            var time = DateTime.Now.AddMinutes(-5).ToString("MMddHHmm");
            var fileName = "E1" + time + ".H76";

            try
            {
                if (!File.Exists(Path.Combine(txtFilePath.Text, fileName)))
                {
                    var msg = time + " 未產生檔案";
                    Program.LineNotify(msg);
                    Log.LogMsg(msg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Log.LogMsg(ex.Message + "\n" + ex.StackTrace);
                tmrMain.Enabled = false;
                ButtonState(false);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                var savePaths = Program.ReadConfigFile(_filePath);
                if (savePaths != null)
                    txtFilePath.Text = savePaths[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Log.LogMsg(ex.Message + "\n" + ex.StackTrace);
            }

        }

        private void ButtonState(bool isOpen)
        {
            if (isOpen)
            {
                btnStart.BackColor = Color.LightGreen;
                btnStart.Text = "運作中";
                tmrMain.Enabled = true;
            }
            else
            {
                btnStart.BackColor = Color.LightYellow;
                btnStart.Text = "啟動";
                tmrMain.Enabled = false;
            }
        }
    }
}
