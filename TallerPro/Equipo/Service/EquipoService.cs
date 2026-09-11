using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using TallerPro.Data;
using TallerPro.Equipo.Service;

namespace TallerPro.Equipo.Service
{
    public class EquipoService
    {
        private readonly DatabaseConnection databaseConnection;

        public EquipoService()
        {
            databaseConnection = new DatabaseConnection();
        }

        // ============================================================
        // OBTENER TODOS LOS EQUIPOS (con nombre del cliente)
        // ============================================================
        public List<Equipos> ObtenerEquipos()
        {
            List<Equipos> equipos = new();

            using SqlConnection connection = databaseConnection.GetConnection();

            string query = @"
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
                INNER JOIN Clientes c ON c.IdCliente = e.IdCliente
                ORDER BY e.IdEquipo DESC";

            using SqlCommand command = new(query, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                equipos.Add(new Equipos
                {
                    IdEquipo = Convert.ToInt32(reader["IdEquipo"]),
                    IdCliente = Convert.ToInt32(reader["IdCliente"]),
                    Tipo = reader["Tipo"].ToString() ?? string.Empty,
                    Marca = reader["Marca"].ToString() ?? string.Empty,
                    Modelo = reader["Modelo"] == DBNull.Value
                        ? null
                        : reader["Modelo"].ToString(),
                    NumeroSerie = reader["NumeroSerie"] == DBNull.Value
                        ? null
                        : reader["NumeroSerie"].ToString(),
                    Descripcion = reader["Descripcion"] == DBNull.Value
                        ? null
                        : reader["Descripcion"].ToString(),
                    FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"]),
                    NombreCliente = reader["NombreCliente"].ToString() ?? string.Empty
                });
            }

            return equipos;
        }

        // ============================================================
        // AGREGAR EQUIPO
        // ============================================================
        public void AgregarEquipo(Equipos equipo)
        {
            using SqlConnection connection = databaseConnection.GetConnection();

            string query = @"
                INSERT INTO Equipos
                (
                    IdCliente,
                    Tipo,
                    Marca,
                    Modelo,
                    NumeroSerie,
                    Descripcion
                )
                VALUES
                (
                    @IdCliente,
                    @Tipo,
                    @Marca,
                    @Modelo,
                    @NumeroSerie,
                    @Descripcion
                )";

            using SqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@IdCliente", equipo.IdCliente);
            command.Parameters.AddWithValue("@Tipo", equipo.Tipo);
            command.Parameters.AddWithValue("@Marca", equipo.Marca);
            command.Parameters.AddWithValue(
                "@Modelo",
                (object?)equipo.Modelo ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@NumeroSerie",
                (object?)equipo.NumeroSerie ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@Descripcion",
                (object?)equipo.Descripcion ?? DBNull.Value);

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                throw new InvalidOperationException(
                    "Ya existe un equipo registrado con ese número de serie.",
                    ex);
            }
        }

        // ============================================================
        // ACTUALIZAR EQUIPO
        // ============================================================
        public void ActualizarEquipo(Equipos equipo)
        {
            using SqlConnection connection = databaseConnection.GetConnection();

            string query = @"
                UPDATE Equipos
                SET
                    IdCliente = @IdCliente,
                    Tipo = @Tipo,
                    Marca = @Marca,
                    Modelo = @Modelo,
                    NumeroSerie = @NumeroSerie,
                    Descripcion = @Descripcion
                WHERE IdEquipo = @IdEquipo";

            using SqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@IdEquipo", equipo.IdEquipo);
            command.Parameters.AddWithValue("@IdCliente", equipo.IdCliente);
            command.Parameters.AddWithValue("@Tipo", equipo.Tipo);
            command.Parameters.AddWithValue("@Marca", equipo.Marca);
            command.Parameters.AddWithValue(
                "@Modelo",
                (object?)equipo.Modelo ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@NumeroSerie",
                (object?)equipo.NumeroSerie ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@Descripcion",
                (object?)equipo.Descripcion ?? DBNull.Value);

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                throw new InvalidOperationException(
                    "Ya existe un equipo registrado con ese número de serie.",
                    ex);
            }
        }

        // ============================================================
        // ELIMINAR EQUIPO
        // ============================================================
        public void EliminarEquipo(int idEquipo)
        {
            using SqlConnection connection = databaseConnection.GetConnection();

            string query = @"
                DELETE FROM Equipos
                WHERE IdEquipo = @IdEquipo";

            using SqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@IdEquipo", idEquipo);

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}