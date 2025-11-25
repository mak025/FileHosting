using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FileHostingBackend.Models
{
    public class fsDBContext : DbContext
    {
        public fsDBContext() { }

        public fsDBContext(DbContextOptions<fsDBContext> options)
        { 
        
        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StoredFileInfo> StoredFiles { get; set; }
        public DbSet<Folder> Folders { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("Server=; database=; encrypt=false; integrated security=true;");

    }
}
