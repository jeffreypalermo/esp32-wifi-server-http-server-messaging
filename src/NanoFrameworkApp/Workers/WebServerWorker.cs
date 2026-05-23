using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using NanoFrameworkApp.Messaging;

namespace NanoFrameworkApp.Workers
{
    public class WebServerWorker : IWorker
    {
        private readonly MessageBus _messageBus;
        private Thread _serverThread;
        private HttpListener _listener;
        private bool _isRunning;

        public WebServerWorker(MessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public string Name => "WebServerWorker";

        public bool IsRunning => _isRunning;

        public void Start()
        {
            _isRunning = true;
            _serverThread = new Thread(RunServer);
            _serverThread.Start();
            Debug.WriteLine("WebServerWorker started on port 80");
        }

        public void Stop()
        {
            _isRunning = false;

            if (_listener != null)
            {
                _listener.Stop();
            }

            Debug.WriteLine("WebServerWorker stopped");
        }

        private void RunServer()
        {
            _listener = new HttpListener("http", 80);
            _listener.Start();

            while (_isRunning)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Debug.WriteLine("WebServerWorker error: " + ex.Message);
                    }
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                string url = context.Request.RawUrl;
                string method = context.Request.HttpMethod;

                Debug.WriteLine("WebServer: " + method + " " + url);

                string responseString;
                string contentType;
                int statusCode = 200;

                if (url == "/api/led/flash" && method == "POST")
                {
                    _messageBus.Publish(new Message("led/flash", "3"));
                    responseString = "LED flash triggered";
                    contentType = "text/plain";
                }
                else if (url == "/api/status")
                {
                    responseString = "OK";
                    contentType = "text/plain";
                }
                else
                {
                    // Serve homepage for any other request
                    responseString = GetHtmlPage();
                    contentType = "text/html";
                }

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = contentType;
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WebServerWorker request error: " + ex.Message);
                try { context.Response.Close(); } catch { }
            }
        }

        private static string GetHtmlPage()
        {
            return
                "<!DOCTYPE html>" +
                "<html><head>" +
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" +
                "<title>nanoFramework ESP32-S3</title>" +
                "<style>" +
                "body { font-family: Arial, sans-serif; text-align: center; margin-top: 50px; background: #f0f0f0; }" +
                "h1 { color: #333; }" +
                "button { padding: 15px 30px; font-size: 18px; cursor: pointer; background: #0078d4; color: white; border: none; border-radius: 5px; }" +
                "button:hover { background: #005a9e; }" +
                "#status { margin-top: 20px; font-size: 16px; color: #555; }" +
                "</style>" +
                "</head><body>" +
                "<h1>nanoFramework ESP32-S3</h1>" +
                "<button onclick=\"flashLed()\">Flash LED</button>" +
                "<p id=\"status\"></p>" +
                "<script>" +
                "function flashLed() {" +
                "  document.getElementById('status').textContent = 'Sending...';" +
                "  fetch('/api/led/flash', { method: 'POST' })" +
                "    .then(function(r) { return r.text(); })" +
                "    .then(function(t) { document.getElementById('status').textContent = t; })" +
                "    .catch(function(e) { document.getElementById('status').textContent = 'Error: ' + e; });" +
                "}" +
                "</script>" +
                "</body></html>";
        }
    }
}
