using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Sean.WorldClient.Hosts;
using Sean.WorldClient.Hosts.Input;
using Sean.WorldClient.Hosts.Ui;
using Sean.WorldClient.Hosts.World;

namespace Sean.WorldClient
{
    /// <summary>
    /// Use Settings class for values that can be altered but not saved.
    /// Use Config class for values that can be altered and saved.
    /// Use Constants class for values that cannot be altered.
    /// </summary>
    /// <remarks>
    /// For adding new types in the xsd, use this table to match the .net type to the type in the xsd file: http://msdn.microsoft.com/en-us/library/aa719879%28v=vs.71%29.aspx
    /// </remarks>
    internal static class Config
    {
        #region Properties (Saved)
        internal static string UserName;
        internal static string Server;
        internal static ushort Port;
        internal static bool SoundEnabled;
        internal static bool MusicEnabled;
        internal static bool Windowed;
        internal static bool Maximized;
        internal static bool InvertMouse;
        internal static bool VSync;
        internal static bool Mipmapping;
        internal static bool Fog;
        internal static bool LinearMagnificationFilter;
        internal static bool SmoothLighting;
        // ReSharper disable InconsistentNaming
        internal static string MOTD;
        // ReSharper restore InconsistentNaming

        #endregion

        #region Properties (Static)
        private const int TCP_LISTENER_PORT = 8080;
        private static string _configFilePath;
        private static XmlDocument _configXml;
        internal static DirectoryInfo AppDirectory;
        internal static DirectoryInfo SaveDirectory;
        #endregion

        #region Operations
        internal static void Load()
        {
            try
            {
                AppDirectory = new DirectoryInfo(Application.StartupPath);
                if (AppDirectory == null) throw new Exception(string.Format("Failed to retrieve app directory info for: {0}", Application.StartupPath));
                _configFilePath = Path.Combine(AppDirectory.FullName, "Config.xml"); //use Path.Combine to play nice with linux
                _configXml = new XmlDocument();

                if (File.Exists(_configFilePath)) //config file exists, load it
                {
                    _configXml.Load(_configFilePath);
                }
                else //no config file, use defaults
                {
                    _configXml.LoadXml("<?xml version=\"1.0\" ?><Config><UserName>Test</UserName><Server>127.0.0.1</Server><Port>8084</Port></Config>");
                }
                _configXml.Schemas.Add("", XmlReader.Create(new StringReader(Properties.Resources.Config)));
                _configXml.Validate(null);

                UserName = LoadSetting("UserName");
                Server = LoadSetting("Server");
                Port = LoadSetting("Port", TCP_LISTENER_PORT);

                Windowed = LoadSetting("Windowed", true);
                Maximized = LoadSetting("Maximized", true);
                InvertMouse = LoadSetting("InvertMouse", false);
                VSync = LoadSetting("VSync", true);
                Mipmapping = LoadSetting("Mipmapping", true);
                Fog = LoadSetting("Fog", true);
                LinearMagnificationFilter = LoadSetting("LinearMagnificationFilter", false);
                SmoothLighting = LoadSetting("SmoothLighting", true);
                MOTD = LoadSetting("MOTD");

                SoundEnabled = LoadSetting("SoundEnabled", true);
                MusicEnabled = LoadSetting("MusicEnabled", true);

                const string SAVE_FILE_FOLDER_NAME = "SaveFiles";
                SaveDirectory = new DirectoryInfo(Path.Combine(AppDirectory.FullName, SAVE_FILE_FOLDER_NAME));
                if (!SaveDirectory.Exists) SaveDirectory = AppDirectory.CreateSubdirectory(SAVE_FILE_FOLDER_NAME);

                //set version here so the game window has access to it and so the server also loads it when starting in a new process
                Settings.Version = new Version(Application.ProductVersion);
            }
            catch (Exception ex)
            {
                Utilities.Misc.MessageError(string.Format("Error loading config, if the problem persists, try removing your Config.xml file.\n\n{0}", ex.GetBaseException().Message));
                Application.Exit(); //weird things can happen if config doesnt load properly so just exit, the client gets a nice enough message that they should be able to figure out the problem
            }
        }

        private static string LoadSetting(string name, string defaultValue = "")
        {
            var node = _configXml.SelectSingleNode("//" + name);
            return node != null ? node.InnerText : defaultValue;
        }

        private static bool LoadSetting(string name, bool defaultValue)
        {
            var node = _configXml.SelectSingleNode("//" + name);
            return node != null ? XmlConvert.ToBoolean(node.InnerText) : defaultValue;
        }

        private static ushort LoadSetting(string name, ushort defaultValue)
        {
            var node = _configXml.SelectSingleNode("//" + name);
            return node != null ? XmlConvert.ToUInt16(node.InnerText) : defaultValue;
        }

        internal static void Save()
        {
            try
            {
                SaveSetting("UserName", UserName);
                SaveSetting("Server", Server);
                SaveSetting("Port", Port.ToString());
                SaveSetting("Windowed", Windowed);
                SaveSetting("Maximized", Maximized);
                SaveSetting("InvertMouse", InvertMouse);
                SaveSetting("VSync", VSync);
                SaveSetting("Mipmapping", Mipmapping);
                SaveSetting("Fog", Fog);
                SaveSetting("LinearMagnificationFilter", LinearMagnificationFilter);
                SaveSetting("SmoothLighting", SmoothLighting);
                SaveSetting("MOTD", MOTD);
                SaveSetting("SoundEnabled", SoundEnabled);
                SaveSetting("MusicEnabled", MusicEnabled);

                _configXml.Save(_configFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving config: " + ex.Message);
            }
        }

        private static void SaveSetting(string name, string value)
        {
            // ReSharper disable PossibleNullReferenceException
            var node = _configXml.SelectSingleNode("//" + name) ?? _configXml.DocumentElement.AppendChild(_configXml.CreateNode(XmlNodeType.Element, name, ""));
            // ReSharper restore PossibleNullReferenceException
            node.InnerText = value;
        }

        private static void SaveSetting(string name, bool value)
        {
            // ReSharper disable PossibleNullReferenceException
            var node = _configXml.SelectSingleNode("//" + name) ?? _configXml.DocumentElement.AppendChild(_configXml.CreateNode(XmlNodeType.Element, name, ""));
            // ReSharper restore PossibleNullReferenceException
            node.InnerText = XmlConvert.ToString(value);
        }
        #endregion
    }
}
