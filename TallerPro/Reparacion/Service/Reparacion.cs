namespace TallerPro.Reparacion.Service
{
    public class Reparacion
    {
        public int IdOrden { get; set; }

        public int IdEquipo { get; set; }

        public int IdEstado { get; set; }

        public DateTime FechaRecepcion { get; set; }

        public DateTime? FechaFinalizacion { get; set; }

        public string ProblemaReportado { get; set; } = string.Empty;

        public string Diagnostico { get; set; } = string.Empty;

        public string TrabajoRealizado { get; set; } = string.Empty;

        // Información adicional para mostrar en la interfaz
        public string EquipoDescripcion { get; set; } = string.Empty;

        public string ClienteNombre { get; set; } = string.Empty;

        public string EstadoNombre { get; set; } = string.Empty;
    }
}