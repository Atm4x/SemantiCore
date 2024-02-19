using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemantiCore.Models
{
    public class IndexingModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public List<List<double>> Vectors { get; set; }

        public IndexingModel()
        {
            Id = 0;
            FileName = "";
            Vectors = new List<List<double>>();
        }

        public IndexingModel(int id, string fileName, List<List<double>> vectors)
        {
            Id = id;
            FileName = fileName;
            Vectors = vectors;
        }
    }

    public class IndexingDirectory
    {
        public int Id { get; set; }

        public List<IndexingModel> Models { get; set; }

        public string Path { get; set; }
        public string FilePath { get; set; }
        public IndexingDirectory()
        {
            
        }

        public IndexingDirectory(string path, int id)
        {
            Models = new List<IndexingModel>();
            Path = path;
            Id = id;    
        }

        public IndexingDirectory(List<IndexingModel> models, string path, int id)
        {
            Models = models;
            Path = path;
            Id = id;
        }

        private bool _saved = true;

        public void Save()
        {
            if (_saved)
                return;

            if (string.IsNullOrWhiteSpace(FilePath))
            {
                var path = App.MainConfig.IndexingPath;

                if (string.IsNullOrWhiteSpace(path))
                    return;

                var name = AbbreviatePath(Path.ToString());

                var fullPath = System.IO.Path.Combine(path, $"{name}.json");

                FilePath = fullPath;
            }

            File.WriteAllText(FilePath, JsonConvert.SerializeObject(this));

            _saved = true;
        }

        static string AbbreviatePath(string filePath)
        {
            string[] parts = filePath.Split('\\');

            string drive = parts[0];

            int directoryCount = parts.Length - 1;

            string abbreviatedPath = $"{drive.TrimEnd(':')}{'.'.ToString().PadLeft(directoryCount, '.')}{parts[parts.Length-1]}";

            return abbreviatedPath;
        }

        public void AddModel(IndexingModel model)
        {
            Models.Add(model);
            _saved = false;
        }

        public void RemoveModel(IndexingModel model)
        {
            Models.Remove(model);
            _saved = false;
        }

        public void Clear()
        {
            Models = new List<IndexingModel>();
            _saved = false;
        }
    }

    public class Query
    {
        public string Type { get; set; }
        public object Value { get; set; }

        public Query(QueryTypes type, object value)
        {
            Type = type.ToString();
            Value = value;
        }
    }

    public class CompareQuery
    {
        public List<double> Text_Vector { get; set; }
        public List<double> Vector { get; set; }

        public CompareQuery(List<double> text, List<double> vector)
        {
            Text_Vector = text;
            Vector = vector;
        }
    }

    public enum QueryTypes
    {
        IndexingFile = 0,
        IndexingText = 1,
        Compare = 2,
        Close = 3
    }
}
