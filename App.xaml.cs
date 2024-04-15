using GlobalHotKey;
using Newtonsoft.Json;
using SemantiCore.Helpers;
using SemantiCore.Models;
using SemantiCore.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace SemantiCore
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<IndexingDirectory> IndexedDirectories;
        public HotKeyManager GlobalKeys;
        public ManageWindow ManageWindow;
        public TcpClient Client;
        public static string ConfigPath;
        public static Stream TcpStream;
        public static Config MainConfig;
        public static NotifyIcon Notifications;
        public static HostingHelper Hosting;


        protected override void OnStartup(StartupEventArgs e)
        {
            Notifications = new NotifyIcon();
            Notifications.Icon = new Icon(Path.Combine(Environment.CurrentDirectory, "SemantiCoreIcon.ico"));
            Notifications.Visible = true;
            ConfigPath = Path.Combine(Environment.CurrentDirectory, "config.json");
            MainConfig = Config.ReadConfig();
            Task.Run(() => WaitingForError());
            IndexedDirectories = new List<IndexingDirectory>();
            GlobalKeys = new HotKeyManager();
            GlobalKeys.Register(Key.F, ModifierKeys.Shift | ModifierKeys.Windows);
            Configure();
            ReadAllIndexed();
            base.OnStartup(e);
            if (App.MainConfig.Hosting)
            {
                Hosting = new HostingHelper();
                Hosting.Start();
            }
            else
            {
                ManageWindow = new ManageWindow();
                ManageWindow.Show();
            }
        }

        public static bool ReadAllIndexed()
        {
            if (string.IsNullOrWhiteSpace(MainConfig.IndexingPath))
                return false;

            if (!Directory.Exists(MainConfig.IndexingPath))
                return false;

            foreach(var indexing in IndexedDirectories.ToList())
            {
                if(!string.IsNullOrWhiteSpace(indexing.FilePath))
                    IndexedDirectories.Remove(indexing);
            }

            var files = Directory.GetFiles(MainConfig.IndexingPath);
            foreach (var file in files)
            {
                try
                {
                    IndexedDirectories.Add(JsonConvert.DeserializeObject<IndexingDirectory>(File.ReadAllText(file)));
                } catch
                {
                    MessageBox.Show($"Ошибка при чтении файла {System.IO.Path.GetFileName(file)}");
                    return false;
                }
            }
            return true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Query query = new Query(QueryTypes.Close, null);
            TcpHelper.WriteLine(JsonConvert.SerializeObject(query));
            base.OnExit(e);
        }

        public Task WaitingForError()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"cmd.exe";
            start.Arguments = "python --version";
            start.UseShellExecute = false;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardError)
                {
                    string result = reader.ReadToEnd();
                    MessageBox.Show("Python3 не установлен.");
                    Environment.Exit(-1);
                }
            }
            return Task.FromResult(0);
        }

        public void Kill()
        {
            try
            {
                ServerProcess.Kill();
            } catch
            {

            }
        }

        private void Exiting(object sender, ExitEventArgs e)
        {

            Kill();
        }


        public Process ServerProcess { get; private set; }

        public void Configure()
        {
            var modelPath = Environment.CurrentDirectory;
            bool isDirectoryExists = Directory.GetDirectories(modelPath).Any(file => file.ToLower().EndsWith("LaBSE".ToLower()));

            var path = Path.Combine(Environment.CurrentDirectory, "Resources\\main.py");

            var installPath = Path.Combine(Environment.CurrentDirectory, "Resources\\install.py");

            if (!isDirectoryExists)
            {

                var process = Process.Start(new ProcessStartInfo()
                {
                    FileName = @"python3",
                    CreateNoWindow = true,
                    Arguments = $"\"{installPath}\" \"{modelPath}\"",
                    UseShellExecute = false
                });

                Notifications.BalloonTipIcon = ToolTipIcon.Info;
                Notifications.BalloonTipTitle = "Начало установки модели LaBSE";
                Notifications.BalloonTipText = "Идёт процесс установки весов модели LaBSE.";
                Notifications.ShowBalloonTip(3000);
                process.WaitForExit();


                Notifications.BalloonTipIcon = ToolTipIcon.Info;
                Notifications.BalloonTipTitle = "Установка модели LaBSE завершена";
                Notifications.BalloonTipText = "Процесс установки завершён, модель загружается.";
                Notifications.ShowBalloonTip(3000);
            } else
            {
                Notifications.BalloonTipIcon = ToolTipIcon.Info;
                Notifications.BalloonTipTitle = "Запуск сервера";
                Notifications.BalloonTipText = "Дождитесь загрузки модели для использования семантического поиска.";
                Notifications.ShowBalloonTip(3000);
            }

            
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"python3";
            start.Arguments = $"\"{path}\" \"{modelPath}\"";
            start.UseShellExecute = false;
            start.CreateNoWindow = !MainConfig.Debug;
            ServerProcess = Process.Start(start);

            while (IsAvailableTcp(19200))
                Thread.Sleep(1000);

            Client = new TcpClient();
            try
            {
                Client.Connect("127.0.0.1", 19200);
                TcpStream = Client.GetStream();
                StreamReader sr = new StreamReader(TcpStream);

                var connection = sr.ReadLine();
                if (!connection.Equals("Loaded"))
                {
                    MessageBox.Show($"Connection lost: {connection}.");
                }
            }
            catch
            {
                Kill();
            }
            GlobalKeys.KeyPressed += KeyPressedGlobally;
        }

        private bool IsAvailableTcp(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            return !tcpConnInfoArray.Any(x => x.Port == port);
        }

        private void KeyPressedGlobally(object sender, KeyPressedEventArgs e)
        {

            if (e.HotKey.Key == Key.F)
            {
                bool opened = ManageWindow.IsShowing;
                if (!opened)
                    ManageWindow.Show();
                else
                    ManageWindow.Hide();
            }
        }
    }
}
