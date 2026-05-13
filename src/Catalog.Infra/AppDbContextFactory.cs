using Catalog.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Infra;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlConnection")
            ?? "Server=localhost,1433;Database=dbCatalog;User Id=sa;Password=ChangeMe123!;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
