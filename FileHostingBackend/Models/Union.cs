namespace FileHostingBackend.Models
{
    public class Union
    {
        public int UnionId { get; private set; }
        public string UnionName { get; set; }
        public List<User> Members { get; set; } = new List<User>();
        public Union() { }
        public Union(string unionName, List<User>members)
        {
            UnionName = unionName;
            Members = members;
        }
    }
}
