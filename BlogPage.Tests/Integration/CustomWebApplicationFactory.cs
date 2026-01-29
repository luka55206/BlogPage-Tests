using BlogPage.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlogPage.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BlogDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database for testing
            services.AddDbContext<BlogDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Build service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to get the database context
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<BlogDbContext>();

            // Ensure the database is created
            db.Database.EnsureCreated();
        });
    }
}