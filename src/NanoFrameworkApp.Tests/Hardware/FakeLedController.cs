using NanoFrameworkApp.Hardware;

namespace NanoFrameworkApp.Tests.Hardware
{
    public class FakeLedController : ILedController
    {
        private bool _isOn;
        private int _turnOnCount;
        private int _turnOffCount;

        public void TurnOn()
        {
            _isOn = true;
            _turnOnCount++;
        }

        public void TurnOff()
        {
            _isOn = false;
            _turnOffCount++;
        }

        public bool IsOn => _isOn;
        public int TurnOnCount => _turnOnCount;
        public int TurnOffCount => _turnOffCount;

        public void Reset()
        {
            _isOn = false;
            _turnOnCount = 0;
            _turnOffCount = 0;
        }
    }
}
