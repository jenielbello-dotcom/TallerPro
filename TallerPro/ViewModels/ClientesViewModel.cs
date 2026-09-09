using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TallerPro.Service;
using TallerPro.Services;

namespace TallerPro.ViewModels
{
    public class ClienteViewModel : INotifyPropertyChanged
    {
        private readonly ClienteService clienteService;

        // Copia completa en memoria, usada para filtrar sin volver a golpear
        // la base de datos en cada letra que el usuario escribe en el buscador.
        private List<Cliente> clientesCompletos = new();

        private int idEnEdicion;

        public ObservableCollection<Cliente> Clientes { get; } = new();

        // ================================================================
        // SELECCIÓN EN LA TABLA
        // ================================================================
        private Cliente? clienteSeleccionado;

        public Cliente? ClienteSeleccionado
        {
            get => clienteSeleccionado;
            set
            {
                clienteSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HayClienteSeleccionado));
                CargarFormularioDesdeSeleccion();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool HayClienteSeleccionado => ClienteSeleccionado != null;

        // ================================================================
        // CAMPOS DEL FORMULARIO (Nuevo / Editar)
        // ================================================================
        private string nombre = string.Empty;
        public string Nombre
        {
            get => nombre;
            set { nombre = value; OnPropertyChanged(); }
        }

        private string apellido = string.Empty;
        public string Apellido
        {
            get => apellido;
            set { apellido = value; OnPropertyChanged(); }
        }

        private string telefono = string.Empty;
        public string Telefono
        {
            get => telefono;
            set { telefono = value; OnPropertyChanged(); }
        }

        private string email = string.Empty;
        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }

        // ================================================================
        // BÚSQUEDA
        // ================================================================
        private string textoBusqueda = string.Empty;

        public string TextoBusqueda
        {
            get => textoBusqueda;
            set
            {
                textoBusqueda = value;
                OnPropertyChanged();
                FiltrarClientes();
            }
        }

        // ================================================================
        // COMANDOS
        // ================================================================
        public ICommand GuardarCommand { get; }
        public ICommand LimpiarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand BuscarCommand { get; }

        public ClienteViewModel()
        {
            clienteService = new ClienteService();

            GuardarCommand = new RelayCommand(Guardar);
            LimpiarCommand = new RelayCommand(LimpiarFormulario);
            EliminarCommand = new RelayCommand(Eliminar, () => HayClienteSeleccionado);
            BuscarCommand = new RelayCommand(FiltrarClientes);

            CargarClientes();
        }

        // ================================================================
        // CARGAR CLIENTES (única fuente que consulta la BD)
        // ================================================================
        public void CargarClientes()
        {
            clientesCompletos = clienteService.ObtenerClientes();
            AplicarListaAColeccion(clientesCompletos);
        }

        // ================================================================
        // FILTRAR EN MEMORIA (sin tocar la BD)
        // ================================================================
        private void FiltrarClientes()
        {
            string texto = TextoBusqueda.Trim().ToLower();

            List<Cliente> resultado = string.IsNullOrWhiteSpace(texto)
                ? clientesCompletos
                : clientesCompletos.Where(c =>
                        c.Nombre.ToLower().Contains(texto) ||
                        c.Apellido.ToLower().Contains(texto) ||
                        (c.Telefono != null && c.Telefono.ToLower().Contains(texto)) ||
                        (c.Email != null && c.Email.ToLower().Contains(texto)))
                    .ToList();

            AplicarListaAColeccion(resultado);
        }

        private void AplicarListaAColeccion(List<Cliente> lista)
        {
            Clientes.Clear();
            foreach (Cliente cliente in lista)
            {
                Clientes.Add(cliente);
            }
        }

        // ================================================================
        // LLEVAR LA FILA SELECCIONADA AL FORMULARIO (modo edición)
        // ================================================================
        private void CargarFormularioDesdeSeleccion()
        {
            if (ClienteSeleccionado is null)
            {
                idEnEdicion = 0;
                return;
            }

            idEnEdicion = ClienteSeleccionado.IdCliente;
            Nombre = ClienteSeleccionado.Nombre;
            Apellido = ClienteSeleccionado.Apellido;
            Telefono = ClienteSeleccionado.Telefono ?? string.Empty;
            Email = ClienteSeleccionado.Email ?? string.Empty;
        }

        // ================================================================
        // GUARDAR (inserta si es nuevo, actualiza si hay uno seleccionado)
        // ================================================================
        private void Guardar()
        {
            if (string.IsNullOrWhiteSpace(Nombre) || string.IsNullOrWhiteSpace(Apellido))
            {
                MessageBox.Show(
                    "Nombre y apellido son obligatorios.",
                    "Datos incompletos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Cliente cliente = new Cliente
            {
                IdCliente = idEnEdicion,
                Nombre = Nombre.Trim(),
                Apellido = Apellido.Trim(),
                Telefono = string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim()
            };

            try
            {
                if (idEnEdicion == 0)
                {
                    // AgregarCliente asigna cliente.IdCliente internamente (SCOPE_IDENTITY)
                    clienteService.AgregarCliente(cliente);
                }
                else
                {
                    clienteService.ActualizarCliente(cliente);
                }

                LimpiarFormulario();
                CargarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo guardar el cliente: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ================================================================
        // ELIMINAR
        // ================================================================
        private void Eliminar()
        {
            if (ClienteSeleccionado is null)
            {
                return;
            }

            MessageBoxResult confirmacion = MessageBox.Show(
                $"¿Eliminar a {ClienteSeleccionado.Nombre} {ClienteSeleccionado.Apellido}?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                clienteService.EliminarCliente(ClienteSeleccionado.IdCliente);
                LimpiarFormulario();
                CargarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo eliminar el cliente: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ================================================================
        // LIMPIAR FORMULARIO
        // ================================================================
        private void LimpiarFormulario()
        {
            idEnEdicion = 0;
            ClienteSeleccionado = null;
            Nombre = string.Empty;
            Apellido = string.Empty;
            Telefono = string.Empty;
            Email = string.Empty;
        }

        // ================================================================
        // NOTIFICACIÓN MVVM
        // ================================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}