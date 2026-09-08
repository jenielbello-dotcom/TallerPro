using Microsoft.Data.SqlClient;

namespace TallerPro.Data
{
    public class DatabaseConnection
    {
        private readonly string connectionString =
            "Server=localhost\\SQLSERVER;Database=TallerPro1.0;Trusted_Connection=True;TrustServerCertificate=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    

          public bool TestConnection()
        {
            try
            {
                using SqlConnection connection = GetConnection();
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}