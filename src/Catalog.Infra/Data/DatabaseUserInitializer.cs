using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Catalog.Infra.Data
{
    public static class DatabaseInitializer
    {
        public static void EnsureDatabase(IConfiguration configuration)
        {
            var setupConnection =
                configuration.GetConnectionString("SetupConnection");

            const string databaseName = "dbCatalog";

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(setupConnection)
                .Options;

            using var context = new AppDbContext(options);

            // 1️⃣ Criar banco (conectado ao MASTER)
            context.Database.ExecuteSqlRaw($@"
            IF DB_ID(N'{databaseName}') IS NULL
            BEGIN
                CREATE DATABASE [{databaseName}];
            END
        ");

            // 2️⃣ Criar login no servidor
            context.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (
                SELECT 1 FROM sys.server_principals 
                WHERE name = N'usuario_app'
            )
            BEGIN
                CREATE LOGIN [usuario_app]
                WITH PASSWORD = N'SenhaForte123!', CHECK_POLICY = OFF;
            END
        ");

            // 3️⃣ Criar usuário no banco
            context.Database.ExecuteSqlRaw($@"
            USE [{databaseName}];

            IF NOT EXISTS (
                SELECT 1 FROM sys.database_principals 
                WHERE name = N'usuario_app'
            )
            BEGIN
                CREATE USER [usuario_app] FOR LOGIN [usuario_app];
                ALTER ROLE db_owner ADD MEMBER [usuario_app];
            END
        ");
        }
    }

}
