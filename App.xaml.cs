using GlobalHotKey;
using SemantiCore.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SemantiCore
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public HotKeyManager GlobalKeys;
        public ManageWindow ManageWindow;
        public TcpClient Client;
        public static Stream TcpStream;



        protected override void OnStartup(StartupEventArgs e)
        {
            Task.Run(() => WaitingForError());

            GlobalKeys = new HotKeyManager();
            GlobalKeys.Register(Key.F, ModifierKeys.Shift | ModifierKeys.Windows);
            Configure();
            base.OnStartup(e);
            ManageWindow = new ManageWindow();
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
            var path = Path.Combine(Environment.CurrentDirectory, "Resources\\main.py");
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"python3";
            start.Arguments = $"\"{path}\"";
            start.UseShellExecute = false;
            start.CreateNoWindow = false;
            ServerProcess = Process.Start(start);

            Thread.Sleep(10000);

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
                else
                {
                    MessageBox.Show("Connected");
                }
            }
            catch
            {
                Kill();
            }
            GlobalKeys.KeyPressed += KeyPressedGlobally;
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
