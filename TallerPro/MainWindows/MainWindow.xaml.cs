using System.Windows;
using TallerPro.Reparacion.View;

namespace TallerPro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Content = new ReparacionView();
        }
    }
}