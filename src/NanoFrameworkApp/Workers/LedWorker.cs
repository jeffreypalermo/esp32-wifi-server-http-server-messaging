using System;
using System.Diagnostics;
using System.Threading;
using NanoFrameworkApp.Hardware;
using NanoFrameworkApp.Messaging;

namespace NanoFrameworkApp.Workers
{
    public class LedWorker : IWorker
    {
        private readonly ILedController _ledController;
        private readonly MessageBus _messageBus;
        private readonly DeviceStatus _deviceStatus;
        private bool _isRunning;
        private int _flashCount;

        public LedWorker(ILedController ledController, MessageBus messageBus, DeviceStatus deviceStatus)
        {
            _ledController = ledController;
            _messageBus = messageBus;
            _deviceStatus = deviceStatus;
        }

        public string Name => "LedWorker";

        public bool IsRunning => _isRunning;

        public int FlashCount => _flashCount;

        public void Start()
        {
            _messageBus.Subscribe("led/flash", OnFlashMessage);
            _isRunning = true;
            Debug.WriteLine("LedWorker started");
        }

        public void Stop()
        {
            _isRunning = false;
            _ledController.TurnOff();
            Debug.WriteLine("LedWorker stopped");
        }

        private void OnFlashMessage(Message message)
        {
            try
            {
                int count = int.Parse(message.Payload);
                Debug.WriteLine("LedWorker flashing " + count.ToString() + " times");

                for (int i = 0; i < count; i++)
                {
                    _ledController.TurnOn();
                    Thread.Sleep(200);
                    _ledController.TurnOff();
                    Thread.Sleep(200);
                    _flashCount++;
                }

                _deviceStatus.RecordFlashEvent(count);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LedWorker error: " + ex.Message);
            }
        }
    }
}
