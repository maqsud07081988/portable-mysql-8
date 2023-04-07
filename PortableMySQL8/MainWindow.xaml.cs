using System;
using System.Collections.Generic;
using System.Data;
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
using System.IO;

using MySql.Data.MySqlClient;

namespace PortableMySQL8
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string PathMySqlBase = "mysql";
        private static string PathMySqlData = Path.Combine(PathMySqlBase, "data");
        private static string PathMySqlConfig = Path.Combine(PathMySqlBase, "config");
        private static string PathMyIniFile = Path.Combine(PathMySqlConfig, "my.ini");

        private static string PathMySqlD = "\"" + Path.Combine(Environment.CurrentDirectory, PathMySqlBase, "bin", "mysqld.exe") + "\"";
        private static string PathMySqlAdmin = "\"" + Path.Combine(Environment.CurrentDirectory, PathMySqlBase, "bin", "mysqladmin.exe") + "\"";

        public MainWindow()
        {
            InitializeComponent();

            #region Setup

            this.Title = $"{Version.NAME} {Version.VersionPretty}";

            this.Closing += MainWindow_Closing;
            this.ContentRendered += MainWindow_ContentRendered;

            #endregion Setup

            bool success = CreateServiceFiles();

            if (!success)
            {
                MessageBox.Show("Could not create the required files to start!", "Error");
                Environment.Exit(0);
                return;
            }
        }

        #region Events

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string proc = "mysqld";

            if (ProcessHelpers.ProcessExists(proc))
            {
                e.Cancel = true;

                MessageBoxResult result = MessageBox.Show($"Closing {Version.NAME} will also shutdown MySQL\r\n\r\nAre you sure you want to do this?", "Confirm Close", MessageBoxButton.YesNo);

                //User did anything but click "Yes"
                if (result != MessageBoxResult.Yes)
                    return;

                StopMySql();

                //Wait until mysqld exits
                Console.WriteLine("Waiting until MySQL has stopped...");
                System.Threading.SpinWait.SpinUntil(() => !ProcessHelpers.ProcessExists(proc));
                Console.WriteLine("MySQL stopped! Exiting...");

                Environment.Exit(0);
            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            //DoMySqlInitIfNeeded();
        }

        private void BtnStartSql_Click(object sender, RoutedEventArgs e)
        {
            bool needsInit = NeedsInit(GetStartParams());

            if (needsInit && String.IsNullOrWhiteSpace(passwordBoxMySqlRootPass.Password))
            {
                MessageBox.Show("MySQL needs to be initialized and can not start with no password set!", "Error");
                return;
            }

            bool didInit = DoMySqlInitIfNeeded();

            StartMySql();

            if (didInit)
            {
                bool success = SetPassword("root", "localhost", String.Empty, passwordBoxMySqlRootPass.Password);

                if (!success)
                {
                    MessageBox.Show("Was not able to set root password!", "Password Error");
                    return;
                }
            }
        }

        private void BtnStopSql_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(passwordBoxMySqlRootPass.Password))
            {
                MessageBox.Show("Can not stop with no password set!");
                return;
            }

            StopMySql();
        }

        #endregion Events

        #region Methods

        private bool CreateServiceFiles()
        {
            try
            {
                if (!Directory.Exists(PathMySqlBase))
                    Directory.CreateDirectory(PathMySqlBase);

                //Don't create the data directory here, MySQL does that on initialize

                if (!Directory.Exists(PathMySqlConfig))
                    Directory.CreateDirectory(PathMySqlConfig);

                if (!File.Exists(PathMyIniFile))
                {
                    //Set up default my.ini file contents
                    //I'm sure there's probably a better way to do this but this is simple enough for now.
                    List<string> myIni = new List<string>()
                    {
                        "[client]",
                        "",
                        "port=3306",
                        "",
                        "[mysqld]",
                        "",
                        "# The TCP/IP Port the MySQL Server will listen on",
                        "port=3306",
                        "",
                        "#Path to installation directory. All paths are usually resolved relative to this.",
                        "basedir=" + "\"" + Path.GetFullPath(PathMySqlBase) + "\"",
                        "",
                        "#Path to the database root",
                        "datadir=" + "\"" + Path.GetFullPath(PathMySqlData) + "\"",
                        "",
                        "#OpenSim needs this on MySQL 8.0.4+",
                        "default-authentication-plugin=mysql_native_password",
                        "",
                        "#Max packetlength to send/receive from to server.",
                        "#MySQL's default seems to be 1 MB but OpenSim needs more than that",
                        "max_allowed_packet=128M"
                    };

                    //Dump the new my.ini file to the proper location
                    File.WriteAllLines(PathMyIniFile, myIni);
                }

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private bool DoMySqlInitIfNeeded()
        {
            try
            {
                if (Directory.Exists(PathMySqlData))
                {
                    List<string> files = Directory.EnumerateFiles(PathMySqlData, "*", SearchOption.AllDirectories).ToList();

                    if (files.Count <= 0)
                    {
                        MessageBoxResult result = MessageBox.Show($"The MySQL data directory at '{PathMySqlData}' exists but appears to contain no files in it. This directory will need to be DELETED in order for MySQL to sucessfully be initialized.\r\n\r\nAre you SURE you want to do this?", "Warning", MessageBoxButton.YesNo);

                        //User did anything except click "Yes"; stop here
                        if (result != MessageBoxResult.Yes)
                            return false;

                        Directory.Delete(PathMySqlData, true);
                    }
                }

                string prams = GetStartParams();
                bool needsInit = NeedsInit(prams);

                Console.WriteLine($"{PathMySqlD} {prams} Needs Init = {needsInit}");
                Console.WriteLine();

                if (needsInit)
                {
                    Console.WriteLine("Initializing MySQL data directory...");
                    ProcessHelpers.RunCommand(PathMySqlD, prams, true);
                    Console.WriteLine("Initialization done!");
                    return true;
                }

                else
                {
                    Console.WriteLine("MySQL data directory exists with files in it. No need to init.");
                    return false;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private bool SetPassword(string user, string server, string curPass, string newPass)
        {
            bool success = false;
            string connectString = $"server={server};user={user};database=mysql;password={curPass}";
            MySqlConnection connection = new MySqlConnection(connectString);

            try
            {
                connection.Open();

                string sql = $"alter user '{user}'@'{server}' identified with mysql_native_password by '{newPass}'; flush privileges;";
                MySqlCommand myCmd = new MySqlCommand(sql, connection);

                int rows = myCmd.ExecuteNonQuery();

                //Console.WriteLine($"Password set to '{pass}', {rows} rows affected");
                Console.Write("Password set sucessfully");

                success = true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                success = false;
            }

            connection.Close();
            return success;
        }

        private void StartMySql()
        {
            string prams = GetStartParams();
            bool needsInit = NeedsInit(prams);

            Console.WriteLine($"{PathMySqlD} {prams} Needs Init = {needsInit}");
            Console.WriteLine();

            if (!needsInit)
            {
                Console.WriteLine($"Started MySQL");
                ProcessHelpers.RunCommand(PathMySqlD, prams, false);
            }

            else
                Console.WriteLine("Could not start MySQL because it needs initialization.");
        }

        private void StopMySql()
        {
            string prams = $"-u root -p{passwordBoxMySqlRootPass.Password} shutdown";
            ProcessHelpers.RunCommand(PathMySqlAdmin, prams, true);
            Console.WriteLine("Stopped MySQL");
        }

        private bool NeedsInit(string prams)
        {
            return prams.Contains("--initialize");
        }

        private string GetStartParams()
        {
            string prams = "--defaults-file=" + "\"" + Path.Combine(Environment.CurrentDirectory, PathMyIniFile) + "\" --standalone --explicit_defaults_for_timestamp";

            //No MySQL data directory found, let's initialize it.
            //Doing an insecure initialization because we will set
            //a password for it immediately after.
            if (!Directory.Exists(PathMySqlData))
                prams += " --initialize-insecure";

            return prams;
        }

        #endregion Methods
    }
}
