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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace PortableMySQL8
{
    /// <summary>
    /// Interaction logic for TabDatabase.xaml
    /// </summary>
    public partial class TabDatabase : UserControl
    {
        private MainWindow Instance = null;
        private Settings Config = null;

        public TabDatabase(MainWindow _instance, Settings _config)
        {
            InitializeComponent();

            Instance = _instance;
            Config = _config;

            LoadUIConfig();
        }

        #region Events

        #region Login Info

        private void textBoxDbUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.Database.DatabaseUser = textBoxDbUser.Text.Trim();
        }

        private void textBoxDbServer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.Database.DatabaseServer = textBoxDbServer.Text.Trim();
        }

        private void passWordBoxDbUserPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Config.Database.DatabasePassword = passWordBoxDbUserPass.Password.Trim();
        }

        private void checkBoxSaveLoginInfo_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxSaveLoginInfo.IsChecked == true)
            {
                MessageBoxResult result = MessageBox.Show($"Your database login info (user name, server name, and user password) will be stored in CLEAR TEXT in {Path.GetFileName(Instance.PathConfig)}!\r\n\r\nPlease note that these details have to be stored in clear text in OpenSim's various .ini config files anyway, so this may be a non-issue for some. If this is an issue for you, or you are unsure, click 'No'.\r\n\r\nAre you sure you want to do this?", "Confirm Allow Save Login Info", MessageBoxButton.YesNo);

                //User did anything but click "Yes"
                if (result != MessageBoxResult.Yes)
                    checkBoxSaveLoginInfo.IsChecked = false;
            }

            Config.Database.SaveLoginInfo = checkBoxSaveLoginInfo.IsChecked.TranslateNullableBool();
        }

        #endregion Login Info

        #region Database Details

        private void textBoxMainName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.Database.DatabaseMain = textBoxMainName.Text.Trim();
        }

        private void textBoxProfilesName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.Database.DatabaseProfiles = textBoxProfilesName.Text.Trim();
        }

        private void textBoxGroupsName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.Database.DatabaseGroups = textBoxGroupsName.Text.Trim();
        }

        #endregion Database Details

        #region Buttons

        private void btnCreateDb_Click(object sender, RoutedEventArgs e)
        {
            if (!ProcessHelpers.ProcessExists("mysqld"))
            {
                MessageBox.Show("Can't create database(s) because MySQL is not running", "Error");
                return;
            }

            string user = "root";
            string server = "localhost";

            if (!String.IsNullOrWhiteSpace(Config.Database.DatabaseUser))
                user = Config.Database.DatabaseUser;

            if (!string.IsNullOrWhiteSpace(Config.Database.DatabaseServer))
                server = Config.Database.DatabaseServer;

            string creationStatus = String.Empty;

            List<string> databases = new List<string>() { Config.Database.DatabaseMain, Config.Database.DatabaseProfiles, Config.Database.DatabaseGroups };

            foreach (string db in databases)
            {
                if (String.IsNullOrWhiteSpace(db))
                    continue;

                bool success = SQLTools.CreateDatabaseIfNotExists(
                    user, server, (int)Instance.nudPort.Value, Config.Database.DatabasePassword, db);

                if (!success)
                    Console.WriteLine($"Could not create database '{db}'");

                creationStatus += CreateStatusString(db, success) + "\r\n";
            }

            //Results
            MessageBox.Show(creationStatus, "Database Creation Results");
        }

        #endregion Buttons

        #endregion Events

        #region Methods

        private void LoadUIConfig()
        {
            textBoxDbUser.Text = Config.Database.DatabaseUser;
            textBoxDbServer.Text = Config.Database.DatabaseServer;
            passWordBoxDbUserPass.Password = Config.Database.DatabasePassword;
            checkBoxSaveLoginInfo.IsChecked = Config.Database.SaveLoginInfo;

            textBoxMainName.Text = Config.Database.DatabaseMain;
            textBoxProfilesName.Text = Config.Database.DatabaseProfiles;
            textBoxGroupsName.Text = Config.Database.DatabaseGroups;
        }

        private string CreateStatusString(string name, bool success)
        {
            string ret = String.Empty;

            ret += $"'{name}'";

            if (success)
                ret += ": Successfully created!";

            else
                ret += ": Creation failed! (Perhaps it already exists?)";

            return ret;
        }

        #endregion Methods
    }
}
