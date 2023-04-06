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

                if (!Directory.Exists(Globals.PathMySqlData))
                    Directory.CreateDirectory(Globals.PathMySqlData);

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
