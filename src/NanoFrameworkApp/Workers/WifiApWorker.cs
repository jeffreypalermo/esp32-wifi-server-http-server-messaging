using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Iot.Device.DhcpServer;
using nanoFramework.Runtime.Native;
using NanoFrameworkApp.Hardware;

namespace NanoFrameworkApp.Workers
{
    public class WifiApWorker : IWorker
    {
        public static string SoftApIP = "192.168.4.1";
        public static string SoftApSsid = "NanoFramework-" + BoardConfig.SocName;

        private bool _isRunning;
        private DhcpServer _dhcpServer;

        public string Name => "WifiApWorker";

        public bool IsRunning => _isRunning;

        public void Start()
        {
            Debug.WriteLine("WifiApWorker: Configuring SoftAP...");

            // Check if AP is already running (from AutoStart on previous boot)
            bool apAlreadyRunning = false;
            try
            {
                IPAddress ip = IPAddress.GetDefaultLocalAddress();
                if (ip != null && ip.ToString() != "0.0.0.0")
                {
                    Debug.WriteLine("WifiApWorker: IP already assigned: " + ip.ToString());
                    apAlreadyRunning = true;
                }
            }
            catch { }

            // Get all wireless AP configurations
            WirelessAPConfiguration[] configs = WirelessAPConfiguration.GetAllWirelessAPConfigurations();
            Debug.WriteLine("WifiApWorker: Found " + configs.Length + " AP configs");

            if (configs.Length == 0)
            {
                Debug.WriteLine("WifiApWorker: ERROR - No AP configurations available!");
                return;
            }

            WirelessAPConfiguration wapconf = configs[0];

            // Check if already configured correctly
            bool needsConfig = (wapconf.Ssid != SoftApSsid) ||
                               (wapconf.Options & WirelessAPConfiguration.ConfigurationOptions.Enable) == 0;

            if (needsConfig)
            {
                Debug.WriteLine("WifiApWorker: Saving new AP configuration...");
                wapconf.Ssid = SoftApSsid;
                wapconf.Password = "";
                wapconf.Authentication = System.Net.NetworkInformation.AuthenticationType.Open;
                wapconf.Encryption = EncryptionType.None;
                wapconf.Radio = RadioType._802_11n;
                wapconf.MaxConnections = 4;

                wapconf.Options =
                    WirelessAPConfiguration.ConfigurationOptions.AutoStart |
                    WirelessAPConfiguration.ConfigurationOptions.Enable;

                wapconf.SaveConfiguration();
                Debug.WriteLine("WifiApWorker: Configuration saved - rebooting to activate AP...");

                // Reboot to activate the AP (AutoStart takes effect on boot)
                Thread.Sleep(1000);
                Power.RebootDevice();
                // Code below won't execute after reboot
                return;
            }

            Debug.WriteLine("WifiApWorker: AP already configured, waiting for ready...");

            // Wait for the AP to become available
            if (!WaitForApReady(15000))
            {
                Debug.WriteLine("WifiApWorker: Timeout waiting for AP - may need another reboot");
            }
            else
            {
                Debug.WriteLine("WifiApWorker: AP interface is ready");
            }

            // Configure static IP on the AP network interface
            try
            {
                NetworkInterface ni = GetApNetworkInterface();
                Debug.WriteLine("WifiApWorker: Found network interface type: " + ni.NetworkInterfaceType.ToString());
                ni.EnableStaticIPv4(SoftApIP, "255.255.255.0", SoftApIP);
                Debug.WriteLine("WifiApWorker: Static IP configured");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WifiApWorker: IP config error: " + ex.Message);
            }

            // Start DHCP server
            try
            {
                _dhcpServer = new DhcpServer();
                _dhcpServer.CaptivePortalUrl = SoftApIP;
                _dhcpServer.Start(
                    IPAddress.Parse(SoftApIP),
                    new IPAddress(new byte[] { 255, 255, 255, 0 }));
                Debug.WriteLine("WifiApWorker: DHCP server started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WifiApWorker: DHCP error: " + ex.Message);
            }

            _isRunning = true;
            Debug.WriteLine("WifiApWorker: SoftAP started - SSID: " + SoftApSsid + " IP: " + SoftApIP);
        }

        public void Stop()
        {
            try
            {
                WirelessAPConfiguration wapconf = GetWirelessApConfiguration();
                wapconf.Options = WirelessAPConfiguration.ConfigurationOptions.None;
                wapconf.SaveConfiguration();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WifiApWorker: Error stopping AP - " + ex.Message);
            }

            _isRunning = false;
            Debug.WriteLine("WifiApWorker stopped");
        }

        private static bool WaitForApReady(int timeoutMs)
        {
            int elapsed = 0;
            const int pollInterval = 500;

            while (elapsed < timeoutMs)
            {
                IPAddress ip = IPAddress.GetDefaultLocalAddress();
                if (ip != null && ip.ToString() != "0.0.0.0")
                {
                    return true;
                }

                Thread.Sleep(pollInterval);
                elapsed += pollInterval;
            }

            return false;
        }

        private static WirelessAPConfiguration GetWirelessApConfiguration()
        {
            WirelessAPConfiguration[] configs = WirelessAPConfiguration.GetAllWirelessAPConfigurations();
            return configs[0];
        }

        private static NetworkInterface GetApNetworkInterface()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            // Look for the wireless AP interface
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].NetworkInterfaceType == NetworkInterfaceType.WirelessAP)
                {
                    return interfaces[i];
                }
            }

            // Fallback: use the last interface (AP is typically second)
            return interfaces[interfaces.Length - 1];
        }
    }
}
