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
            return "<!DOCTYPE html>" +
                   "<html lang=\"en\"><head>" +
                   "<meta charset=\"UTF-8\">" +
                   "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">" +
                   "<title>nanoFramework " + BoardConfig.SocName + "</title>" +
                   GetCssBlock() +
                   "</head><body>" +
                   GetBodyHtml() +
                   GetScript() +
                   "</body></html>";
        }

        private static string GetCssBlock()
        {
            string lc = BoardConfig.LedColor;
            return "<style>" +
                "*{box-sizing:border-box;margin:0;padding:0}" +
                "body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;background:#0d1117;color:#c9d1d9;padding:12px;min-height:100vh}" +
                ".app{max-width:460px;margin:0 auto}" +
                ".card{background:#161b22;border:1px solid #30363d;border-radius:12px;padding:16px;margin-bottom:12px}" +
                ".hdr{display:flex;justify-content:space-between;align-items:flex-start}" +
                ".brand{font-size:.72rem;color:#8b949e;text-transform:uppercase;letter-spacing:1.5px}" +
                ".bname{font-size:1.25rem;font-weight:700;color:#e6edf3}" +
                ".chip{color:" + lc + "}" +
                ".pill{display:flex;align-items:center;gap:5px;background:#21262d;border-radius:20px;padding:4px 10px;font-size:.78rem;color:#8b949e}" +
                ".dot{width:7px;height:7px;border-radius:50%;background:#3fb950;animation:dp 2s ease-in-out infinite}" +
                "@keyframes dp{0%,100%{opacity:1}50%{opacity:.3}}" +
                ".upt{font-size:.75rem;color:#8b949e;margin-top:8px}" +
                ".lc{display:flex;flex-direction:column;align-items:center;gap:16px}" +
                ".ls{transition:filter .2s}" +
                ".ls.fl{animation:lf .42s ease-in-out;animation-iteration-count:3}" +
                "@keyframes lf{0%,100%{filter:drop-shadow(0 0 5px " + lc + ")}50%{filter:drop-shadow(0 0 22px " + lc + ") drop-shadow(0 0 50px " + lc + ")}}" +
                ".fb{width:100%;max-width:280px;padding:13px 24px;font-size:1rem;font-weight:600;color:#fff;" +
                "background:linear-gradient(135deg,#1f6feb,#388bfd);border:none;border-radius:8px;" +
                "cursor:pointer;transition:transform .1s,box-shadow .15s;box-shadow:0 3px 10px rgba(31,111,235,.4)}" +
                ".fb:active{transform:scale(.97);box-shadow:none}" +
                ".fb:disabled{opacity:.45;cursor:not-allowed}" +
                "#msg{font-size:.82rem;color:#8b949e;min-height:1.2em;text-align:center}" +
                ".ct{text-align:center}" +
                ".clbl{font-size:.72rem;color:#8b949e;text-transform:uppercase;letter-spacing:1.5px;margin-bottom:6px}" +
                ".cv{font-size:3rem;font-weight:800;color:" + lc + ";transition:transform .2s}" +
                ".cv.bump{transform:scale(1.18)}" +
                ".hlbl{font-size:.72rem;color:#8b949e;text-transform:uppercase;letter-spacing:1.5px;margin-bottom:10px}" +
                "footer{text-align:center;font-size:.72rem;color:#484f58;padding:6px 0 16px}" +
                "</style>";
        }

        private static string GetBodyHtml()
        {
            string soc = BoardConfig.SocName;
            string lc = BoardConfig.LedColor;
            string ll = BoardConfig.LedColorLight;
            string ld = BoardConfig.LedColorDark;
            string pin = BoardConfig.LedPin.ToString();
            string act = BoardConfig.LedActiveHigh ? "Active-High" : "Active-Low";

            return "<div class=\"app\">" +
                "<div class=\"card\">" +
                "<div class=\"hdr\">" +
                "<div>" +
                "<div class=\"brand\">nanoFramework</div>" +
                "<div class=\"bname\"><span class=\"chip\">" + soc + "</span></div>" +
                "</div>" +
                "<div class=\"pill\"><span class=\"dot\"></span><span>Online</span></div>" +
                "</div>" +
                "<div class=\"upt\" id=\"upt\">Uptime: --:--</div>" +
                "</div>" +
                "<div class=\"card lc\">" +
                "<svg id=\"led\" class=\"ls\" width=\"110\" height=\"110\" viewBox=\"-55 -55 110 110\">" +
                "<defs>" +
                "<radialGradient id=\"rg\" cx=\"35%\" cy=\"35%\">" +
                "<stop offset=\"0%\" stop-color=\"" + ll + "\"/>" +
                "<stop offset=\"50%\" stop-color=\"" + lc + "\"/>" +
                "<stop offset=\"100%\" stop-color=\"" + ld + "\"/>" +
                "</radialGradient>" +
                "</defs>" +
                "<circle r=\"52\" fill=\"#0d1117\" stroke=\"#30363d\" stroke-width=\"1.5\"/>" +
                "<circle r=\"38\" fill=\"url(#rg)\"/>" +
                "<circle cx=\"-13\" cy=\"-13\" r=\"9\" fill=\"white\" opacity=\"0.13\"/>" +
                "</svg>" +
                "<button class=\"fb\" id=\"btn\" onclick=\"doFlash()\">&#9889; Flash LED</button>" +
                "<div id=\"msg\"></div>" +
                "</div>" +
                "<div class=\"card ct\">" +
                "<div class=\"clbl\">Total Flashes</div>" +
                "<div class=\"cv\" id=\"cnt\">0</div>" +
                "</div>" +
                "<div class=\"card\">" +
                "<div class=\"hlbl\">Flash History</div>" +
                "<svg id=\"ch\" viewBox=\"0 0 320 72\" width=\"100%\" height=\"72\" style=\"display:block\">" +
                "<text x=\"160\" y=\"40\" text-anchor=\"middle\" fill=\"#484f58\" font-size=\"13\">No flash events yet</text>" +
                "</svg>" +
                "</div>" +
                "<footer>GPIO " + pin + " &middot; " + act + " &middot; 192.168.4.1</footer>" +
                "</div>";
        }

        private static string GetScript()
        {
            string lc = BoardConfig.LedColor;
            return "<script>" +
                "var fh=[],tot=0;" +
                "function doFlash(){" +
                "var btn=document.getElementById('btn');" +
                "var led=document.getElementById('led');" +
                "btn.disabled=true;" +
                "document.getElementById('msg').textContent='Flashing...';" +
                "led.classList.remove('fl');" +
                "void led.getBoundingClientRect();" +
                "led.classList.add('fl');" +
                "fetch('/api/led/flash',{method:'POST'})" +
                ".then(function(r){return r.text();})" +
                ".then(function(){" +
                "document.getElementById('msg').textContent='Done!';" +
                "setTimeout(function(){document.getElementById('msg').textContent='';},2000);" +
                "btn.disabled=false;" +
                "fetchS();fetchH();" +
                "})" +
                ".catch(function(){" +
                "document.getElementById('msg').textContent='Error';" +
                "btn.disabled=false;" +
                "});}" +
                "function fetchS(){" +
                "fetch('/api/status')" +
                ".then(function(r){return r.text();})" +
                ".then(function(t){" +
                "var d=JSON.parse(t);" +
                "var p=tot;tot=d.count||0;" +
                "var e=document.getElementById('cnt');" +
                "e.textContent=tot;" +
                "if(tot!==p){e.classList.add('bump');setTimeout(function(){e.classList.remove('bump');},300);}" +
                "document.getElementById('upt').textContent='Uptime '+fmtU(d.uptime||0);" +
                "}).catch(function(){});}" +
                "function fetchH(){" +
                "fetch('/api/history')" +
                ".then(function(r){return r.text();})" +
                ".then(function(t){fh=JSON.parse(t);drawC();})" +
                ".catch(function(){});}" +
                "function drawC(){" +
                "var sv=document.getElementById('ch');" +
                "if(!fh||!fh.length){" +
                "sv.innerHTML='<text x=\"160\" y=\"40\" text-anchor=\"middle\" fill=\"#484f58\" font-size=\"13\">No flash events yet</text>';" +
                "return;}" +
                "var W=320,H=68,p=3,max=1;" +
                "for(var i=0;i<fh.length;i++)if(fh[i]>max)max=fh[i];" +
                "var bw=Math.max(8,Math.floor((W-p*(fh.length+1))/fh.length));" +
                "var h='<defs><linearGradient id=\"gr\" x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\"><stop offset=\"0%\" stop-color=\"" + lc + "\"/><stop offset=\"100%\" stop-color=\"#1f6feb\"/></linearGradient></defs>';" +
                "for(var j=0;j<fh.length;j++){" +
                "var bh=Math.max(5,Math.round((fh[j]/max)*(H-10)));" +
                "var x=p+j*(bw+p);" +
                "h+='<rect x=\"'+x+'\" y=\"'+(H-bh)+'\" width=\"'+bw+'\" height=\"'+bh+'\" rx=\"3\" fill=\"url(#gr)\"/>';}" +
                "sv.innerHTML=h;}" +
                "function fmtU(ms){" +
                "var s=Math.floor(ms/1000),h=Math.floor(s/3600),m=Math.floor((s%3600)/60),sec=s%60;" +
                "return (h?h+'h ':'')+('0'+m).slice(-2)+':'+('0'+sec).slice(-2);}" +
                "setInterval(fetchS,3000);" +
                "setInterval(fetchH,6000);" +
                "fetchS();fetchH();" +
                "</script>";
        }
    }
}
