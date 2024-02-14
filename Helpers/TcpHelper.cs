using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemantiCore.Helpers
{
    public static class TcpHelper
    {

        public static void WriteLine(string message)
        {
            StreamWriter sw = new StreamWriter(App.TcpStream);
            sw.WriteLine(message);
            sw.Flush();

        }

        public static string ReadLine()
        {
            StreamReader sr = new StreamReader(App.TcpStream);
            return sr.ReadLine();
        }
    }
}
