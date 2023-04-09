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
            MessageBox.Show("Not implemented yet!");
        }
    }
}
