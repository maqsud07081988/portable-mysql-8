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

using PortableMySQL8.Themes;
using System.Windows.Threading;

namespace PortableMySQL8
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region MySQL Paths

        private static readonly string PathMySqlBase = "mysql";
        private static readonly string PathMySqlData = Path.Combine(PathMySqlBase, "data");
        private static readonly string PathMySqlConfig = Path.Combine(PathMySqlBase, "config");
        private static readonly string PathMyIniFile = Path.Combine(PathMySqlConfig, "my.ini");

        private static readonly string PathMySqlD = "\"" + Path.Combine(Environment.CurrentDirectory, PathMySqlBase, "bin", "mysqld.exe") + "\"";
        private static readonly string PathMySqlAdmin = "\"" + Path.Combine(Environment.CurrentDirectory, PathMySqlBase, "bin", "mysqladmin.exe") + "\"";

        #endregion MySQL Paths

        #region Process Monitor

        private DispatcherTimer ProcessCheckTimer = null;
        private const int ProcessCheckTimerInterval = 5000;

        private bool IsMySqlRunning
        {
            get { return ProcessHelpers.ProcessExists("mysqld"); }
        }

        private bool IsRobustRunning
        {
            get { return ProcessHelpers.ProcessExists("Robust") || ProcessHelpers.ProcessExists("Robust32"); }
        }

        private bool IsOpenSimRunning
        {
            get { return ProcessHelpers.ProcessExists("OpenSim") || ProcessHelpers.ProcessExists("OpenSim32"); }
        }

        #endregion Process Monitor

        #region Window

        private readonly WindowTheming WindowTheme = new WindowTheming();

        #endregion Window

        public MainWindow()
        {
            InitializeComponent();

            SettingsGlobal.LoadSettings();

            #region Setup

            WindowTheme.ApplyTheme("Blue", "Light");

            this.Title = $"{Version.NAME} {Version.VersionPretty}";

            this.Closing += MainWindow_Closing;
            this.ContentRendered += MainWindow_ContentRendered;

            #endregion Setup

            LoadUIConfig();

            bool success = CreateServiceFiles();

            if (!success)
            {
                MessageBox.Show("Could not create the required files to start!", "Error");
                Environment.Exit(0);
                return;
            }

            StartProcessCheckTimer();
        }

        #region Events

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsMySqlRunning)
            {
                e.Cancel = true;

                MessageBoxResult result = MessageBox.Show($"Closing {Version.NAME} will also shutdown MySQL\r\n\r\nAre you sure you want to do this?", "Confirm Close", MessageBoxButton.YesNo);

                //User did anything but click "Yes"
                if (result != MessageBoxResult.Yes)
                    return;

                if (!ConfirmStopMySqlWhileOpenSimRunning())
                    return;

                if (StopMySql() == 0)
                {

                    //Wait until mysqld exits
                    Console.WriteLine("Waiting until MySQL has stopped...");
                    System.Threading.SpinWait.SpinUntil(() => !IsMySqlRunning);
                    Console.WriteLine("MySQL stopped! Exiting...");

                    SaveUIConfig();
                    StopProcessCheckTimer();

                    Environment.Exit(0);
                }

                else
                    MessageBox.Show("Could not stop MySQL!");
            }

            else
            {
                e.Cancel = true;
                SaveUIConfig();
                Environment.Exit(0);
            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            //DoMySqlInitIfNeeded();
        }

        private void BtnStartSql_Click(object sender, RoutedEventArgs e)
        {
            if (IsMySqlRunning)
            {
                MessageBox.Show("MySQL is already running!", "Information");
                return;
            }

            bool needsInit = NeedsInit(GetStartParams());

            if (needsInit && String.IsNullOrWhiteSpace(passwordBoxMySqlRootPass.Password))
            {
                MessageBox.Show("MySQL needs to be initialized and can not start with no password set!", "Error");
                return;
            }

            SaveUIConfig();

            bool didInit = DoMySqlInitIfNeeded();

            bool updateIniSuccess = UpdateMyIni();

            if (!updateIniSuccess)
            {
                MessageBox.Show("Could not update my.ini! Aborting!", "Error");
                return;
            }

            StartMySql();

            if (didInit)
            {
                bool success = SetPassword(
                    "root", "localhost",
                    SettingsGlobal.Config.MySQL.Port.ToString(),
                    String.Empty, passwordBoxMySqlRootPass.Password);

                if (!success)
                {
                    MessageBox.Show("Was not able to set root password!", "Password Error");
                    return;
                }
            }
        }

        private void BtnStopSql_Click(object sender, RoutedEventArgs e)
        {
            if (!IsMySqlRunning)
            {
                MessageBox.Show("MySQL is not running!", "Information");
                return;
            }

            if (String.IsNullOrWhiteSpace(passwordBoxMySqlRootPass.Password))
            {
                MessageBox.Show("Can not stop with no password set!");
                return;
            }

            if (!ConfirmStopMySqlWhileOpenSimRunning())
                return;

            if (StopMySql() != 0)
                MessageBox.Show("Could not stop MySQL!");
        }

        private void CheckBoxSavePass_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxSavePass.IsChecked == true)
            {
                MessageBoxResult result = MessageBox.Show($"Your MySQL root user password will be stored in CLEAR TEXT in {Path.GetFileName(SettingsGlobal.SettingsFilePath)}!\r\n\r\nAre you sure you want to do this?", "Confirm Allow Save Password", MessageBoxButton.YesNo);

                //User did anything but click "Yes"
                if (result != MessageBoxResult.Yes)
                    checkBoxSavePass.IsChecked = false;
            }
        }

        #endregion Events

        #region Timers

        private void StartProcessCheckTimer()
        {
            StopProcessCheckTimer();

            if (ProcessCheckTimer == null)
            {
                ProcessCheckTimer = new DispatcherTimer();
                ProcessCheckTimer.Tick += ProcessCheckTimer_Tick;
            }

            ProcessCheckTimer.Interval = TimeSpan.FromMilliseconds(ProcessCheckTimerInterval);
            ProcessCheckTimer.Start();
        }

        private void StopProcessCheckTimer()
        {
            if (ProcessCheckTimer != null)
                ProcessCheckTimer.Stop();
        }

        private void ProcessCheckTimer_Tick(object sender, EventArgs e)
        {
            if (IsMySqlRunning)
            {
                labelMySqlStatus.Content = "MySQL is running";
                labelMySqlStatus.Foreground = Brushes.Green;
            }

            else
            {
                labelMySqlStatus.Content = "MySQL is not running";
                labelMySqlStatus.Foreground = Brushes.Red;
            }

            if (IsRobustRunning)
            {
                labelRobustStatus.Content = "ROBUST is running";
                labelRobustStatus.Foreground = Brushes.Green;
            }

            else
            {
                labelRobustStatus.Content = "ROBUST is not running";
                labelRobustStatus.Foreground = Brushes.Red;
            }

            if (IsOpenSimRunning)
            {
                labelOpenSimStatus.Content = "OpenSim is running";
                labelOpenSimStatus.Foreground = Brushes.Green;
            }

            else
            {
                labelOpenSimStatus.Content = "OpenSim is not running";
                labelOpenSimStatus.Foreground = Brushes.Red;
            }
        }

        #endregion Timers

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

        private bool UpdateMyIni()
        {
            try
            {
                Console.WriteLine($"Writing port config to {PathMyIniFile}...");
                IniFile.WriteValue("client", "port", SettingsGlobal.Config.MySQL.Port.ToString(), PathMyIniFile);
                IniFile.WriteValue("mysqld", "port", SettingsGlobal.Config.MySQL.Port.ToString(), PathMyIniFile);

                Console.WriteLine($"Updating basedir and datadir in {PathMyIniFile}...");
                IniFile.WriteValue("mysqld", "basedir", "\"" + Path.GetFullPath(PathMySqlBase) + "\"", PathMyIniFile);
                IniFile.WriteValue("mysqld", "datadir", "\"" + Path.GetFullPath(PathMySqlData) + "\"", PathMyIniFile);

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        private bool SetPassword(string user, string server, string port, string curPass, string newPass)
        {
            bool success;
            string connectString = $"server={server};user={user};database=mysql;port={port};password={curPass}";
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

        private int StopMySql()
        {
            string prams = $"-u root -p{passwordBoxMySqlRootPass.Password} --port {SettingsGlobal.Config.MySQL.Port} shutdown";
            int code = ProcessHelpers.RunCommand(PathMySqlAdmin, prams, true);

            if (code == 0)
                Console.WriteLine("Stopped MySQL");

            else
                Console.WriteLine($"Could not stop MySQL! Command returned code {code}.");

            return code;
        }

        private bool ConfirmStopMySqlWhileOpenSimRunning()
        {
            if (IsRobustRunning || IsOpenSimRunning)
            {
                MessageBoxResult result = MessageBox.Show("OpenSim and/or ROBUST seems to still be running. You should shut them down first before shutting MySQL down.\r\n\r\nAre you sure you want to continue with MySQL shutdown?", "Confirm Shutdown", MessageBoxButton.YesNo);

                //User did anything but click "Yes")
                if (result != MessageBoxResult.Yes)
                    return false;
            }

            return true;
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

        private void LoadUIConfig()
        {
            this.Width = SettingsGlobal.Config.General.WindowSize.Width;
            this.Height = SettingsGlobal.Config.General.WindowSize.Height;

            //Start in the center of the screen if our location is 0
            if (SettingsGlobal.Config.General.WindowLocation.X == 0
            && SettingsGlobal.Config.General.WindowLocation.Y == 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            //Else put the window where it was last
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = SettingsGlobal.Config.General.WindowLocation.X;
                this.Top = SettingsGlobal.Config.General.WindowLocation.Y;
            }

            passwordBoxMySqlRootPass.Password = SettingsGlobal.Config.MySQL.RootPass;
            checkBoxSavePass.IsChecked = SettingsGlobal.Config.MySQL.SavePass;
            nudPort.Value = SettingsGlobal.Config.MySQL.Port;
        }

        private void SaveUIConfig()
        {
            SettingsGlobal.Config.General.WindowSize = new Size(this.Width, this.Height);
            SettingsGlobal.Config.General.WindowLocation = new Point(this.Left, this.Top);

            if (checkBoxSavePass.IsChecked == true)
                SettingsGlobal.Config.MySQL.RootPass = passwordBoxMySqlRootPass.Password;

            else
                SettingsGlobal.Config.MySQL.RootPass = String.Empty;

            SettingsGlobal.Config.MySQL.SavePass = checkBoxSavePass.IsChecked.TranslateNullableBool();
            SettingsGlobal.Config.MySQL.Port = (int)nudPort.Value;

            SettingsGlobal.SaveSettings();
        }

        #endregion Methods
    }
}
