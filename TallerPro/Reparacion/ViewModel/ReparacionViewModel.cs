using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using TallerPro.Equipo.Service;
using TallerPro.Reparacion.Helpers;
using TallerPro.Reparacion.Service;

using ReparacionModelo = TallerPro.Reparacion.Service.Reparacion;

namespace TallerPro.Reparacion.ViewModels
{
    public class ReparacionViewModel : INotifyPropertyChanged
    {
        private readonly ReparacionService reparacionService = new();

        public ReparacionViewModel()
        {
            CargarDatos();

            ReparacionesFiltradas = CollectionViewSource.GetDefaultView(Reparaciones);
            ReparacionesFiltradas.Filter = FiltrarOrdenesPredicate;

            SeleccionarEquipoCommand = new RelayCommand<Equipos>(SeleccionarEquipo);
            GuardarOrdenCommand = new RelayCommand(
                GuardarOrden,
                () => IdEquipoSeleccionado > 0 &&
                      !string.IsNullOrWhiteSpace(ProblemaReportado));
            LimpiarFormularioCommand = new RelayCommand(LimpiarFormulario);
            EditarEstadoCommand = new RelayCommand<ReparacionModelo>(ActualizarEstadoOrden);
            SeleccionarOrdenCommand = new RelayCommand<ReparacionModelo>(SeleccionarOrden);
        }

        public ObservableCollection<ReparacionModelo> Reparaciones { get; set; } = new();
        public ICollectionView ReparacionesFiltradas { get; }
        public ObservableCollection<Equipos> Equipos { get; set; } = new();
        public ObservableCollection<Equipos> EquiposEncontrados { get; set; } = new();
        public ObservableCollection<EstadoOrden> Estados { get; set; } = new();

        public RelayCommand<Equipos> SeleccionarEquipoCommand { get; }
        public RelayCommand GuardarOrdenCommand { get; }
        public RelayCommand LimpiarFormularioCommand { get; }
        public RelayCommand<ReparacionModelo> EditarEstadoCommand { get; }
        public RelayCommand<ReparacionModelo> SeleccionarOrdenCommand { get; }

        private ReparacionModelo? ordenSeleccionada;

        public ReparacionModelo? OrdenSeleccionada
        {
            get => ordenSeleccionada;
            set
            {
                ordenSeleccionada = value;
                OnPropertyChanged();

                if (value != null)
                {
                    SeleccionarOrden(value);
                }
            }
        }
        private int idOrdenEnEdicion = 0;
        public bool EstaEditando => idOrdenEnEdicion > 0;
        public string TituloFormulario =>
     EstaEditando
         ? $"Editando Orden de Reparación #{idOrdenEnEdicion}"
         : "Nueva Orden de Reparación";

        public string EtiquetaEquipo =>
            EstaEditando
                ? "EQUIPO DE LA ORDEN"
                : "BUSCAR Y SELECCIONAR EQUIPO";
        private int idEquipoSeleccionado;
        public int IdEquipoSeleccionado
        {
            get => idEquipoSeleccionado;
            set
            {
                idEquipoSeleccionado = value;
                OnPropertyChanged();
                GuardarOrdenCommand.ActualizarEstado();
            }
        }

        private string textoEquipoBusqueda = string.Empty;
        public string TextoEquipoBusqueda
        {
            get => textoEquipoBusqueda;
            set
            {
                textoEquipoBusqueda = value;
                OnPropertyChanged();

                // Si el usuario escribe manualmente, reiniciamos el ID seleccionado y buscamos
                if (IdEquipoSeleccionado > 0 && !value.Contains("-"))
                {
                    IdEquipoSeleccionado = 0;
                }

                BuscarEquipos();
            }
        }

        private bool mostrarResultados;
        public bool MostrarResultados
        {
            get => mostrarResultados;
            set
            {
                mostrarResultados = value;
                OnPropertyChanged();
            }
        }

        private string problemaReportado = string.Empty;
        public string ProblemaReportado
        {
            get => problemaReportado;
            set
            {
                problemaReportado = value;
                OnPropertyChanged();
                GuardarOrdenCommand.ActualizarEstado();
            }
        }

        private string diagnostico = string.Empty;
        public string Diagnostico
        {
            get => diagnostico;
            set { diagnostico = value; OnPropertyChanged(); }
        }

