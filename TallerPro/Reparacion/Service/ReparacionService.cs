using System;
using System.Collections.Generic;
using TallerPro.Data;
using Microsoft.Data.SqlClient;
using EquipoModelo = TallerPro.Equipo.Service.Equipos;

namespace TallerPro.Reparacion.Service
{
    public class ReparacionService
    {
        private readonly DatabaseConnection db = new();

        public List<Reparacion> ObtenerReparaciones()
        {
            List<Reparacion> reparaciones = new();

            const string query = @"
                SELECT 
                    o.IdOrden,
                    o.IdEquipo,
                    o.IdEstado,
                    o.FechaRecepcion,
                    o.FechaFinalizacion,
                    o.ProblemaReportado,
                    o.Diagnostico,
                    o.TrabajoRealizado,
                    CONCAT(e.Tipo, ' - ', e.Marca, ' ', e.Modelo) AS EquipoDescripcion,
                    CONCAT(c.Nombre, ' ', c.Apellido) AS ClienteNombre,
                    es.NombreEstado AS EstadoNombre
                FROM OrdenesReparacion o
                INNER JOIN Equipos e 
                    ON o.IdEquipo = e.IdEquipo
                INNER JOIN Clientes c 
                    ON e.IdCliente = c.IdCliente
                INNER JOIN EstadosOrden es 
                    ON o.IdEstado = es.IdEstado
                ORDER BY o.IdOrden DESC";

            using SqlConnection connection = db.GetConnection();
            using SqlCommand command = new(query, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                reparaciones.Add(new Reparacion
                {
                    IdOrden = reader.GetInt32(0),
                    IdEquipo = reader.GetInt32(1),
                    IdEstado = reader.GetInt32(2),
                    FechaRecepcion = reader.GetDateTime(3),
                    FechaFinalizacion = reader.IsDBNull(4)
                        ? null
                        : reader.GetDateTime(4),
                    ProblemaReportado = reader.IsDBNull(5)
                        ? string.Empty
                        : reader.GetString(5),
                    Diagnostico = reader.IsDBNull(6)
                        ? string.Empty
                        : reader.GetString(6),
                    TrabajoRealizado = reader.IsDBNull(7)
                        ? string.Empty
                        : reader.GetString(7),
                    EquipoDescripcion = reader.GetString(8),
                    ClienteNombre = reader.GetString(9),
                    EstadoNombre = reader.GetString(10)
                });
            }

            return reparaciones;
        }

        public List<EstadoOrden> ObtenerEstados()
        {
            List<EstadoOrden> estados = new();

            const string query = @"
                SELECT IdEstado, NombreEstado
                FROM EstadosOrden
                ORDER BY IdEstado";

            using SqlConnection connection = db.GetConnection();
            using SqlCommand command = new(query, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                estados.Add(new EstadoOrden
                {
                    IdEstado = reader.GetInt32(0),
                    NombreEstado = reader.GetString(1)
                });
            }

            return estados;
        }

        public List<EquipoModelo> ObtenerEquipos()
        {
            List<EquipoModelo> equipos = new();

            const string query = @"
        SELECT
            e.IdEquipo,
            e.IdCliente,
            e.Tipo,
            e.Marca,
            e.Modelo,
            e.NumeroSerie,
            e.Descripcion,
            e.FechaRegistro,
            CONCAT(c.Nombre, ' ', c.Apellido) AS NombreCliente
        FROM Equipos e
        INNER JOIN Clientes c
            ON e.IdCliente = c.IdCliente
        ORDER BY e.IdEquipo DESC";

            using SqlConnection connection = db.GetConnection();
            using SqlCommand command = new(query, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                equipos.Add(new EquipoModelo
                {
                    IdEquipo = reader.GetInt32(0),
                    IdCliente = reader.GetInt32(1),
                    Tipo = reader.GetString(2),
                    Marca = reader.GetString(3),
                    Modelo = reader.GetString(4),
                    NumeroSerie = reader.IsDBNull(5)
                        ? null
                        : reader.GetString(5),
                    Descripcion = reader.IsDBNull(6)
                        ? null
                        : reader.GetString(6),
                    FechaRegistro = reader.GetDateTime(7),
                    NombreCliente = reader.GetString(8)
                });
            }

            return equipos;
        }

