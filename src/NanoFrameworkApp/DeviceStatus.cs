using System;
using System.Collections;
using System.Text;
using NanoFrameworkApp.Hardware;

namespace NanoFrameworkApp
{
    public class DeviceStatus
    {
        private readonly object _lock = new object();
        private readonly ArrayList _history = new ArrayList();
        private const int MaxHistory = 20;
        private int _totalFlashes;
        private readonly DateTime _startTime;

        public DeviceStatus()
        {
            _startTime = DateTime.UtcNow;
        }

        public void RecordFlashEvent(int count)
        {
            lock (_lock)
            {
                _totalFlashes += count;
                _history.Add(count);
                if (_history.Count > MaxHistory)
                    _history.RemoveAt(0);
            }
        }

        public string GetStatusJson()
        {
            lock (_lock)
            {
                long uptimeMs = (DateTime.UtcNow - _startTime).Ticks / 10000L;
                return "{\"board\":\"" + BoardConfig.SocName +
                       "\",\"count\":" + _totalFlashes.ToString() +
                       ",\"uptime\":" + uptimeMs.ToString() + "}";
            }
        }

        public string GetHistoryJson()
        {
            lock (_lock)
            {
                if (_history.Count == 0) return "[]";
                StringBuilder sb = new StringBuilder();
                sb.Append("[");
                for (int i = 0; i < _history.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(_history[i].ToString());
                }
                sb.Append("]");
                return sb.ToString();
            }
        }
    }
}