        private string trabajoRealizado = string.Empty;
        public string TrabajoRealizado
        {
            get => trabajoRealizado;
            set { trabajoRealizado = value; OnPropertyChanged(); }
        }

        private DateTime fechaRecepcion = DateTime.Now;
        public DateTime FechaRecepcion
        {
            get => fechaRecepcion;
            set { fechaRecepcion = value; OnPropertyChanged(); }
        }

        private DateTime? fechaFinalizacion;
        public DateTime? FechaFinalizacion
        {
            get => fechaFinalizacion;
            set { fechaFinalizacion = value; OnPropertyChanged(); }
        }

        private int idEstadoSeleccionado = 1;
        public int IdEstadoSeleccionado
        {
            get => idEstadoSeleccionado;
            set { idEstadoSeleccionado = value; OnPropertyChanged(); }
        }

        private string textoBusquedaOrden = string.Empty;
        public string TextoBusquedaOrden
        {
            get => textoBusquedaOrden;
            set
            {
                textoBusquedaOrden = value;
                OnPropertyChanged();
                ReparacionesFiltradas.Refresh();
            }
        }

        private void SeleccionarEquipo(Equipos? equipo)
        {
            if (equipo == null) return;

            // Mantenemos solo el IdEquipo internamente
            IdEquipoSeleccionado = equipo.IdEquipo;

            // Formato de texto para el TextBox
            TextoEquipoBusqueda = $"{equipo.Marca} {equipo.Modelo} - {equipo.NombreCliente}";

            // Ocultamos la lista desplegable
            MostrarResultados = false;
            EquiposEncontrados.Clear();
        }

        private void SeleccionarOrden(ReparacionModelo? orden)
        {
            if (orden == null) return;

            idOrdenEnEdicion = orden.IdOrden;
            
            OnPropertyChanged(nameof(EstaEditando));
            
            OnPropertyChanged(nameof(TituloFormulario));

            OnPropertyChanged(nameof(EtiquetaEquipo));


            IdEquipoSeleccionado = orden.IdEquipo;
            TextoEquipoBusqueda = $"{orden.EquipoDescripcion} - {orden.ClienteNombre}";

           
        
        IdEstadoSeleccionado = orden.IdEstado;
            FechaRecepcion = orden.FechaRecepcion;
            FechaFinalizacion = orden.FechaFinalizacion;
            ProblemaReportado = orden.ProblemaReportado;
            Diagnostico = orden.Diagnostico;
            TrabajoRealizado = orden.TrabajoRealizado;
        }

        private void BuscarEquipos()
        {
            EquiposEncontrados.Clear();

            if (string.IsNullOrWhiteSpace(TextoEquipoBusqueda) || IdEquipoSeleccionado > 0)
            {
                MostrarResultados = false;
                return;
            }

            string texto = TextoEquipoBusqueda.Trim();
            var resultados = Equipos
    .Where(e =>
        (!string.IsNullOrWhiteSpace(e.NombreCliente) &&
         e.NombreCliente.Contains(texto, StringComparison.OrdinalIgnoreCase)) ||

        (!string.IsNullOrWhiteSpace(e.Marca) &&
         e.Marca.Contains(texto, StringComparison.OrdinalIgnoreCase)) ||

        (!string.IsNullOrWhiteSpace(e.Modelo) &&
         e.Modelo.Contains(texto, StringComparison.OrdinalIgnoreCase)) ||

        (!string.IsNullOrWhiteSpace(e.Tipo) &&
         e.Tipo.Contains(texto, StringComparison.OrdinalIgnoreCase)) ||

        (!string.IsNullOrWhiteSpace(e.NumeroSerie) &&
         e.NumeroSerie.Contains(texto, StringComparison.OrdinalIgnoreCase))
    )
    .Take(6)
    .ToList();

            foreach (var equipo in resultados)
            {
                EquiposEncontrados.Add(equipo);
            }

            MostrarResultados = EquiposEncontrados.Count > 0;
        }

        private bool FiltrarOrdenesPredicate(object item)
        {
            if (item is not ReparacionModelo orden) return false;
            if (string.IsNullOrWhiteSpace(TextoBusquedaOrden)) return true;

            string filtro = TextoBusquedaOrden.Trim();
            return orden.IdOrden.ToString().Contains(filtro, StringComparison.OrdinalIgnoreCase)
                || orden.ClienteNombre.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                || orden.EquipoDescripcion.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                || orden.EstadoNombre.Contains(filtro, StringComparison.OrdinalIgnoreCase)
                || orden.ProblemaReportado.Contains(filtro, StringComparison.OrdinalIgnoreCase);
        }

