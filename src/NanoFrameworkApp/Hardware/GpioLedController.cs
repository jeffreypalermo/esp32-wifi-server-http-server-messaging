using System.Device.Gpio;
using System.Diagnostics;

namespace NanoFrameworkApp.Hardware
{
    public class GpioLedController : ILedController
    {
        private readonly GpioController _gpio;
        private readonly int _pinNumber;
        private readonly PinValue _onValue;
        private readonly PinValue _offValue;
        private bool _isOn;

        public GpioLedController(int pinNumber, bool activeHigh)
        {
            _pinNumber = pinNumber;
            _onValue  = activeHigh ? PinValue.High : PinValue.Low;
            _offValue = activeHigh ? PinValue.Low  : PinValue.High;
            _gpio = new GpioController();
            _gpio.OpenPin(_pinNumber, PinMode.Output);
            _gpio.Write(_pinNumber, _offValue);
            _isOn = false;

            Debug.WriteLine("GpioLedController pin " + _pinNumber + " activeHigh=" + activeHigh.ToString());
        }

        public void TurnOn()
        {
            _gpio.Write(_pinNumber, _onValue);
            _isOn = true;
        }

        public void TurnOff()
        {
            _gpio.Write(_pinNumber, _offValue);
            _isOn = false;
        }

        public bool IsOn => _isOn;
    }
}
