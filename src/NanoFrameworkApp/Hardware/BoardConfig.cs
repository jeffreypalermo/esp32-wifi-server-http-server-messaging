namespace NanoFrameworkApp.Hardware
{
    /// <summary>
    /// Compile-time board configuration. Build with /p:ExtraDefineConstants=ESP32_C3
    /// (via build.ps1 -Target esp32c3) to target the ESP32-C3 SuperMini.
    /// Defaults to XIAO ESP32-S3.
    /// </summary>
    public static class BoardConfig
    {
#if ESP32_C3
        public static readonly SocType Soc = SocType.ESP32C3;
        /// <summary>GPIO 8 is the onboard blue LED on the ESP32-C3 SuperMini.</summary>
        public const int LedPin = 8;
        /// <summary>ESP32-C3 SuperMini LED is active-low: LOW = on, HIGH = off.</summary>
        public const bool LedActiveHigh = false;
        public const string SocName = "ESP32-C3";
        public const string NanoffTarget = "XIAO_ESP32C3";
        /// <summary>Blue LED color for SVG visuals and CSS animations.</summary>
        public const string LedColor = "#58a6ff";
        public const string LedColorLight = "#79c0ff";
        public const string LedColorDark = "#0d419d";
#else
        public static readonly SocType Soc = SocType.ESP32S3;
        /// <summary>GPIO 21 is the onboard yellow LED on the XIAO ESP32-S3.</summary>
        public const int LedPin = 21;
        /// <summary>ESP32-S3 onboard LED is active-high: HIGH = on, LOW = off.</summary>
        public const bool LedActiveHigh = true;
        public const string SocName = "ESP32-S3";
        public const string NanoffTarget = "ESP32_S3_BLE";
        /// <summary>Yellow LED color for SVG visuals and CSS animations.</summary>
        public const string LedColor = "#ffd700";
        public const string LedColorLight = "#fff2a0";
        public const string LedColorDark = "#8b6914";
#endif
    }
}
