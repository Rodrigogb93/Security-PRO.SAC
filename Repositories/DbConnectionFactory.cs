using MySql.Data.MySqlClient;
using System.Data;

namespace SecurityPRO.ERP.Web.Repositories;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MySqlConnection")!;
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}