using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FanControl.RemoteSensors
{
    // Ajustes del plugin: nombre del PC y/o IP del PC donde corre FanControl.DataServer, puerto en el
    // que escucha y contraseña. Se guardan en un XML en %LocalAppData%\FanControl (el mismo que escribe
    // la pestaña RemoteSensors de WatchTower); si no existe se crea con los valores por defecto.
    public class ClientSettings
    {
        private const int DefaultPort = 8765;
        private const int MinPort = 1;
        private const int MaxPort = 65535;

        // La misma contraseña por defecto que usa DataServer, para que sin tocar nada coincidan.
        private const string DefaultPassword = "Password1234";
        private const string PasswordComment = " Password set in DataServer on the REMOTE PC. It must be the same. ";

        public string PcName { get; set; } = "";
        public string Ip { get; set; } = "";

        // Texto tal como está en el XML. Vacío o inválido significa usar el 8765.
        public string Port { get; set; } = "";

        // Texto tal como está en el XML. Vacío o ausente significa usar la de por defecto.
        public string Password { get; set; } = "";

        public static string FolderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FanControl");

        public static string FilePath => Path.Combine(FolderPath, "RemoteSensorsConfig.xml");

        // Basta con que esté rellenado uno de los dos.
        public bool IsConfigured => !string.IsNullOrWhiteSpace(Ip) || !string.IsNullOrWhiteSpace(PcName);

        // Puerto efectivo: el del XML si es válido y, si no, el de por defecto.
        public int PortNumber
        {
            get
            {
                int port;
                return TryParsePort(Port, out port) ? port : DefaultPort;
            }
        }

        // Contraseña efectiva: la del XML o, si no hay, la de por defecto.
        public string EffectivePassword =>
            string.IsNullOrWhiteSpace(Password) ? DefaultPassword : Password.Trim();

        // Provisional: usa la IP si está rellena y, si no, el nombre del PC. Cuando decidas el orden
        // de prueba cuando están los dos, esta propiedad se sustituye por esa lógica.
        public string BaseUrl =>
            "http://" + (string.IsNullOrWhiteSpace(Ip) ? PcName : Ip).Trim() + ":" + PortNumber + "/";

        // Lee el XML; si no existe lo crea con los valores por defecto y comentarios. Los XML de
        // versiones anteriores (sin Port ni Password) siguen valiendo. Ante cualquier error devuelve
        // unos ajustes sin configurar, para que el plugin siga vivo.
        public static ClientSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    ClientSettings settings;
                    XmlSerializer serializer = new XmlSerializer(typeof(ClientSettings));
                    using (FileStream stream = File.OpenRead(FilePath))
                        settings = (ClientSettings)serializer.Deserialize(stream);

                    int port;
                    if (!string.IsNullOrWhiteSpace(settings.Port) && !TryParsePort(settings.Port, out port))
                        ErrorLogger.Write(ErrorLogger.Severity.Warning,
                            "Invalid port '" + settings.Port + "' in " + FilePath + "; using " + DefaultPort + ".");

                    return settings;
                }

                Directory.CreateDirectory(FolderPath);
                CreateDefaultDocument().Save(FilePath);
                return new ClientSettings();
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ErrorLogger.Severity.Error, ex);
                return new ClientSettings();
            }
        }

        // Documento que se crea la primera vez: igual que el que guarda WatchTower, con los campos
        // vacíos salvo la contraseña por defecto.
        private static XDocument CreateDefaultDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("ClientSettings",
                    new XComment(" Name of the REMOTE PC: type 'hostname' in a console on the remote PC. "),
                    new XElement("PcName", ""),
                    new XComment(" IP of the REMOTE PC: type 'ipconfig' in a console on the remote PC. "),
                    new XElement("Ip", ""),
                    new XComment(" Port DataServer listens on at the REMOTE PC. Leave empty to use 8765. "),
                    new XElement("Port", ""),
                    new XComment(PasswordComment),
                    new XElement("Password", DefaultPassword)));
        }

        // True si el texto es un número entero entre MinPort y MaxPort (solo dígitos).
        private static bool TryParsePort(string text, out int port)
        {
            return int.TryParse((text ?? "").Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out port)
                   && port >= MinPort && port <= MaxPort;
        }
    }
}