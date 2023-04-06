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
using System.IO;

namespace PortableMySQL8
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            #region Setup

            this.Title = $"{Version.NAME} {Version.VersionPretty}";

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

        private void BtnStartSql_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Globals.PathMySqlData))
                {
                    List<string> files = Directory.EnumerateFiles(Globals.PathMySqlData, "*", SearchOption.AllDirectories).ToList();

                    if (files.Count <= 0)
                    {
                        MessageBoxResult result = MessageBox.Show($"The MySQL data directory at '{Globals.PathMySqlData}' exists but appears to contain no files in it. This directory will need to be DELETED in order for MySQL to sucessfully be initialized.\r\n\r\nAre you SURE you want to do this?", "Warning", MessageBoxButton.YesNo);

                        //User did anything except click "Yes"; stop here
                        if (result != MessageBoxResult.Yes)
                            return;

                        Directory.Delete(Globals.PathMySqlData, true);
                    }
                }

                string prams = "--defaults-file=" + "\"" + Path.Combine(Environment.CurrentDirectory, Globals.PathMyIniFile) + "\" --standalone --explicit_defaults_for_timestamp";

                //No MySQL data directory found, let's initialize it
                if (!Directory.Exists(Globals.PathMySqlData))
                    prams += " --initialize";

                string mysqlpath = "\"" + Path.Combine(Environment.CurrentDirectory, Globals.PathMySqlBase, @"bin", @"mysqld.exe") + "\"";

                Console.WriteLine(mysqlpath +  " " + prams);
                Console.WriteLine();

                ProcessHelpers.RunCommand(mysqlpath, prams, 0, false);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void BtnStopSql_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion Events

        #region Methods

        private bool CreateServiceFiles()
        {
            try
            {
                if (!Directory.Exists(Globals.PathMySqlBase))
                    Directory.CreateDirectory(Globals.PathMySqlBase);

                //Don't create the data directory here, MySQL does that on initialize

                if (!Directory.Exists(Globals.PathMySqlConfig))
                    Directory.CreateDirectory(Globals.PathMySqlConfig);

                if (!File.Exists(Globals.PathMyIniFile))
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
                        "basedir=" + "\"" + Path.GetFullPath(Globals.PathMySqlBase) + "\"",
                        "",
                        "#Path to the database root",
                        "datadir=" + "\"" + Path.GetFullPath(Globals.PathMySqlData) + "\"",
                        "",
                        "#OpenSim needs this on MySQL 8.0.4+",
                        "default-authentication-plugin=mysql_native_password",
                        "",
                        "#Max packetlength to send/receive from to server.",
                        "#MySQL's default seems to be 1 MB but OpenSim needs more than that",
                        "max_allowed_packet=128M"
                    };

                    //Dump the new my.ini file to the proper location
                    File.WriteAllLines(Globals.PathMyIniFile, myIni);
                }

                return true;
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion Methods
    }
}
