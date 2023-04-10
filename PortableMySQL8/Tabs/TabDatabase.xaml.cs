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
        private SettingsTabDatabase Config = null;

        public TabDatabase(MainWindow _instance, SettingsTabDatabase _config)
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
            Config.DatabaseUser = textBoxDbUser.Text.Trim();
        }

        private void textBoxDbServer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.DatabaseServer = textBoxDbServer.Text.Trim();
        }

        private void passWordBoxDbUserPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Config.DatabasePassword = passWordBoxDbUserPass.Password.Trim();
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

            Config.SaveLoginInfo = checkBoxSaveLoginInfo.IsChecked.TranslateNullableBool();
        }

        #endregion Login Info

        #region Database Details

        private void textBoxMainName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.DatabaseMain = textBoxMainName.Text.Trim();
        }

        private void textBoxProfilesName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.DatabaseProfiles = textBoxProfilesName.Text.Trim();
        }

        private void textBoxGroupsName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Config.DatabaseGroups = textBoxGroupsName.Text.Trim();
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

            if (!String.IsNullOrWhiteSpace(Config.DatabaseUser))
                user = Config.DatabaseUser;

            if (!string.IsNullOrWhiteSpace(Config.DatabaseServer))
                server = Config.DatabaseServer;

            string creationStatus = String.Empty;

            List<string> databases = new List<string>() { Config.DatabaseMain, Config.DatabaseProfiles, Config.DatabaseGroups };

            foreach (string db in databases)
            {
                bool success = SQLTools.CreateDatabaseIfNotExists(
                    user, server, (int)Instance.nudPort.Value, Config.DatabasePassword, db);

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
            textBoxDbUser.Text = Config.DatabaseUser;
            textBoxDbServer.Text = Config.DatabaseServer;
            passWordBoxDbUserPass.Password = Config.DatabasePassword;
            checkBoxSaveLoginInfo.IsChecked = Config.SaveLoginInfo;

            textBoxMainName.Text = Config.DatabaseMain;
            textBoxProfilesName.Text = Config.DatabaseProfiles;
            textBoxGroupsName.Text = Config.DatabaseGroups;
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
