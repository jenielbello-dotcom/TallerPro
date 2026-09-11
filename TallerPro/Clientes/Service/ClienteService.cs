using Microsoft.Data.SqlClient;
using TallerPro.Data;

namespace TallerPro.Clientes.Service
{
    public class ClienteService
    {
        private readonly DatabaseConnection databaseConnection;

        public ClienteService()
        {
            databaseConnection = new DatabaseConnection();
        }

        // ============================================================
        // OBTENER TODOS LOS CLIENTES
        // ============================================================
        public List<Cliente> ObtenerClientes()
        {
            List<Cliente> clientes = new List<Cliente>();

            string query = @"
                SELECT IdCliente, Nombre, Apellido, Telefono, Email
                FROM Clientes
                ORDER BY IdCliente DESC";

            using SqlConnection connection = databaseConnection.GetConnection();
            using SqlCommand command = new SqlCommand(query, connection);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Cliente cliente = new Cliente
                {
                    IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Telefono")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Email"))
                };

                clientes.Add(cliente);
            }

            return clientes;
        }

        // ============================================================
        // INSERTAR CLIENTE
        // ============================================================
        public void AgregarCliente(Cliente cliente)
        {
            // SCOPE_IDENTITY() devuelve el IdCliente autogenerado por SQL Server
            // para que el objeto "cliente" quede con su Id real inmediatamente
            // después de guardarse, sin necesidad de recargar toda la lista.
            string query = @"
                INSERT INTO Clientes
                (Nombre, Apellido, Telefono, Email)
                VALUES
                (@Nombre, @Apellido, @Telefono, @Email);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using SqlConnection connection = databaseConnection.GetConnection();
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
            command.Parameters.AddWithValue("@Apellido", cliente.Apellido);
            command.Parameters.AddWithValue(
                "@Telefono",
                (object?)cliente.Telefono ?? DBNull.Value
            );
            command.Parameters.AddWithValue(
                "@Email",
                (object?)cliente.Email ?? DBNull.Value
            );

            connection.Open();

            object? idGenerado = command.ExecuteScalar();
            cliente.IdCliente = idGenerado != null ? Convert.ToInt32(idGenerado) : 0;
        }

        // ============================================================
        // ACTUALIZAR CLIENTE
        // ============================================================
        public void ActualizarCliente(Cliente cliente)
        {
            string query = @"
                UPDATE Clientes
                SET
                    Nombre = @Nombre,
                    Apellido = @Apellido,
                    Telefono = @Telefono,
                    Email = @Email
                WHERE IdCliente = @IdCliente";

            using SqlConnection connection = databaseConnection.GetConnection();
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
            command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
            command.Parameters.AddWithValue("@Apellido", cliente.Apellido);
            command.Parameters.AddWithValue(
                "@Telefono",
                (object?)cliente.Telefono ?? DBNull.Value
            );
            command.Parameters.AddWithValue(
                "@Email",
                (object?)cliente.Email ?? DBNull.Value
            );

            connection.Open();
            command.ExecuteNonQuery();
        }

        // ============================================================
        // ELIMINAR CLIENTE
        // ============================================================
        public void EliminarCliente(int idCliente)
        {
            string query = @"
                DELETE FROM Clientes
                WHERE IdCliente = @IdCliente";

            using SqlConnection connection = databaseConnection.GetConnection();
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@IdCliente", idCliente);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // ============================================================
        // BUSCAR CLIENTES PARA AUTOCOMPLETADO
        // ============================================================
        public List<Cliente> BuscarClientes(string texto)
        {
            List<Cliente> clientes = new List<Cliente>();

            string query = @"
        SELECT TOP 10
            IdCliente,
            Nombre,
            Apellido,
            Telefono,
            Email
        FROM Clientes
        WHERE
            Nombre LIKE @Texto
            OR Apellido LIKE @Texto
            OR CONCAT(Nombre, ' ', Apellido) LIKE @Texto
        ORDER BY Nombre, Apellido";

            using SqlConnection connection = databaseConnection.GetConnection();
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Texto", texto + "%");

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Cliente cliente = new Cliente
                {
                    IdCliente = reader.GetInt32(reader.GetOrdinal("IdCliente")),
                    Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Telefono")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("Email"))
                };

                clientes.Add(cliente);
            }

            return clientes;
        }
    }
}