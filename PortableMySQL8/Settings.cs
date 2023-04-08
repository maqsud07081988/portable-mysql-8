using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PortableMySQL8
{
    public class SettingsHelper
    {
        private void CreateSettingsIfNotExists(string path, Settings settings)
        {
            if (!File.Exists(path))
            {
                try
                {
                    if (settings == null)
                        settings = new Settings();

                    SaveSettings(path, settings);
                }

                catch { }
            }
        }

        public Settings LoadSettings(string path, Settings settings)
        {
            try
            {
                CreateSettingsIfNotExists(path, settings);

                if (File.Exists(path))
                    settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));

                return settings;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public bool SaveSettings(string path, Settings settings)
        {
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
                return true;
            }

            catch
            {
                return false;
            }
        }
    }

    public class Settings
    {
        public SettingsGeneral General { get; set; } = new SettingsGeneral();
        public SettingsMySQL MySQL { get; set; } = new SettingsMySQL();
    }

    public class SettingsGeneral
    {
        public Size WindowSize { get; set; } = new Size(515, 200);
        public Point WindowLocation { get; set; } = new Point();
    }

    public class SettingsMySQL
    {
        public string RootPass { get; set; } = String.Empty;
        public bool SavePass { get; set; } = false;
        public int Port { get; set; } = 3306;
    }
}
