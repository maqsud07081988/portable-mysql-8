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
using System.Windows.Shapes;

using PortableMySQL8.Themes;

namespace PortableMySQL8
{
    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow
    {
        private MainWindow Instance = null;
        private Settings Config = null;
        private WindowTheming WindowTheme = new WindowTheming();

        public LicenseWindow(MainWindow _instance, Settings _config)
        {
            InitializeComponent();

            Instance = _instance;
            Config = _config;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            textBlockLicense.Text = Version.LicenseText;

            WindowTheme.ApplyTheme("Blue", "Light");
        }

        private void BtnAgree_Click(object sender, RoutedEventArgs e)
        {
            Config.General.AgreedToLicense = true;
            this.Close();
        }

        private void btnDisagree_Click(object sender, RoutedEventArgs e)
        {
            Config.General.AgreedToLicense = false;
            MessageBox.Show("You must first agree to the license before using this software", "License");
            this.Close();
        }
    }
}
