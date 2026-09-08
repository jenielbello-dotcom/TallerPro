using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using global ::TallerPro.Data;
using System;

namespace TallerPro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DatabaseConnection db = new DatabaseConnection();

            if (db.TestConnection())
                MessageBox.Show("✅ La conexion fue bregada.");
            else
                MessageBox.Show("❌ Error al bregar la conexion.");
        }
    }
}


