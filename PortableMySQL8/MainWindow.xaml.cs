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

        private static readonly string PathMySqlLauncher = Path.Combine(Environment.CurrentDirectory, "MySQLLauncher.exe");

        #endregion MySQL Paths

        #region Process Monitor

        private DispatcherTimer ProcessCheckTimer = null;
        private const int ProcessCheckTimerInterval = 5000;
        private const int ProcessCheckTimerIntervalFast = 500;

        private bool MySQLIsStarting = false;
        private bool MySQLIsStopping = false;

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

        #region Settings

        private readonly SettingsHelper SettingsHelper = new SettingsHelper();
        private static Settings Config = new Settings();
        public readonly string PathConfig = $"Config.json";

        #endregion Settings

        public MainWindow()
        {
            InitializeComponent();

            #region Window Setup

            WindowTheme.ApplyTheme("Blue", "Light");

            this.Title = $"{Version.NAME} {Version.VersionPretty}";

            this.Closing += MainWindow_Closing;
            this.ContentRendered += MainWindow_ContentRendered;

            #endregion Window Setup

            #region Config Loading

            //Load the configuration file
            Config = SettingsHelper.LoadSettings(PathConfig, Config);

            //Got bad config; stop here
            if (Config == null)
            {
                //Malformed settings detected; end ourself
                MessageBox.Show("There was a problem loading settings: The file is possibly malformed. To fix it you can attempt to repair " + Path.GetFileName(PathConfig) + " by hand or delete it and let the program recreate it.");
                Environment.Exit(0);
                return;
            }

            //Now setup the UI with the loaded configuration
            LoadUIConfig();

            #endregion Config Loading

            #region Tab Setup

            tabDatabase.Content = new TabDatabase(this, Config.Database);

            #endregion Tab Setup
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

                SetMySqlStatusToStop();

                if (StopMySql() == 0)
                {

                    //Wait until mysqld exits
                    Console.WriteLine("Waiting until MySQL has stopped...");
                    System.Threading.SpinWait.SpinUntil(() => !IsMySqlRunning);
                    Console.WriteLine("MySQL stopped! Exiting...");

                    ClearSensitiveData();
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
                ClearSensitiveData();
                SaveUIConfig();
                Environment.Exit(0);
            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            CheckProcessesAndUpdateStatus();
            StartProcessCheckTimer(ProcessCheckTimerInterval);
        }

        private void BtnStartSql_Click(object sender, RoutedEventArgs e)
        {
            if (IsMySqlRunning)
            {
                MessageBox.Show("MySQL is already running!", "Information");
                return;
            }

            //Create the directory structure needed to work
            bool createdFiles = CreateServiceFiles();

            //Couldn't create it; stop here
            if (!createdFiles)
            {
                MessageBox.Show("Could not create the required files for MySQL to start!", "Error");
                return;
            }

            //Do MySQL initialization if needed
            bool needsInit = SQLTools.NeedsInit(SQLTools.GetStartParams(Path.GetFullPath(PathMyIniFile), PathMySqlData));

            if (needsInit && String.IsNullOrWhiteSpace(passwordBoxMySqlRootPass.Password))
            {
                MessageBox.Show("MySQL needs to be initialized and can not start with no password set!", "Error");
                return;
            }

            SaveUIConfig();

            bool didInit = DoMySqlInitIfNeeded();

            bool updateIniSuccess = SQLTools.UpdateMyIni(PathMyIniFile, Config.MySQL.Port, Path.GetFullPath(PathMySqlBase), Path.GetFullPath(PathMySqlData));

            if (!updateIniSuccess)
            {
                MessageBox.Show("Could not update my.ini! Aborting!", "Error");
                return;
            }

            SetMySqlStatusToStart();
            StartMySql();

            MySQLIsStarting = true;
            StartProcessCheckTimer(ProcessCheckTimerIntervalFast);

            //If we newly initialized MySQL then set the root password
            if (didInit)
            {
                bool success = SQLTools.SetUserPassword(
                    "root", "localhost",
                    Config.MySQL.Port,
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

            SetMySqlStatusToStop();

            MySQLIsStopping = true;
            StartProcessCheckTimer(ProcessCheckTimerIntervalFast);

            if (StopMySql() != 0)
            {
                MessageBox.Show("Could not stop MySQL!");
                StartProcessCheckTimer(ProcessCheckTimerInterval);
                Console.WriteLine("Reset ProcessCheckTimerInterval because StopMySQL() failed");
            }
        }

        private void CheckBoxSavePass_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxSavePass.IsChecked == true)
            {
                MessageBoxResult result = MessageBox.Show($"Your MySQL root user password will be stored in CLEAR TEXT in {Path.GetFileName(PathConfig)}!\r\n\r\nAre you sure you want to do this?", "Confirm Allow Save Password", MessageBoxButton.YesNo);

                //User did anything but click "Yes"
                if (result != MessageBoxResult.Yes)
                    checkBoxSavePass.IsChecked = false;
            }
        }

        #endregion Events

        #region Timers

        private void StartProcessCheckTimer(int intervalMS)
        {
            StopProcessCheckTimer();

            if (ProcessCheckTimer == null)
            {
                ProcessCheckTimer = new DispatcherTimer();
                ProcessCheckTimer.Tick += ProcessCheckTimer_Tick;
            }

            ProcessCheckTimer.Interval = TimeSpan.FromMilliseconds(intervalMS);
            ProcessCheckTimer.Start();
        }

        private void StopProcessCheckTimer()
        {
            if (ProcessCheckTimer != null)
                ProcessCheckTimer.Stop();
        }

        private void ProcessCheckTimer_Tick(object sender, EventArgs e)
        {
            //Reset the timer intervals on MySQL run state change
            if (MySQLIsStarting && IsMySqlRunning)
            {
                MySQLIsStarting = false;
                StartProcessCheckTimer(ProcessCheckTimerInterval);
                Console.WriteLine("Reset ProcessCheckTimerInterval on MySQL start");
            }

            if (MySQLIsStopping && !IsMySqlRunning)
            {
                MySQLIsStopping = false;
                StartProcessCheckTimer(ProcessCheckTimerInterval);
                Console.WriteLine("Reset ProcessCheckTimerInterval on MySQL stop");
            }

            if (!MySQLIsStarting && !MySQLIsStopping)
                CheckProcessesAndUpdateStatus();
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
                    bool success = SQLTools.CreateNewMyIni(PathMyIniFile, Path.GetFullPath(PathMySqlBase), Path.GetFullPath(PathMySqlData));

                    if (!success)
                        return false;
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

                string prams = SQLTools.GetStartParams(Path.GetFullPath(PathMyIniFile), PathMySqlData);
                bool needsInit = SQLTools.NeedsInit(prams);

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

        private void StartMySql()
        {
            string prams = SQLTools.GetStartParams(Path.GetFullPath(PathMyIniFile), PathMySqlData);
            bool needsInit = SQLTools.NeedsInit(prams);

            Console.WriteLine($"{PathMySqlD} {prams} Needs Init = {needsInit}");
            Console.WriteLine();

            if (!needsInit)
            {
                Console.WriteLine($"Started MySQL");

                //Starts the process attached to this one
                //ProcessHelpers.RunCommand(PathMySqlD, prams, false);

                //Starts the process detached from this one
                ProcessHelpers.RunCommand(PathMySqlLauncher, $"{PathMySqlD} {prams}", false);
            }

            else
                Console.WriteLine("Could not start MySQL because it needs initialization.");
        }

        private int StopMySql()
        {
            string prams = $"-u root -p{passwordBoxMySqlRootPass.Password} --port {Config.MySQL.Port} shutdown";
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

        private void CheckProcessesAndUpdateStatus()
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

        private void SetMySqlStatusToStart()
        {
            StopProcessCheckTimer();
            labelMySqlStatus.Foreground = Brushes.Green;
            labelMySqlStatus.Content = "Starting MySQL...";
            StartProcessCheckTimer(ProcessCheckTimerInterval);
        }

        private void SetMySqlStatusToStop()
        {
            StopProcessCheckTimer();
            labelMySqlStatus.Foreground = Brushes.Red;
            labelMySqlStatus.Content = "Stopping MySQL...";
            StartProcessCheckTimer(ProcessCheckTimerInterval);
        }

        private void LoadUIConfig()
        {
            //Start in the center of the screen if our location is 0
            if (Config.General.WindowLocation.X == 0
            && Config.General.WindowLocation.Y == 0)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            //Else put the window where it was last
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = Config.General.WindowLocation.X;
                this.Top = Config.General.WindowLocation.Y;
            }

            passwordBoxMySqlRootPass.Password = Config.MySQL.RootPass;
            checkBoxSavePass.IsChecked = Config.MySQL.SavePass;
            nudPort.Value = Config.MySQL.Port;
        }

        private void SaveUIConfig()
        {
            if (checkBoxSavePass.IsChecked == true)
                Config.MySQL.RootPass = passwordBoxMySqlRootPass.Password;

            else
                Config.MySQL.RootPass = String.Empty;

            Config.MySQL.SavePass = checkBoxSavePass.IsChecked.TranslateNullableBool();
            Config.MySQL.Port = (int)nudPort.Value;

            SettingsHelper.SaveSettings(PathConfig, Config);
        }

        private void ClearSensitiveData()
        {
            if (!Config.Database.SaveLoginInfo)
            {
                Config.Database.DatabaseUser = String.Empty;
                Config.Database.DatabaseServer = String.Empty;
                Config.Database.DatabasePassword = String.Empty;
            }
        }

        #endregion Methods
    }
}
