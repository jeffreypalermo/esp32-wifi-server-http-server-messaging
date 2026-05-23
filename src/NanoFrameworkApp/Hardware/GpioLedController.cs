using System.Device.Gpio;
using System.Diagnostics;

namespace NanoFrameworkApp.Hardware
{
    public class GpioLedController : ILedController
    {
        private readonly GpioController _gpio;
        private readonly int _pinNumber;
        private bool _isOn;

        public GpioLedController(int pinNumber)
        {
            _pinNumber = pinNumber;
            _gpio = new GpioController();
            _gpio.OpenPin(_pinNumber, PinMode.Output);
            _isOn = false;

            Debug.WriteLine("GpioLedController initialized on pin " + _pinNumber);
        }

        public void TurnOn()
        {
            _gpio.Write(_pinNumber, PinValue.High);
            _isOn = true;
        }

        public void TurnOff()
        {
            _gpio.Write(_pinNumber, PinValue.Low);
            _isOn = false;
        }

        public bool IsOn => _isOn;
    }
}
