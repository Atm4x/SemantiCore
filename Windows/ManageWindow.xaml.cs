using Newtonsoft.Json;
using SemantiCore.Helpers;
using SemantiCore.Interfaces;
using SemantiCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SemantiCore.Windows
{
    /// <summary>
    /// Логика взаимодействия для ManageWindow.xaml
    /// </summary>
    public partial class ManageWindow : GlobalWindow
    {
        public ManageWindow()
        {
            InitializeComponent();
        }

        private void ChooseFolder(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                var result = folderBrowser.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                {
                    List<string> files = Directory.GetFiles(folderBrowser.SelectedPath).ToList();
                    files = files.Where(file => System.IO.Path.GetExtension(file).EndsWith("txt")).ToList();

                    SelectedDirectory = folderBrowser.SelectedPath;
                    System.Windows.Forms.MessageBox.Show("Files found: " + files.Count.ToString(), "Message");
                }
            }
        }
        public string SelectedDirectory = "";

        private void StartIndexing(object sender, RoutedEventArgs e)
        {
            List<string> files = Directory.GetFiles(SelectedDirectory).ToList();
            files = files.Where(file => System.IO.Path.GetExtension(file).EndsWith("txt")).ToList();

            Query query = new Query(QueryTypes.IndexingFile, files[0]);

            TcpHelper.WriteLine(JsonConvert.SerializeObject(query));
            var answer = TcpHelper.ReadLine();
        }
    }
}
