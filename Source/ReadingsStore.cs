using System;
using System.Collections.Generic;

namespace FanControl.RemoteSensors
{
    // Última foto de lecturas recibida. El hilo de sondeo la publica y los sensores la leen.
    // Si el contador (sequence) deja de cambiar durante MaxAge, los datos se consideran parados
    // y se devuelve null en vez de un valor congelado. Se mide con el reloj local, no con el remoto.
    internal static class ReadingsStore
    {
        private static readonly TimeSpan MaxAge = TimeSpan.FromSeconds(5);
        private static readonly object _lock = new object();

        private static Dictionary<string, float?> _values = new Dictionary<string, float?>();
        private static long _lastSequence = -1;
        private static DateTime _lastChangeUtc = DateTime.MinValue;

        // Sustituye los valores por los de la foto nueva y anota cuándo cambió el contador por última vez.
        public static void Publish(ReadingsSnapshot snapshot)
        {
            Dictionary<string, float?> values = new Dictionary<string, float?>();
            if (snapshot.Sensors != null)
                foreach (Reading reading in snapshot.Sensors)
                {
                    if (reading.Identifier == null) continue;
                    values[reading.Identifier] = reading.Value.HasValue ? (float?)reading.Value.Value : null;
                }

            lock (_lock)
            {
                _values = values;
                if (snapshot.Sequence != _lastSequence)
                {
                    _lastSequence = snapshot.Sequence;
                    _lastChangeUtc = DateTime.UtcNow;
                }
            }
        }

        // Devuelve el último valor del sensor, o null si no existe, no tiene valor o los datos están parados.
        public static float? GetValue(string identifier)
        {
            lock (_lock)
            {
                if (DateTime.UtcNow - _lastChangeUtc > MaxAge) return null;

                float? value;
                return _values.TryGetValue(identifier, out value) ? value : null;
            }
        }
    }
}