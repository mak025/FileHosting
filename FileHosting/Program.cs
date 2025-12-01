using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

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






            // Authentication - cookie based
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });



            // Example policy (optional)
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireClaim(ClaimTypes.Role, User.UserType.Admin.ToString(), User.UserType.SysAdmin.ToString()));
            });






            // Bind Minio settings from configuration (appsettings.json -> "Minio")
            builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));

            // Register repo and service
            builder.Services.AddScoped<IStoredFileInfoRepo, StoredFileInfoRepo>();
            // Optionally: builder.Services.AddScoped<StoredFileInfoService();

            var app = builder.Build();






            // Auto updates the database on application run and ensure a standard user exists
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FileHostDBContext>();
                dbContext.Database.Migrate();

                // Create a default "standard" user if no users exist
                if (!dbContext.Users.Any())
                {
                    var defaultUnion = new Union { UnionName = "DefaultUnion" };
                    dbContext.Add(defaultUnion);
                    dbContext.SaveChanges();

                    // create a SysAdmin as the initial user (adjust fields as you like)
                    var sysAdmin = new SysAdmin("sysadmin", "sysadmin@example.com", "Default address", "0000000000", defaultUnion);
                    dbContext.Add(sysAdmin);
                    dbContext.SaveChanges();
                }
            }






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

            app.UseAuthentication(); // ADDED: ensure authentication middleware runs before authorization
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
