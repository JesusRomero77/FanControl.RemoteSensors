using FanControl.Plugins;

namespace FanControl.RemoteSensors
{
    // Sensor ficticio: FanControl solo acepta un plugin que añada al menos un sensor.
    // Solo se usa cuando no hay sensores remotos que mostrar. No mide nada.
    internal class DummySensor : IPluginSensor
    {
        public string Id => "RemoteSensors";
        public string Name => "Remote Sensors";
        public float? Value => null;
        public void Update()
        {
        }
    }
}