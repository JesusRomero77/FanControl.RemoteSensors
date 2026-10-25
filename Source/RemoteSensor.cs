using FanControl.Plugins;

namespace FanControl.RemoteSensors
{
    // Sensor de FanControl que muestra el valor de un sensor del PC remoto. El nombre empieza
    // por REMOTE para que sea evidente que no es un sensor de este PC.
    internal class RemoteSensor : IPluginSensor
    {
        private readonly string _identifier;

        public RemoteSensor(ElementInfo info)
        {
            _identifier = info.Identifier;
            Id = "RemoteSensors/" + info.Identifier;
            Name = "REMOTE " + info.Name;
        }

        public string Id { get; }

        public string Name { get; }

        public float? Value => ReadingsStore.GetValue(_identifier);

        public void Update()
        {
        }
    }
}