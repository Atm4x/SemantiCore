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
        public string VectorJson { get; set; }

        public IndexingModel()
        {
            Id = 0;
            FileName = "";
            VectorJson = "";
        }

        public IndexingModel(int id, string fileName, object vector)
        {
            Id = id;
            FileName = fileName;
            VectorJson = JsonConvert.SerializeObject(vector);
        }
    }

    public class IndexingDirectory
    {
        public List<IndexingModel> Models { get; set; }

        public IndexingDirectory()
        {
            Models = new List<IndexingModel>();
        }

        public IndexingDirectory(List<IndexingModel> models)
        {
            Models = models;
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

    public enum QueryTypes
    {
        IndexingFile = 0,

    }
}
