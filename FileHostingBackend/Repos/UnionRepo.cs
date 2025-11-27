using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Repos
{
    public class UnionRepo : IUnionRepo
    {
        private readonly string _connectionstring; 

        public UnionRepo (string connectionstring)
        {
            _connectionstring = connectionstring;
        }

        public void CreateUnion (string unionName, string member)
        {
            using var connection = new SqlConnection(_connectionstring);

        }

    }
}
