using FanControl.Plugins;
using System;
using System.Threading;

namespace FanControl.RemoteSensors
{
    public class RemoteSensorsPlugin : IPlugin2
    {
        private const int PollIntervalMs = 1000;
        private const int LoadTimeoutMs = 2000;

        public string Name => "Remote Sensors";

        private ClientSettings _settings;
        private RemoteApi _api;

        // Perfil remoto con el que se crearon los sensores en Load (null si no se pudo obtener).
        private ProfileInfo _loadedProfile;

        // Último ProfileId remoto procesado; solo lo toca el hilo de sondeo.
        private int _lastSeenProfileId = -1;
        private volatile bool _stopping;

        public RemoteSensorsPlugin(IPluginLogger logger)
        {
            ErrorLogger.Initialize(logger);
        }

        public void Initialize()
        {
        }

        // Crea un sensor por cada sensor del perfil remoto y arranca el hilo que pide las lecturas.
        public void Load(IPluginSensorsContainer container)
        {
            ErrorLogger.Write(ErrorLogger.Severity.Warning,
                "REMOTE SENSORS ARE FOR MONITORING ONLY. NEVER USE THEM IN CURVES OR CONTROLS OF THIS PC: " +
                "IF THE REMOTE DATA STOPS OR IS WRONG, THIS PC COULD LOSE THE COOLING IT NEEDS AND THE " +
                "HARDWARE COULD BE PERMANENTLY DAMAGED. LOCAL HARDWARE COULD ALSO BE DAMAGED IF ITS " +
                "COOLING DEPENDS ON A REMOTE SENSOR INSTEAD OF ON ITS OWN SENSOR.");

            _settings = ClientSettings.Load();

            int added = 0;
            if (_settings.IsConfigured)
            {
                _api = new RemoteApi(_settings.BaseUrl, LoadTimeoutMs);
                _loadedProfile = ProfileCache.Load() ?? FetchProfileNow();

                if (_loadedProfile != null && _loadedProfile.Sensors != null)
                    foreach (ElementInfo info in _loadedProfile.Sensors)
                    {
                        container.TempSensors.Add(new RemoteSensor(info));
                        added++;
                    }

                Thread thread = new Thread(PollLoop) { IsBackground = true };
                thread.Start();
            }
            else
            {
                ErrorLogger.Write(ErrorLogger.Severity.Warning,
                    "Neither the PC name nor the IP is configured. Edit " + ClientSettings.FilePath +
                    " and restart the FanControl interface.");
            }

            // FanControl exige que el plugin añada al menos un sensor.
            if (added == 0) container.TempSensors.Add(new DummySensor());
        }

        // Pide /profile en el arranque cuando no hay caché; guarda la caché si lo consigue.
        private ProfileInfo FetchProfileNow()
        {
            try
            {
                ProfileInfo profile = _api.GetProfile();
                ProfileCache.Save(profile);
                return profile;
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ErrorLogger.Severity.Warning, "Could not read the remote profile: " + ex.Message);
                return null;
            }
        }

        // Cada segundo pide /readings y las publica en ReadingsStore. Si el ProfileId remoto cambia,
        // comprueba si la lista de sensores ha cambiado. Los fallos de red solo se anotan y se reintenta.
        private void PollLoop()
        {
            while (!_stopping)
            {
                try
                {
                    ReadingsSnapshot snapshot = _api.GetReadings();
                    ReadingsStore.Publish(snapshot);
                    if (snapshot.ProfileId != _lastSeenProfileId) CheckRemoteProfile();
                }
                catch (Exception ex)
                {
                    ErrorLogger.Write(ErrorLogger.Severity.Warning, "Could not read the remote data: " + ex.Message);
                }
                Thread.Sleep(PollIntervalMs);
            }
        }

        // Compara los sensores del perfil remoto con los creados en Load. Si difieren, actualiza la caché
        // y avisa: la lista de sensores solo se puede cambiar reiniciando la interfaz de FanControl.
        private void CheckRemoteProfile()
        {
            ProfileInfo remote = _api.GetProfile();
            _lastSeenProfileId = remote.ProfileId;

            if (ProfileCache.HaveSameSensors(_loadedProfile, remote)) return;

            ProfileCache.Save(remote);
            ErrorLogger.Write(ErrorLogger.Severity.Warning,
                "The remote profile has different sensors than the ones loaded. Restart the FanControl interface to refresh the REMOTE sensors.");
        }

        // Los sensores leen su valor directamente de ReadingsStore, así que aquí no hay nada que hacer.
        public void Update()
        {
        }

        public void Close()
        {
            _stopping = true;
        }
    }
}