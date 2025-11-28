using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace FileHosting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            // Example: configure EF Core DbContext (use your connection string or configuration)
            var connectionString = "Server=localhost,1433;Database=FileHostingDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;";
            builder.Services.AddDbContext<FileHostDBContext>(options =>
                options.UseSqlServer(connectionString));

            // Bind Minio settings from configuration (appsettings.json -> "Minio")
            builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));

            // Register repo and service
            builder.Services.AddScoped<IStoredFileInfoRepo, StoredFileInfoRepo>();
            // Optionally: builder.Services.AddScoped<StoredFileInfoService();

            var app = builder.Build();

            // Auto updates the database on application run
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FileHostDBContext>();
                dbContext.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
