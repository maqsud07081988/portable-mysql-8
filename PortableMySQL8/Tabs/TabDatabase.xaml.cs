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
        }

        private void btnCreateDb_Click(object sender, RoutedEventArgs e)
        {
            string user = "root";
            string server = "localhost";
            string pass = Instance.passwordBoxMySqlRootPass.Password;
            string port = Instance.nudPort.Value.ToString();

            string creationStatus = String.Empty;

            bool success = CreateDatabaseIfNotExists(user, server, port, pass, textBoxOpenSimName.Text);

            //Main DB
            if (!success)
                Console.WriteLine($"Could not create database '{textBoxOpenSimName.Text}'");

            creationStatus += CreateStatusString(textBoxOpenSimName.Text, success) + "\r\n";

            //Profiles DB
            success = CreateDatabaseIfNotExists(user, server, port, pass, textBoxProfilesName.Text);

            if (!success)
                Console.WriteLine($"Could not create database '{textBoxProfilesName.Text}'");

            creationStatus += CreateStatusString(textBoxProfilesName.Text, success) + "\r\n";

            //Groups DB
            success = CreateDatabaseIfNotExists(user, server, port, pass, textBoxGroupsName.Text);

            if (!success)
                Console.WriteLine($"Could not create database '{textBoxGroupsName.Text}'");

            creationStatus += CreateStatusString(textBoxGroupsName.Text, success);

            pass = null;

            //Results
            MessageBox.Show(creationStatus, "Database Creation Results");
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

        private bool CreateDatabaseIfNotExists(string user, string server, string port, string password, string dbName)
        {
            if (String.IsNullOrWhiteSpace(dbName))
                return false;

            bool? exists = SQLTools.DatabaseExists(user, server, port, password, dbName);

            if (exists != null && exists == false)
                return SQLTools.CreateDatabase(user, server, port, password, dbName);

            return false;
        }
    }
}
