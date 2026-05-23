using System;
using System.Diagnostics;
using System.Threading;
using NanoFrameworkApp.Hardware;
using NanoFrameworkApp.Messaging;
using NanoFrameworkApp.Workers;

namespace NanoFrameworkApp
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("=== nanoFramework ESP32-S3 Starting ===");

            // Create shared LED controller (only one can own the GPIO pin)
            ILedController ledController = null;
            try
            {
                ledController = new GpioLedController(21);
                // Blink 3 times = "I'm alive"
                for (int i = 0; i < 3; i++)
                {
                    ledController.TurnOn();
                    Thread.Sleep(200);
                    ledController.TurnOff();
                    Thread.Sleep(200);
                }
                Debug.WriteLine("LED controller OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LED init error: " + ex.Message);
            }

            // Start WiFi AP
            WifiApWorker wifiApWorker = null;
            try
            {
                Debug.WriteLine("Starting WiFi AP...");
                wifiApWorker = new WifiApWorker();
                wifiApWorker.Start();
                Debug.WriteLine("WiFi AP started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WiFi AP error: " + ex.Message);
            }

            // Give WiFi time to fully initialize
            Thread.Sleep(5000);

            // Create message bus and start workers
            MessageBus messageBus = new MessageBus();

            // Start LED worker
            try
            {
                LedWorker ledWorker = new LedWorker(ledController, messageBus);
                ledWorker.Start();
                Debug.WriteLine("LED worker started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LED worker error: " + ex.Message);
            }

            // Start web server
            try
            {
                WebServerWorker webServerWorker = new WebServerWorker(messageBus);
                webServerWorker.Start();
                Debug.WriteLine("Web server started on port 80");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Web server error: " + ex.Message);
            }

            Debug.WriteLine("=== All workers started ===");
            Debug.WriteLine("Connect to WiFi: " + WifiApWorker.SoftApSsid);
            Debug.WriteLine("Open browser: http://" + WifiApWorker.SoftApIP);

            // Signal ready with LED
            if (ledController != null)
            {
                ledController.TurnOn();
                Thread.Sleep(1000);
                ledController.TurnOff();
            }

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
