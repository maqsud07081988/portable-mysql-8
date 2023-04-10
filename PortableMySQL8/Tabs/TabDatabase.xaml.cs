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

            string user = "root";
            string server = "localhost";
            string pass = Instance.passwordBoxMySqlRootPass.Password;
            string port = Instance.nudPort.Value.ToString();

            string creationStatus = String.Empty;

            bool success = SQLTools.CreateDatabaseIfNotExists(user, server, port, pass, Config.DatabaseMain);

            //Main DB
            if (!success)
                Console.WriteLine($"Could not create database '{Config.DatabaseMain}'");

            creationStatus += CreateStatusString(Config.DatabaseMain, success) + "\r\n";

            //Profiles DB
            success = SQLTools.CreateDatabaseIfNotExists(user, server, port, pass, Config.DatabaseProfiles);

            if (!success)
                Console.WriteLine($"Could not create database '{Config.DatabaseProfiles}'");

            creationStatus += CreateStatusString(Config.DatabaseProfiles, success) + "\r\n";

            //Groups DB
            success = SQLTools.CreateDatabaseIfNotExists(user, server, port, pass, Config.DatabaseGroups);

            if (!success)
                Console.WriteLine($"Could not create database '{Config.DatabaseGroups}'");

            creationStatus += CreateStatusString(Config.DatabaseGroups, success);

            pass = null;

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
