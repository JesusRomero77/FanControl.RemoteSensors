using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FanControl.RemoteSensors
{
    // Cliente HTTP de solo lectura para las rutas /profile y /readings de DataServer. Cada consulta
    // son dos peticiones: /challenge para obtener un número de un solo uso y después la ruta de datos
    // con la cabecera Authorization: HMAC <número>:<huella>, donde la huella es HMAC-SHA256 del número
    // con la contraseña como clave. La contraseña no viaja nunca.
    internal sealed class RemoteApi
    {
        private const string AuthorizationScheme = "HMAC ";

        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _password;

        public RemoteApi(string baseUrl, string password, int timeoutMs)
        {
            _baseUrl = baseUrl;
            _password = password;
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

        // Petición síncrona; lanza excepción si hay timeout, error de red, contraseña rechazada o un
        // código distinto de 200 (por ejemplo 503 mientras DataServer aún no tiene datos). Task.Run evita
        // que la espera se enganche al contexto de sincronización si se llama desde un hilo de la interfaz.
        private T Get<T>(string route)
        {
            string json = Task.Run(async () =>
            {
                string nonce = await RequestNonceAsync().ConfigureAwait(false);

                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _baseUrl + route))
                {
                    request.Headers.TryAddWithoutValidation("Authorization",
                        AuthorizationScheme + nonce + ":" + ComputeHmac(_password, nonce));

                    using (HttpResponseMessage response = await _http.SendAsync(request).ConfigureAwait(false))
                    {
                        ThrowIfRejected(response);
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
            }).GetAwaiter().GetResult();

            return JsonConvert.DeserializeObject<T>(json);
        }

        // Pide a /challenge el número de un solo uso que hay que firmar.
        private async Task<string> RequestNonceAsync()
        {
            using (HttpResponseMessage response = await _http.GetAsync(_baseUrl + "challenge").ConfigureAwait(false))
            {
                ThrowIfRejected(response);
                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                string nonce = JsonConvert.DeserializeObject<ChallengeResponse>(json)?.Nonce;
                if (string.IsNullOrEmpty(nonce)) throw new InvalidOperationException("DataServer sent an empty challenge.");
                return nonce;
            }
        }

        // Convierte en excepciones con mensaje claro los rechazos propios de DataServer (contraseña
        // incorrecta y límite de clientes) y deja el resto de códigos al método estándar.
        private static void ThrowIfRejected(HttpResponseMessage response)
        {
            switch ((int)response.StatusCode)
            {
                case 401:
                    throw new InvalidOperationException(
                        "DataServer rejected the password. Check <Password> in " + ClientSettings.FilePath + ".");
                case 429:
                    throw new InvalidOperationException(
                        "DataServer refused the request: too many clients or connections.");
                default:
                    response.EnsureSuccessStatusCode();
                    break;
            }
        }

        // Huella HMAC-SHA256 del número con la contraseña como clave, en hexadecimal y minúsculas.
        // Debe dar exactamente lo mismo que HmacAuth.Compute del servidor.
        private static string ComputeHmac(string password, string nonce)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(password)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(nonce));
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        // Respuesta de /challenge. Newtonsoft ignora mayúsculas al leer, así que "nonce" encaja con Nonce.
        private class ChallengeResponse
        {
            public string Nonce { get; set; }
        }
    }
}