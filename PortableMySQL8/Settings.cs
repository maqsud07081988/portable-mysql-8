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
    public class SettingsGlobal
    {
        public static Settings Config = new Settings();

        private static string SettingsFilePath = $"Config{Version.NAME}.json".Replace(" ", "_");

		#region Load/Save Settings

		private static void CreateSettingsIfNotExists()
		{
			if (!File.Exists(SettingsFilePath))
			{
				try
				{
					if (Config == null)
						Config = new Settings();

					SaveSettings();
				}

				catch { }
			}
		}

		public static void LoadSettings()
		{
			try
			{
				CreateSettingsIfNotExists();

				if (File.Exists(SettingsFilePath))
					Config = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFilePath));
			}

			catch (Exception ex)
			{
				//Malformed settings detected; end ourself
				MessageBox.Show("There was a problem loading settings: The file is possibly malformed. To fix it you can attempt to repair " + Path.GetFileName(SettingsFilePath) + " by hand or delete it and let the program recreate it.\r\n\r\nException:\r\n\r\n" + ex.ToString());
				Environment.Exit(0);
			}
		}

		public static bool SaveSettings()
		{
			try
			{
				File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(Config, Formatting.Indented));
				return true;
			}

			catch
			{
				return false;
			}
		}

		#endregion Load/Save Settings
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
		public int Port { get; set; } = 3306;
	}
}
