using System.Windows;
using System.Windows.Controls;
using TallerPro.Reparacion.ViewModels;

namespace TallerPro.Reparacion.View
{
    public partial class ReparacionView : UserControl
    {
        public ReparacionView()
        {
            InitializeComponent();

            DataContext = new ReparacionViewModel();
        }
    }
}