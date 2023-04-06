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
        }

        #region Events

        private void BtnStartSql_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnStopSql_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion Events
    }
}
