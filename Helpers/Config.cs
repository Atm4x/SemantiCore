using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SemantiCore.Helpers
{
    public class Config
    {
        public string IndexingPath { get; set; }
        public bool AutoDetection { get; set; }

        public bool Debug { get; set; } = false;
        public bool Hosting { get; set; } = false;
        public string[] Addresses { get; set; } = { "http://localhost:8080/", };

        public static void WriteConfig(Config config)
        {
            var path = App.ConfigPath;

            if (!File.Exists(path))
                File.Create(path).Close();

            File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public static Config ReadConfig()
        {
            var path = App.ConfigPath;

            if (!File.Exists(path))
            {
                File.Create(path).Close();
                var config = new Config();
                WriteConfig(config);
                return config;
            }
            var read = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(read))
                return new Config();

            var parsed = JsonConvert.DeserializeObject<Config>(read);
            if (parsed == null)
            {
                MessageBox.Show("Ошибка конфигурации");
                return new Config();
            }

            return parsed;
        }
    }
}
