using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBMonitorFileTracker
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        public static void LineNotify(string msg)
        {
            var apiURL = ConfigurationManager.AppSettings["LinNotifyURL"];
            var uuids = ConfigurationManager.AppSettings["uuid"];
            var uuidParts = uuids.Split(',');

            for (int i = 0; i < uuidParts.Length; i++)
            {
                var uuid = uuidParts[i];
                var getdata = "uuid=" + uuid + "&mydata=";

                //發送文字編碼
                var webBrowser = new WebBrowser();
                var content =  msg;
                webBrowser.Navigate(apiURL + getdata + System.Net.WebUtility.UrlEncode(content));
            }
        }

        /// <summary>
        /// 建立設定檔
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void CreateOrUpdateConfigFile(string fileName, string content)
        {
            try
            {
                var filePath = Path.Combine(Application.StartupPath, fileName);
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static string[] ReadConfigFile(string fileName)
        {
            try
            {
                var filePath = Path.Combine(Application.StartupPath, fileName);
                return File.ReadAllLines(filePath);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public static class Log
    {
        public static void LogMsg(string message)
        {
            try
            {
                // 獲取 Log 資料夾的路徑
                string logDirectory = Path.Combine(Application.StartupPath, "Log");

                // 檢查 Log 資料夾是否存在，如果不存在則創建
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                // 設置日誌文件的完整路徑
                string filePath = Path.Combine(logDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");

                // 創建日誌信息
                string msg = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";

                // 寫入日誌文件
                File.AppendAllText(filePath, msg + Environment.NewLine);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Log Error");
            }
        }

    }
}
