using Newtonsoft.Json;
using SemantiCore.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using SemantiCore.Models;
using static SemantiCore.Windows.ManageWindow;

namespace SemantiCore.Helpers
{
    public class HostingHelper
    {
        public HttpListener Listener;
        private TerminalWindow Terminal;

        public HostingHelper()
        {
            Terminal = new TerminalWindow();
            Terminal.Show();
        }

        

        public void Start()
        {
            if(Listener != null && Listener.IsListening)
            {
                Terminal.WriteLine("Прослушивание уже идёт...");
                return;
            }

            _cancel = false;

            Listener = new HttpListener();
            foreach (var address in App.MainConfig.Addresses)
            {
                var http = $"{address}";
                Listener.Prefixes.Add(http);
                Terminal.WriteLine($"Адрес прослушивания: {http}");
            }

            Listener.Start();
            Terminal.WriteLine("Запуск прослушивания...");
            Task.Run(async () => await StartListening());
        }

        public async Task SendTest(string text)
        {
            if(Listener == null)
            {
                App.Current.Dispatcher.Invoke(() =>
                Terminal.WriteLine("Включите сервер перед тем, как искать..."));
                return;
            }
            else if(!Listener.IsListening)
            {
                App.Current.Dispatcher.Invoke(() =>
                Terminal.WriteLine("Включите сервер перед тем, как искать..."));
                return;
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestObject = new RequestModel
            {
                RequestType = RequestType.Similarities,
                Body = new SearchBody() { SearchText = text, MaxCount = 5 }
            };

            var requestString = JsonConvert.SerializeObject(requestObject);
            var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:8080/", requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                App.Current.Dispatcher.Invoke(() =>
                {
                    Terminal.WriteLine($"Поисковая строка: {responseObject.SearchText}");
                    Terminal.WriteLine($"Результаты: {responseObject.FileViews.ToString()}");
                });
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} {response.ReasonPhrase}");
            }

        }

        private async Task StartListening()
        {
            while (!_cancel)
            {
                HttpListenerContext context = await Listener.GetContextAsync();

                var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string requestBody = await reader.ReadToEndAsync();

                object responseObject = null;
                if (!context.Request.HttpMethod.Equals(HttpMethod.Post.Method))
                {   
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.ContentType = "text/plain";

                    byte[] errorBuffer = Encoding.UTF8.GetBytes("Не найдено.");

                    await context.Response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                    context.Response.Close();
                    continue;
                }
                if (string.IsNullOrEmpty(requestBody))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "text/plain";

                    byte[] errorBuffer = Encoding.UTF8.GetBytes("Неверный формат.");

                    await context.Response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                    context.Response.Close();
                    continue;
                }
                    var requestObject = JsonConvert.DeserializeObject<RequestModel>(requestBody);
                if (requestObject != null)
                {
                    if (requestObject.RequestType == RequestType.Similarities)
                    {
                        var searchBody = JsonConvert.DeserializeObject<SearchBody>(requestObject.Body.ToString());
                        var allViews = IndexingHelper.GetFileViews(searchBody.SearchText);
                        var takenViews = allViews.OrderByDescending(x => x.Percentage).Take(searchBody.MaxCount).ToList();
                        responseObject = new SimilaritiesResponse(searchBody.SearchText, takenViews);
                    }
                } else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "text/plain";

                    byte[] errorBuffer = Encoding.UTF8.GetBytes("Неверный формат.");

                    await context.Response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                    context.Response.Close();
                    continue;
                }






                string responseString = JsonConvert.SerializeObject(responseObject);
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                context.Response.ContentLength64 = buffer.Length;
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";

                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

                context.Response.Close();

            }
            
        }

        public class SimilaritiesResponse
        {
            public string SearchText { get; set; }
            public List<FileView> FileViews { get; set; }
            public SimilaritiesResponse(string text, List<FileView> views)
            {
                SearchText = text;
                FileViews = views;
            }
        }

        private bool _cancel = false;

        public void Stop()
        {
            if (Listener != null && !Listener.IsListening)
            {
                Terminal.WriteLine("Прослушивание ещё не началось...");
                return;
            }
            Terminal.WriteLine("Остановка прослушивания...");
            _cancel = true;
            Listener.Stop();
            Listener.Close();
            Terminal.WriteLine("Прослушивание остановлено...");
        }
    }
}
