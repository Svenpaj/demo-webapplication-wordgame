/*using Npgsql;

namespace demo_webapplication;
    public class Database
    {
        private readonly string _host = "localhost";
        private readonly string _database = "database_for_wordapp";
        private readonly string _username = "postgres";
        private readonly string _password = "hejhej123";
        private readonly int _port = 5432;

        private NpgsqlDataSource _connection;

        public NpgsqlDataSource Connection()
        { 
        return _connection; 
        }

        public Database()
        {
        string connectionString = $"Host={_host};Port={_port};Username={_username};Password={_password};Database={_database}";
        _connection = NpgsqlDataSource.Create(connectionString);
        }

}*/

using Npgsql;

namespace demo_webapplication
{
    // Här skapar vi en klass som heter Database som innehåller en metod som heter Connection som returnerar en NpgsqlDataSource och en konstruktor som sätter connection string och datasource
    public class Database
    {   
        // Här skapar vi en privat variabel som heter _connectionString som innehåller en connection string
        private readonly string _connectionString;

        // Här skapar vi en privat variabel som heter _dataSource som är av typen NpgsqlDataSource som kommer att innehålla en datasource
        private readonly NpgsqlDataSource _dataSource;

        // Konstruktor som sätter connection string och datasource
        public Database()
        {
            // Här sätter vi connection string till en NpgsqlConnectionStringBuilder som innehåller host, port, username, password, database och pooling
            _connectionString = new NpgsqlConnectionStringBuilder
            {
                Host = "localhost", // Server namn eller IP-adress till databasen
                Port = 5432, // Port till databasen
                Username = "postgres", // Användarnamn för att ansluta till databasen
                Password = "hejhej123", // Lösenord för att ansluta till databasen
                Database = "database_for_wordapp", // Namn på databasen
                Pooling = true // Aktivera connection pooling för att återanvända anslutningar till databasen
            }.ToString();

            // Här sätter vi _dataSource till en NpgsqlDataSource som skapar en datasource med connection string
            _dataSource = NpgsqlDataSource.Create(_connectionString);
        }

        // Metod som returnerar en NpgsqlDataSource som innehåller en datasource
        public NpgsqlDataSource Connection()
        {
            return _dataSource;
        }
    }
}


