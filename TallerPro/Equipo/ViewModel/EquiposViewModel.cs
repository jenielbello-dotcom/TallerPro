using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using TallerPro.Clientes.Service;
using TallerPro.Clientes.ViewModels;
using TallerPro.Equipo.Service;

// Ajusta este using al namespace real de tu RelayCommand.cs
// (por ejemplo TallerPro.Helpers o TallerPro.Commands).


namespace TallerPro.Equipo.ViewModels
{
    public class EquiposViewModel : INotifyPropertyChanged
    {
        private readonly EquipoService equipoService;
        private readonly ClienteService clienteService;

        // Cache en memoria de todos los clientes: el typeahead filtra
        // aquí en lugar de golpear la base de datos en cada tecla.
        private List<Cliente> todosLosClientes = new();

        // Cache en memoria de todos los equipos: la búsqueda de la tabla
        // también filtra localmente, sin ir a la base de datos.
        private List<Equipos> todosLosEquipos = new();

        public ObservableCollection<Equipos> EquiposVista { get; } = new();
        public ObservableCollection<Cliente> ClientesEncontrados { get; } = new();

        // ---------------- Estado del formulario ----------------

        private int idEquipoActual;
        private int? clienteIdSeleccionado;

        private string clienteNombreSeleccionado = string.Empty;
        public string ClienteNombreSeleccionado
        {
            get => clienteNombreSeleccionado;
            set => SetField(ref clienteNombreSeleccionado, value);
        }

        private string tipo = string.Empty;
        public string Tipo
        {
            get => tipo;
            set => SetField(ref tipo, value);
        }

        private string marca = string.Empty;
        public string Marca
        {
            get => marca;
            set => SetField(ref marca, value);
        }

        private string? modelo;
        public string? Modelo
        {
            get => modelo;
            set => SetField(ref modelo, value);
        }

        private string? numeroSerie;
        public string? NumeroSerie
        {
            get => numeroSerie;
            set => SetField(ref numeroSerie, value);
        }

        private string? descripcion;
        public string? Descripcion
        {
            get => descripcion;
            set => SetField(ref descripcion, value);
        }

        private string textoBusqueda = string.Empty;
        public string TextoBusqueda
        {
            get => textoBusqueda;
            set
            {
                SetField(ref textoBusqueda, value);
                FiltrarEquipos();
            }
        }

        private Equipos? equipoSeleccionadoEnTabla;
        public Equipos? EquipoSeleccionadoEnTabla
        {
            get => equipoSeleccionadoEnTabla;
            set
            {
                SetField(ref equipoSeleccionadoEnTabla, value);
                CargarEnFormulario(value);
            }
        }

        private int totalEquipos;
        public int TotalEquipos
        {
            get => totalEquipos;
            private set => SetField(ref totalEquipos, value);
        }

        // ---------------- Comandos ----------------

        public RelayCommand GuardarCommand { get; }
        public RelayCommand LimpiarCommand { get; }
        public RelayCommand EliminarCommand { get; }
        public RelayCommand BuscarCommand { get; }

        public EquiposViewModel()
        {
            equipoService = new EquipoService();
            clienteService = new ClienteService();

            GuardarCommand = new RelayCommand(Guardar, PuedeGuardar);
            LimpiarCommand = new RelayCommand(Limpiar);
            EliminarCommand = new RelayCommand(Eliminar, () => EquipoSeleccionadoEnTabla != null);
            BuscarCommand = new RelayCommand(FiltrarEquipos);

            CargarClientes();
            CargarEquipos();
        }

        private void CargarClientes()
        {
            todosLosClientes = clienteService.ObtenerClientes();
        }

        private void CargarEquipos()
        {
            todosLosEquipos = equipoService.ObtenerEquipos();
            TotalEquipos = todosLosEquipos.Count;
            FiltrarEquipos();
        }

        // Typeahead de clientes: 100% en memoria, sin consultas por tecla.
        public void BuscarClientes(string texto)
        {
            ClientesEncontrados.Clear();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return;
            }

            var coincidencias = todosLosClientes
                .Where(c => $"{c.Nombre} {c.Apellido}"
                    .Contains(texto, StringComparison.OrdinalIgnoreCase))
                .Take(10);

            foreach (var cliente in coincidencias)
            {
                ClientesEncontrados.Add(cliente);
            }
        }

        public void SeleccionarCliente(Cliente cliente)
        {
            clienteIdSeleccionado = cliente.IdCliente;
            ClienteNombreSeleccionado = $"{cliente.Nombre} {cliente.Apellido}";
        }

        private void FiltrarEquipos()
        {
            EquiposVista.Clear();

            IEnumerable<Equipos> resultado = todosLosEquipos;

            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                string texto = TextoBusqueda.Trim();

                resultado = resultado.Where(eq =>
                    eq.NombreCliente.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    eq.Tipo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    eq.Marca.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    (eq.Modelo?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (eq.NumeroSerie?.Contains(texto, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            foreach (var equipo in resultado)
            {
                EquiposVista.Add(equipo);
            }
        }

        private bool PuedeGuardar()
        {
            return clienteIdSeleccionado.HasValue
                && !string.IsNullOrWhiteSpace(Tipo)
                && !string.IsNullOrWhiteSpace(Marca);
        }

        private void Guardar()
        {
            if (!PuedeGuardar())
            {
                MessageBox.Show(
                    "Selecciona un cliente y completa Tipo y Marca antes de guardar.",
                    "Datos incompletos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var equipo = new Equipos
            {
                IdEquipo = idEquipoActual,
                IdCliente = clienteIdSeleccionado!.Value,
                Tipo = Tipo.Trim(),
                Marca = Marca.Trim(),
                Modelo = string.IsNullOrWhiteSpace(Modelo) ? null : Modelo.Trim(),
                NumeroSerie = string.IsNullOrWhiteSpace(NumeroSerie) ? null : NumeroSerie.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim()
            };

            try
            {
                if (idEquipoActual == 0)
                {
                    equipoService.AgregarEquipo(equipo);
                }
                else
                {
                    equipoService.ActualizarEquipo(equipo);
                }

                CargarEquipos();
                Limpiar();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "No se pudo guardar",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Eliminar()
        {
            if (EquipoSeleccionadoEnTabla == null)
            {
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Eliminar el equipo \"{EquipoSeleccionadoEnTabla.Tipo} {EquipoSeleccionadoEnTabla.Marca}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
            {
                return;
            }

            equipoService.EliminarEquipo(EquipoSeleccionadoEnTabla.IdEquipo);
            CargarEquipos();
            Limpiar();
        }

        private void Limpiar()
        {
            idEquipoActual = 0;
            clienteIdSeleccionado = null;

            ClienteNombreSeleccionado = string.Empty;
            Tipo = string.Empty;
            Marca = string.Empty;
            Modelo = null;
            NumeroSerie = null;
            Descripcion = null;

            equipoSeleccionadoEnTabla = null;
            OnPropertyChanged(nameof(EquipoSeleccionadoEnTabla));

            ClientesEncontrados.Clear();
        }

        private void CargarEnFormulario(Equipos? equipo)
        {
            if (equipo == null)
            {
                return;
            }

            idEquipoActual = equipo.IdEquipo;
            clienteIdSeleccionado = equipo.IdCliente;

            ClienteNombreSeleccionado = equipo.NombreCliente;
            Tipo = equipo.Tipo;
            Marca = equipo.Marca;
            Modelo = equipo.Modelo;
            NumeroSerie = equipo.NumeroSerie;
            Descripcion = equipo.Descripcion;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }
    }
}