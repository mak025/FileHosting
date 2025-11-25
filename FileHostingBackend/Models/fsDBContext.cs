using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FileHostingBackend.Models
{
    public class fsDBContext : DBContext
    {
        public fsDBContext() { }

        public fsDBContext(DbContextOptions<fsDBContext> options)

    }
}
