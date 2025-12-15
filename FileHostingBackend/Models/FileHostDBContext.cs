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
        public DbSet<Invite> Invites { get; set; }
        public DbSet<Union> Union { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StoredFileInfo>()
                .HasOne(u => u.UploadedBy);
            // Explicit many-to-many relationship between StoredFileInfo and User for file permissions
            modelBuilder.Entity<StoredFileInfo>()
                .HasMany(s => s.UsersWithPermission)
                .WithMany(u => u.FilePermissions)
                .UsingEntity<Dictionary<string, object>>(
                    "StoredFileUserPermission",
                    right => right
                        .HasOne<User>()
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade),
                    left => left
                        .HasOne<StoredFileInfo>()
                        .WithMany()
                        .HasForeignKey("StoredFileInfoID")
                        .OnDelete(DeleteBehavior.Cascade),
                    join =>
                    {
                        join.HasKey("StoredFileInfoID", "UserId");
                        join.ToTable("StoredFileUserPermissions");
                    });
        }
    }

    // Do not delete below class - used for migrating to database 
    // https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
    public class FileHostingDbContextFactory : IDesignTimeDbContextFactory<FileHostDBContext>
    {
        public FileHostDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FileHostDBContext>();
            optionsBuilder.UseSqlServer(); // Empty as the system should reference the FIleHostDBConetextModelSnapshop rather than a running database = code first 
            return new FileHostDBContext(optionsBuilder.Options);
        }
    }
}