using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AdPhotoManager.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use a default connection string for migrations
        // This will be overridden at runtime by appsettings.json
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=AdPhotoManager;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
            b => b.MigrationsAssembly("AdPhotoManager.Infrastructure"));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
