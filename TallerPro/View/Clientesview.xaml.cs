using System.Windows.Controls;
using TallerPro.ViewModels;

namespace TallerPro.Views
{
    public partial class ClientesView : Page
    {
        public ClientesView()
        {
            InitializeComponent();

            // Si ya asignas el DataContext desde otro lugar (por ejemplo,
            // un contenedor de inyección de dependencias o el ViewModel
            // de navegación), elimina esta línea para evitar duplicarlo.
            DataContext = new ClienteViewModel();
        }
    }
}