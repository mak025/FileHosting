using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using FileHostingBackend.Services;
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

            // Default hard-coded fallback connection string (kept for compatibility)
            var fallbackConnectionString = "Server=localhost,1433;Database=FileHostingDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;";

            // Prefer configuration value from appsettings.json: "ConnectionStrings:DefaultConnection"
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? fallbackConnectionString;

            // Configure EF Core DbContext with the resolved connection string
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

            // Bind Email settings (appsettings.json -> "Email")
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

            // Register InviteService and other app services
            builder.Services.AddScoped<InviteRepo>();

            // Register repo and service for files (existing)
            builder.Services.AddScoped<IStoredFileInfoRepo, StoredFileInfoRepo>();
            builder.Services.AddScoped<StoredFileInfoService>();

            // Register IUserRepo using same connection string as DbContext
            builder.Services.AddScoped<IUserRepo, UserRepo>();

            // Register UserService so it can be injected
            builder.Services.AddScoped<UserService>();
            //Register Union repo
            builder.Services.AddScoped<IUnionRepo, UnionRepo>();
            builder.Services.AddScoped<UnionService>();

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

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}