        private void GuardarOrden()
        {
            // Validar que se haya seleccionado un equipo
            if (IdEquipoSeleccionado <= 0)
            {
                MessageBox.Show(
                    "Debes seleccionar un equipo antes de guardar la orden.",
                    "Datos incompletos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // Validar que se haya indicado el problema
            if (string.IsNullOrWhiteSpace(ProblemaReportado))
            {
                MessageBox.Show(
                    "Debes indicar el problema reportado.",
                    "Datos incompletos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            if (IdEstadoSeleccionado == 4)
            {
                if (string.IsNullOrWhiteSpace(TrabajoRealizado))
                {
                    MessageBox.Show(
                        "Debes indicar el trabajo realizado cuando la orden está marcada como Reparado.",
                        "Datos incompletos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (!FechaFinalizacion.HasValue)
                {
                    MessageBox.Show(
                        "Debes indicar la fecha de finalización cuando la orden está marcada como Reparado.",
                        "Datos incompletos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }
            }

            // EDITAR ORDEN EXISTENTE
            if (idOrdenEnEdicion > 0)
            {
                var ordenEditada = new ReparacionModelo
                {
                    IdOrden = idOrdenEnEdicion,
                    IdEquipo = IdEquipoSeleccionado,
                    IdEstado = IdEstadoSeleccionado,
                    FechaRecepcion = FechaRecepcion,
                    FechaFinalizacion = FechaFinalizacion,
                    ProblemaReportado = ProblemaReportado,
                    Diagnostico = Diagnostico,
                    TrabajoRealizado = TrabajoRealizado
                };

                try
                {
                    reparacionService.ActualizarReparacion(ordenEditada);

                    MessageBox.Show(
                        "La orden de reparación fue actualizada correctamente.",
                        "Orden actualizada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    idOrdenEnEdicion = 0;
                    LimpiarFormulario();
                    CargarDatos();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "No se pudo actualizar la orden",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                return;
            }

            // CREAR NUEVA ORDEN
            var nuevaOrden = new ReparacionModelo
            {
                IdEquipo = IdEquipoSeleccionado,
                IdEstado = IdEstadoSeleccionado,
                FechaRecepcion = FechaRecepcion,
                FechaFinalizacion = FechaFinalizacion,
                ProblemaReportado = ProblemaReportado,
                Diagnostico = Diagnostico,
                TrabajoRealizado = TrabajoRealizado
            };

            try
            {
                reparacionService.AgregarReparacion(nuevaOrden);

                MessageBox.Show(
                    "La orden de reparación fue registrada correctamente.",
                    "Orden registrada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LimpiarFormulario();
                CargarDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "No se pudo registrar la orden",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void LimpiarFormulario()
        {
            idOrdenEnEdicion = 0;
            
            OnPropertyChanged(nameof(EstaEditando));
            
            OnPropertyChanged(nameof(TituloFormulario));

            OnPropertyChanged(nameof(EtiquetaEquipo));

            IdEquipoSeleccionado = 0;
            TextoEquipoBusqueda = string.Empty;
            ProblemaReportado = string.Empty;
            Diagnostico = string.Empty;
            TrabajoRealizado = string.Empty;
            FechaRecepcion = DateTime.Now;
            FechaFinalizacion = null;
            IdEstadoSeleccionado = 1;
            MostrarResultados = false;
            EquiposEncontrados.Clear();
        }

        private void ActualizarEstadoOrden(ReparacionModelo? orden)
        {
            if (orden == null) return;
            reparacionService.ActualizarReparacion(orden);
            CargarDatos();
        }

        private void CargarDatos()
        {
            Equipos.Clear();
            Estados.Clear();
            Reparaciones.Clear();

            foreach (var equipo in reparacionService.ObtenerEquipos()) Equipos.Add(equipo);
            foreach (var estado in reparacionService.ObtenerEstados()) Estados.Add(estado);
            foreach (var reparacion in reparacionService.ObtenerReparaciones()) Reparaciones.Add(reparacion);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? nombre = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}