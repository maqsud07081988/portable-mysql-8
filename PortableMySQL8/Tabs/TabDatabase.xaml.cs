using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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

        private void btnCreateDb_Click(object sender, RoutedEventArgs e)
        {
            if (!ProcessHelpers.ProcessExists("mysqld"))
            {
                MessageBox.Show("Can't create database(s) because MySQL is not running", "Error");
                return;
            }

            string creationStatus = String.Empty;

            List<string> databases = new List<string>() { Config.DatabaseMain, Config.DatabaseProfiles, Config.DatabaseGroups };

            foreach (string db in databases)
            {
                bool success = SQLTools.CreateDatabaseIfNotExists(
                    "root", "localhost", Instance.nudPort.Value.ToString(), Instance.passwordBoxMySqlRootPass.Password, db);

                if (!success)
                    Console.WriteLine($"Could not create database '{db}'");

                creationStatus += CreateStatusString(db, success) + "\r\n";
            }

            //Results
            MessageBox.Show(creationStatus, "Database Creation Results");
        }

        #endregion Events

        #region Methods

        private void LoadUIConfig()
        {
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
