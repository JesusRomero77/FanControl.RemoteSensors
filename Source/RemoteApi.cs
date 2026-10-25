using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FanControl.RemoteSensors
{
    // Cliente HTTP de solo lectura para las rutas /profile y /readings de DataServer.
    internal sealed class RemoteApi
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public RemoteApi(string baseUrl, int timeoutMs)
        {
            _baseUrl = baseUrl;
            _http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeoutMs) };
        }

        public ProfileInfo GetProfile()
        {
            return Get<ProfileInfo>("profile");
        }

        public ReadingsSnapshot GetReadings()
        {
            return Get<ReadingsSnapshot>("readings");
        }

        // Petición síncrona; lanza excepción si hay timeout, error de red o un código distinto de 200
        // (por ejemplo 503 mientras DataServer aún no tiene datos). Task.Run evita que la espera
        // se enganche al contexto de sincronización si se llama desde un hilo de la interfaz.
        private T Get<T>(string route)
        {
            string json = Task.Run(async () =>
            {
                using (HttpResponseMessage response = await _http.GetAsync(_baseUrl + route).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }).GetAwaiter().GetResult();

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}