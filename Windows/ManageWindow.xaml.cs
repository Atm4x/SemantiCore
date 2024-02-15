using Newtonsoft.Json;
using SemantiCore.Helpers;
using SemantiCore.Interfaces;
using SemantiCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using MessageBox = System.Windows.MessageBox;

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

            App.IndexedDirectory.Clear();
            int i = 0;
            foreach (var file in files)
            {
                i++;
                Query query = new Query(QueryTypes.IndexingFile, file);
                TcpHelper.WriteLine(JsonConvert.SerializeObject(query));
                var answer = TcpHelper.ReadLine();
                
                IndexingModel model = new IndexingModel(i, file, JsonConvert.DeserializeObject<List<double>>(answer));
                
                App.IndexedDirectory.AddModel(model);
            }
        }

        private void CompareClicked(object sender, RoutedEventArgs e)
        {
            Query indexingText = new Query(QueryTypes.IndexingText, Compare_text.Text);
            TcpHelper.WriteLine(JsonConvert.SerializeObject(indexingText));
            var json_vector = TcpHelper.ReadLine();
            var vector = JsonConvert.DeserializeObject<List<double>>(json_vector);

            List<Similarity> similarities = new List<Similarity>();

            
            foreach (var model in App.IndexedDirectory.Models)
            {
                CompareQuery compareQuery = new CompareQuery(vector, model.Vector);
                Query query = new Query(QueryTypes.Compare, compareQuery);

                TcpHelper.WriteLine(JsonConvert.SerializeObject(query));
                var answer = TcpHelper.ReadLine();

                try
                {
                    var value = Convert.ToDouble(answer.Replace('.', ','));
                    similarities.Add(new Similarity(model.Id, value));
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);
                    return;
                }
            }
            //similarities = similarities.OrderBy(x => x.Value).ToList();
            //
            //var median = Median(similarities.Select(x => x.Value).ToList());
            //
            //var changed = similarities.Count>5 ? similarities.Where(x => x.Value >= median).ToList() : similarities;
            //
            //var bestMatches = changed
            //    .Skip(Math.Max(0, similarities.Count - 5)).ToList();
            //
            MessageBox.Show(string.Join("\n", similarities.OrderByDescending(x => x.Value).Select(x => 
            $"Документ {System.IO.Path.GetFileName(App.IndexedDirectory.Models.First(y=> y.Id == x.Id).FileName)}: {x.Value}")));
        }

        public double Median(List<double> similarities)
        {
            double mid = (similarities.Count - 1) / 2.0;
            return (similarities[(int)(mid)] + similarities[(int)(mid + 0.5)]) / 2;
        }

        public class Similarity
        {
            public int Id { get; set; }
            public double Value { get; set; }

            public Similarity(int id, double value)
            {
                Id = id;
                Value = value;
            }
        }
    }
}
