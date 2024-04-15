using Newtonsoft.Json;
using SemantiCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SemantiCore.Windows.ManageWindow;

namespace SemantiCore.Helpers
{
    public static class IndexingHelper
    {
        public static List<Similarity> GetSimilarities(string text)
        {
            Query indexingText = new Query(QueryTypes.IndexingText, text);
            TcpHelper.WriteLine(JsonConvert.SerializeObject(indexingText));
            var json_vector = TcpHelper.ReadLine();
            var text_vector = JsonConvert.DeserializeObject<List<double>>(json_vector);

            List<Similarity> similarities = new List<Similarity>();


            foreach (var indexedDirectory in App.IndexedDirectories)
                foreach (var model in indexedDirectory.Models)
                {
                    List<double> list = new List<double>();

                    foreach (var vector in model.Vectors)
                    {
                        var answer = MathHelper.GetCosineSimilarity(text_vector, vector);

                        try
                        {
                            list.Add(answer);
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                    similarities.Add(new Similarity(model.Id, model.FileName, list.Count > 0 ? list.Max() : 0));
                }
            return similarities;
        }

        public static List<FileView> GetFileViews(string text)
        {
            List<Similarity> similarities = GetSimilarities(text);


            var fullFiles = similarities.Select(x => new FileView()
            {
                Id = x.Id,
                Path = x.Path.Substring(System.IO.Path.GetDirectoryName(
                    App.IndexedDirectories.First(
                    directory => directory.Models.FirstOrDefault(
                        model => model.FileName == x.Path) != null).Path).Length),
                Name = System.IO.Path.GetFileName(x.Path),
                FullPath = x.Path,
                Percentage = x.Value * 100
            });
            return fullFiles.ToList();
        }
    }
}
