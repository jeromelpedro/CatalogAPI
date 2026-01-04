using Catalog.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infra;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;" +
            "Database=dbCatalog;" +
            "User Id=sa;" +
            "Password=SenhaForte123!;" +
            "TrustServerCertificate=True;"
        );

        return new AppDbContext(optionsBuilder.Options);
    }
}
