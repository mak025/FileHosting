using System.Collections.Generic;

namespace FileHostingBackend.Models
{
    public class Union : BaseEntity
    {
        // Keep a compatibility accessor for code that expects UnionId.
        // We return the base ID. No setter needed because EF will set ID itself.
        public int UnionId => ID;

        // Domain-specific alias for the inherited Name property
        public string UnionName
        {
            get => Name;
            set => Name = value;
        }

        public List<User> Members { get; set; } = new List<User>();

        public Union() : base() { }

        public Union(string unionName, List<User>? members = null) : base(unionName)
        {
            Members = members ?? new List<User>();
        }
    }
}

