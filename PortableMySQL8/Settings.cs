#region License

/*

Copyright 2023 mewtwo0641

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

#endregion License

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
        public SettingsTabDatabase Database { get; set; } = new SettingsTabDatabase();
    }

    public class SettingsGeneral
    {
        public Point WindowLocation { get; set; } = new Point();
        public bool AgreedToLicense { get; set; } = false;
    }

    public class SettingsMySQL
    {
        public string RootPass { get; set; } = String.Empty;
        public bool SavePass { get; set; } = false;
        public int Port { get; set; } = 3306;
    }

    public class SettingsTabDatabase
    {
        public string LoginUser { get; set; } = String.Empty;
        public string LoginServer { get; set; } = String.Empty;
        public string LoginPassword { get; set; } = String.Empty;
        public bool SaveLoginInfo { get; set; } = false;

        public string OSMain { get; set; } = String.Empty;
        public string OSProfiles { get; set; } = String.Empty;
        public string OSGroups { get; set; } = String.Empty;
    }
}
