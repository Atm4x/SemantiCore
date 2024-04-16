using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemantiCore.Models
{
    public class RequestModel
    {
        public RequestType RequestType { get; set; }
        public object Body { get; set; }
    }

    public class SearchBody
    {
        public string SearchText { get; set; }
        public int MaxCount { get; set; } = 10;
    }

    public enum RequestType
    {
        Similarities = 1
    }
}
