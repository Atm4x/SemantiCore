using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public void AddModel(IndexingModel model)
        {
            Models.Add(model);
        }

        public void RemoveModel(IndexingModel model)
        {
            Models.Remove(model);
        }

        public void Clear()
        {
            Models = new List<IndexingModel>();
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
    }
}
