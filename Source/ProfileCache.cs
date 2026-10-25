using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace FanControl.RemoteSensors
{
    // Guarda en disco la última lista de sensores remotos conocida, para poder crear los sensores
    // en el arranque aunque el PC remoto esté apagado o DataServer aún no responda.
    internal static class ProfileCache
    {
        private static string FilePath => Path.Combine(ClientSettings.FolderPath, "RemoteSensorsProfileCache.json");

        // Devuelve el perfil guardado, o null si no existe o no se puede leer.
        public static ProfileInfo Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return null;
                return JsonConvert.DeserializeObject<ProfileInfo>(File.ReadAllText(FilePath));
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ErrorLogger.Severity.Warning, ex);
                return null;
            }
        }

        public static void Save(ProfileInfo profile)
        {
            try
            {
                Directory.CreateDirectory(ClientSettings.FolderPath);
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(profile, Formatting.Indented));
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ErrorLogger.Severity.Warning, ex);
            }
        }

        // Dos perfiles tienen los mismos sensores si coinciden en Identifier y Name, sin importar el orden.
        public static bool HaveSameSensors(ProfileInfo first, ProfileInfo second)
        {
            if (first == null || second == null) return false;

            IEnumerable<string> firstKeys = Keys(first);
            IEnumerable<string> secondKeys = Keys(second);
            return firstKeys.SequenceEqual(secondKeys);
        }

        private static IEnumerable<string> Keys(ProfileInfo profile)
        {
            return (profile.Sensors ?? new List<ElementInfo>()).Select(sensor => sensor.Identifier + "|" + sensor.Name)
                .OrderBy(key => key, StringComparer.Ordinal);
        }
    }
}