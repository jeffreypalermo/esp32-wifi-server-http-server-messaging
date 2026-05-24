using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using NanoFrameworkApp.Hardware;
using NanoFrameworkApp.Messaging;

namespace NanoFrameworkApp.Workers
{
    public class WebServerWorker : IWorker
    {
        private readonly MessageBus _messageBus;
        private readonly DeviceStatus _deviceStatus;
        private Thread _serverThread;
        private HttpListener _listener;
        private bool _isRunning;

        public WebServerWorker(MessageBus messageBus, DeviceStatus deviceStatus)
        {
            _messageBus = messageBus;
            _deviceStatus = deviceStatus;
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
                    responseString = _deviceStatus.GetStatusJson();
                    contentType = "application/json";
                }
                else if (url == "/api/history")
                {
                    responseString = _deviceStatus.GetHistoryJson();
                    contentType = "application/json";
                }
                else
                {
                    responseString = HtmlBuilder.GetHtmlPage();
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
    }
}
