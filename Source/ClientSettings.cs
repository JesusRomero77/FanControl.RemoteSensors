using System;
using System.IO;
using System.Xml.Serialization;

namespace FanControl.RemoteSensors
{
    // Ajustes del plugin: nombre del PC y/o IP del PC donde corre FanControl.DataServer. Se guardan
    // en un XML en %LocalAppData%\FanControl; si no existe se crea vacío para que se edite.
    public class ClientSettings
    {
        // Puerto fijo de DataServer (el mismo que ServerSettings.Port del servidor).
        private const int Port = 8765;

        public string PcName { get; set; } = "";
        public string Ip { get; set; } = "";

        public static string FolderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FanControl");

        public static string FilePath => Path.Combine(FolderPath, "RemoteSensorsConfig.xml");

        // Basta con que esté rellenado uno de los dos.
        public bool IsConfigured => !string.IsNullOrWhiteSpace(Ip) || !string.IsNullOrWhiteSpace(PcName);

        // Provisional: usa la IP si está rellena y, si no, el nombre del PC. Cuando decidas el orden
        // de prueba cuando están los dos, esta propiedad se sustituye por esa lógica.
        public string BaseUrl =>
            "http://" + (string.IsNullOrWhiteSpace(Ip) ? PcName : Ip).Trim() + ":" + Port + "/";

        // Contenido del archivo que se crea la primera vez, con comentarios para quien lo edite.
        private const string DefaultFileContent =
        @"<?xml version=""1.0""?>
<ClientSettings>
    <!-- Name of the REMOTE PC: type 'hostname' in a console on the remote PC. -->
    <PcName></PcName>
    <!-- IP of the REMOTE PC: type 'ipconfig' in a console on the remote PC. -->
    <Ip></Ip>
</ClientSettings>";

        // Lee el XML; si no existe lo crea vacío y con comentarios. Ante cualquier error devuelve unos
        // ajustes sin configurar, para que el plugin siga vivo.
        public static ClientSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ClientSettings));
                    using (FileStream stream = File.OpenRead(FilePath))
                        return (ClientSettings)serializer.Deserialize(stream);
                }

                Directory.CreateDirectory(FolderPath);
                File.WriteAllText(FilePath, DefaultFileContent);
                return new ClientSettings();
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ErrorLogger.Severity.Error, ex);
                return new ClientSettings();
            }
        }
    }
}