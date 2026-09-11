using System;
using System.Collections.Generic;
using System.Text;

namespace TallerPro.Clientes.Service
{
    public class Cliente
    {
        public int IdCliente { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string? Email { get; set; }
    }
}
