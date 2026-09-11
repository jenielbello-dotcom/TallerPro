using System;

namespace TallerPro.Equipo.Service
{
    public class Equipos
    {
        public int IdEquipo { get; set; }

        public int IdCliente { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public string? Modelo { get; set; }

        public string? NumeroSerie { get; set; }

        public string? Descripcion { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string NombreCliente { get; set; } = string.Empty;
    }
}