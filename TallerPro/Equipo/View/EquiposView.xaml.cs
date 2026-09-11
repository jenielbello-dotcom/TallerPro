using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TallerPro.Equipo.ViewModels;


using System;

using TallerPro.Clientes.Service;


namespace TallerPro.Equipo.View
{
    public partial class EquiposView : Window
    {
        private readonly EquiposViewModel viewModel;

        public EquiposView()
        {
            InitializeComponent();

            viewModel = new EquiposViewModel();
            DataContext = viewModel;
        }

        // El TextBox está enlazado (TwoWay) a ClienteNombreSeleccionado,
        // así que aquí solo disparamos la búsqueda en memoria y decidimos
        // si se muestra la lista de sugerencias.
        private void ClienteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string texto = ClienteTextBox.Text.Trim();

            viewModel.BuscarClientes(texto);

            ClientesListBox.Visibility =
                viewModel.ClientesEncontrados.Count > 0 && ClienteTextBox.IsFocused
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        // Pequeño retraso para permitir que el clic sobre un ítem de la
        // lista se registre antes de ocultarla por pérdida de foco.
        private void ClienteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!ClientesListBox.IsMouseOver)
                {
                    ClientesListBox.Visibility = Visibility.Collapsed;
                }
            }));
        }

        private void ClientesListBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (ClientesListBox.SelectedItem is Cliente cliente)
            {
                viewModel.SeleccionarCliente(cliente);

                ClientesListBox.Visibility = Visibility.Collapsed;
                ClientesListBox.SelectedItem = null;
            }
        }
    }
}