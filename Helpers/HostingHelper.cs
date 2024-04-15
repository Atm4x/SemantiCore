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

namespace SemantiCore.Helpers
{
    public class HostingHelper
    {
        public HttpListener Listener;
        private TerminalWindow Terminal;

        public HostingHelper()
        {
            Listener = new HttpListener();
            Terminal = new TerminalWindow();
            Terminal.Show();
            foreach (var address in App.MainConfig.Addresses)
            {
                var http = $"{address}";
                Listener.Prefixes.Add(http);
                Terminal.WriteLine($"Адрес прослушивания: {http}");
            }
        }

        public void Start()
        {
            Terminal.WriteLine("Запуск прослушивания...");
            _cancel = false;
            Listener.Start();
            Task.Run(async () => await Module());
            Thread.Sleep(3000);
            Task.Run(async () => await SendTest());
        }
        private async Task SendTest()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestObject = new
            {
                text = "Hello, World!"
            };

            var requestString = JsonConvert.SerializeObject(requestObject);
            var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:8080/", requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                Console.WriteLine($"Message: {responseObject.message}");
                Console.WriteLine($"Text: {responseObject.text}");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode} {response.ReasonPhrase}");
            }

        }

        private async Task Module()
        {
            while (!_cancel)
            {
                HttpListenerContext context = await Listener.GetContextAsync();

                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    string requestBody = await reader.ReadToEndAsync();

                    string textParameter = null;
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        var requestObject = JsonConvert.DeserializeObject<dynamic>(requestBody);
                        textParameter = requestObject?.text;
                    }

                    var responseObject = new
                    {
                        message = $"Hello, World! You sent: {textParameter ?? "no text parameter"}",
                        text = textParameter
                    };

                    string responseString = JsonConvert.SerializeObject(responseObject);
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";

                    using (var writer = new StreamWriter(context.Response.OutputStream, context.Response.ContentEncoding))
                    {
                        await writer.WriteAsync(responseString);

                        context.Response.Close();
                    }
                }
            }
        }

        private bool _cancel = false;

        public void Stop()
        {
            Terminal.WriteLine("Остановка прослушивания...");
            _cancel = true;
            Listener.Stop();
            Terminal.WriteLine("Прослушивание остановлено...");
        }
    }
}
