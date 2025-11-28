using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
        public DbSet<Folder> Folders
        {
            get; set;
        }
    }

    // Do not delete below class - used for migrating to database 
    // https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
    public class FileHostingDbContextFactory : IDesignTimeDbContextFactory<FileHostDBContext>
    {
        public FileHostDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FileHostDBContext>();
            optionsBuilder.UseSqlServer();
            return new FileHostDBContext(optionsBuilder.Options);
        }
    }
}