        public void AgregarReparacion(Reparacion reparacion)
        {
            const string query = @"
        INSERT INTO OrdenesReparacion
        (
            IdEquipo,
            IdEstado,
            FechaRecepcion,
            FechaFinalizacion,
            ProblemaReportado,
            Diagnostico,
            TrabajoRealizado
        )
        VALUES
        (
            @IdEquipo,
            1,
            @FechaRecepcion,
            NULL,
            @ProblemaReportado,
            @Diagnostico,
            @TrabajoRealizado
        )";

            using SqlConnection connection = db.GetConnection();
            using SqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@IdEquipo", reparacion.IdEquipo);
            command.Parameters.AddWithValue("@FechaRecepcion", reparacion.FechaRecepcion);
            command.Parameters.AddWithValue(
                "@ProblemaReportado",
                string.IsNullOrWhiteSpace(reparacion.ProblemaReportado)
                    ? DBNull.Value
                    : reparacion.ProblemaReportado);

            command.Parameters.AddWithValue(
                "@Diagnostico",
                string.IsNullOrWhiteSpace(reparacion.Diagnostico)
                    ? DBNull.Value
                    : reparacion.Diagnostico);

            command.Parameters.AddWithValue(
                "@TrabajoRealizado",
                string.IsNullOrWhiteSpace(reparacion.TrabajoRealizado)
                    ? DBNull.Value
                    : reparacion.TrabajoRealizado);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public void ActualizarReparacion(Reparacion reparacion)
        {
            if (reparacion.IdEstado < 1 || reparacion.IdEstado > 4)
            {
                throw new ArgumentException("El estado de la reparación no es válido.");
            }

            if (reparacion.IdEstado == 4)
            {
                if (string.IsNullOrWhiteSpace(reparacion.TrabajoRealizado))
                {
                    throw new ArgumentException(
                        "No se puede marcar la reparación como Reparado sin registrar el trabajo realizado.");
                }

                if (!reparacion.FechaFinalizacion.HasValue)
                {
                    throw new ArgumentException(
                        "No se puede marcar la reparación como Reparado sin registrar la fecha de finalización.");
                }
            }

            const string query = @"
        UPDATE OrdenesReparacion
        SET
            IdEstado = @IdEstado,
            FechaFinalizacion = @FechaFinalizacion,
            ProblemaReportado = @ProblemaReportado,
            Diagnostico = @Diagnostico,
            TrabajoRealizado = @TrabajoRealizado
        WHERE IdOrden = @IdOrden";

            using SqlConnection connection = db.GetConnection();
            using SqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@IdOrden", reparacion.IdOrden);
            command.Parameters.AddWithValue("@IdEstado", reparacion.IdEstado);

            command.Parameters.AddWithValue(
                "@FechaFinalizacion",
                reparacion.FechaFinalizacion.HasValue
                    ? reparacion.FechaFinalizacion.Value
                    : DBNull.Value);

            command.Parameters.AddWithValue(
                "@ProblemaReportado",
                string.IsNullOrWhiteSpace(reparacion.ProblemaReportado)
                    ? DBNull.Value
                    : reparacion.ProblemaReportado);

            command.Parameters.AddWithValue(
                "@Diagnostico",
                string.IsNullOrWhiteSpace(reparacion.Diagnostico)
                    ? DBNull.Value
                    : reparacion.Diagnostico);

            command.Parameters.AddWithValue(
                "@TrabajoRealizado",
                string.IsNullOrWhiteSpace(reparacion.TrabajoRealizado)
                    ? DBNull.Value
                    : reparacion.TrabajoRealizado);

            connection.Open();

            command.ExecuteNonQuery();
        }




    }
}