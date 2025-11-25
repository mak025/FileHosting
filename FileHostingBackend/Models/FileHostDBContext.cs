using Microsoft.EntityFrameworkCore;

namespace FileHostingBackend.Models
{
    public class FileHostDBContext : DbContext
    {
        public FileHostDBContext() { }

        // Pass options to the base DbContext so EF and the tools can supply their configured options
        public FileHostDBContext(DbContextOptions<FileHostDBContext> options) : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StoredFileInfo> StoredFiles { get; set; }
        public DbSet<Folder> Folders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only apply the fallback connection string when no options were configured
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost,1433;Database=FileHostingDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;");
            }
        }
    }
}