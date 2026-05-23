namespace NanoFrameworkApp.Hardware
{
    public interface ILedController
    {
        void TurnOn();
        void TurnOff();
        bool IsOn { get; }
    }
}
