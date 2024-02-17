using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MessageBox = System.Windows.MessageBox;

namespace SemantiCore.Windows
{
    /// <summary>
    /// Логика взаимодействия для ManageWindow.xaml
    /// </summary>
    public partial class ManageWindow : GlobalWindow
    {
        public string[] extensions = { ".docx", ".rtf", ".doc", ".docm", ".dotx", ".dot", ".dotm", ".pdf", ".txt" };

        public ObservableCollection<DirectoryView> ViewedDirectories
            = new ObservableCollection<DirectoryView>();


        public ManageWindow()
        {
            InitializeComponent();
            DirectoryList.ItemsSource = ViewedDirectories;
        }

        private void ChooseFolder(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                var result = folderBrowser.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                {
                    List<string> files = GetInsideFiles(new List<string>(), folderBrowser.SelectedPath);


                    files = files.Where(file => extensions.Contains(System.IO.Path.GetExtension(file))).ToList();

                    var existedDirectory = App.IndexedDirectories.FirstOrDefault(directory => directory.Path.StartsWith(folderBrowser.SelectedPath));

                    if (existedDirectory != null)
                    {   
                        var resultOfQuiz = MessageBox.Show("Эта папка содержи индексированную папку. Соединить файлы?", "Внимание", MessageBoxButton.OKCancel);
                        if(resultOfQuiz == MessageBoxResult.OK)
                        {
                            existedDirectory.Path = folderBrowser.SelectedPath;
                            StartIndexing();
                            UpdateViewed();
                            return;
                        } 
                        else
                        {
                            return;
                        }
                    } 
                    else
                    {
                        bool starts = false;
                        foreach (var directory in App.IndexedDirectories)
                        {
                            if(folderBrowser.SelectedPath.StartsWith(directory.Path))
                            {
                                starts = true;
                                break;
                            }
                        }
                        if(starts)
                        {
                            MessageBox.Show("Эта папка уже содержится в индексированном каталоге.");
                            return;
                        }
                    }
                    var indexed = new IndexingDirectory(folderBrowser.SelectedPath, 1);
                    App.IndexedDirectories.Add(indexed);
                    System.Windows.Forms.MessageBox.Show("Files found: " + files.Count.ToString(), "Message");
                    StartIndexing();
                    UpdateViewed();
                }
            }
        }

        public void UpdateViewed()
        {
            ViewedDirectories.Clear();
            foreach (var directory in App.IndexedDirectories)
            {
                ViewedDirectories.Add(new DirectoryView(
                           directory.Id, directory.Path.Replace('\\', '/').Split('/').Last(), directory.Path));
            }
        }

        private List<string> GetInsideFiles(List<string> addFiles, string folder)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    return addFiles;
                }

                string[] files = Directory.GetFiles(folder);

                string[] directories = Directory.GetDirectories(folder);

                addFiles.AddRange(files);

                foreach (var dir in directories)
                {
                    GetInsideFiles(addFiles, dir);
                }

                return addFiles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            return Directory.GetFiles(folder).ToList();
        }

        private ProgressWindow _progressWindow;

        private void StartIndexing()
        {
            List<FileInfo> allFiles = new List<FileInfo>();

            foreach (var directory in App.IndexedDirectories)
            {
                List<string> files = GetInsideFiles(new List<string>(), directory.Path);
                var infos = files.Where(file => extensions.Contains(System.IO.Path.GetExtension(file)))
                    .Where(file => !directory.Models.Any(model => model.FileName.Equals(file)))
                    .Select(x => new FileInfo(x)).OrderBy(file => file.Length)
                    .ToList();

                allFiles.AddRange(infos);
            }
            _progressWindow = new ProgressWindow();
            _progressWindow.SetMax(allFiles.Count);
            _progressWindow.Show();
            Thread thread = new Thread(() => Indexing(allFiles.Select(x => x.FullName)));
            thread.Start();
        }

        public void Indexing(IEnumerable<string> files)
        {
            int i = 0;
            try
            {
                foreach (var file in files)
                {
                    i++;
                    Query query = new Query(QueryTypes.IndexingFile, $"{file}");
                    TcpHelper.WriteLine(JsonConvert.SerializeObject(query));
                    var answer = TcpHelper.ReadLine();
                    var vectors = JsonConvert.DeserializeObject<List<List<double>>>(answer);
                    if (vectors == null)
                    {
                        MessageBox.Show("Ошибка при получении: " + answer);
                        Dispatcher.Invoke(() => _progressWindow.SetValue(i, file));
                        continue;
                    }
                    IndexingModel model = new IndexingModel(i, file, vectors);

                    App.IndexedDirectories.First(directory => file.StartsWith(directory.Path))
                        .AddModel(model);
                    Dispatcher.Invoke(() => _progressWindow.SetValue(i, file));
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Ошибка(");
            }
        }

        private void CompareClicked(object sender, RoutedEventArgs e)
        {
            Query indexingText = new Query(QueryTypes.IndexingText, Compare_text.Text);
            TcpHelper.WriteLine(JsonConvert.SerializeObject(indexingText));
            var json_vector = TcpHelper.ReadLine();
            var text_vector = JsonConvert.DeserializeObject<List<double>>(json_vector);

            List<Similarity> similarities = new List<Similarity>();

            
            foreach(var indexedDirectory in App.IndexedDirectories)
            foreach (var model in indexedDirectory.Models)
            {
                List<double> list = new List<double>();

                foreach (var vector in model.Vectors)
                {
                    //CompareQuery compareQuery = new CompareQuery(text_vector, vector);
                    //Query query = new Query(QueryTypes.Compare, compareQuery);

                    var answer = GetCosineSimilarity(text_vector, vector);
                    //TcpHelper.WriteLine(JsonConvert.SerializeObject(query));
                    //var answer = TcpHelper.ReadLine();

                    try
                    {
                        list.Add(answer);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception: " + ex.Message);
                        return;
                    }
                }
                similarities.Add(new Similarity(model.Id, model.FileName, list.Count>0 ? list.Max() : 0));
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
            $"Документ {System.IO.Path.GetFileName(x.Path)}: {x.Value}")));
        }

        public static double GetCosineSimilarity(List<double> V1, List<double> V2)
        {
            int N = 0;
            N = ((V2.Count < V1.Count) ? V2.Count : V1.Count);
            double dot = 0.0d;
            double mag1 = 0.0d;
            double mag2 = 0.0d;
            for (int n = 0; n < N; n++)
            {
                dot += V1[n] * V2[n];
                mag1 += Math.Pow(V1[n], 2);
                mag2 += Math.Pow(V2[n], 2);
            }

            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }

        public double Median(List<double> similarities)
        {
            double mid = (similarities.Count - 1) / 2.0;
            return (similarities[(int)(mid)] + similarities[(int)(mid + 0.5)]) / 2;
        }

        public class DirectoryView
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
            public DirectoryView(int id, string name, string path)
            {
                Id = id;
                Name = name;
                Path = path;
            }
        }

        public class Similarity
        {
            public int Id { get; set; }
            public string Path { get; set; }
            public double Value { get; set; }

            public Similarity(int id, string path, double value)
            {
                Id = id;
                Path = path;
                Value = value;
            }
        }
    }
}
