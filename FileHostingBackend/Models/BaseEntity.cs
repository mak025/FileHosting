using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public abstract class BaseEntity
    {
        // EF Core will set this when materializing; other code cannot set it.
        public int ID { get; private set; }

        // Shared name property for entities that have a name
        public string Name { get; set; } = string.Empty;

        // Parameterless ctor required by EF Core
        protected BaseEntity() { }

        // Convenient ctor for creating new instances
        protected BaseEntity(string name)
        {
            Name = name;
        }
    }
}
