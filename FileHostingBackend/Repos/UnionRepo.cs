using FileHostingBackend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
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

        public void CreateUnion (string unionName)
        {
            using var connection = new SqlConnection(_connectionstring);
            try
            {
                using var command = new SqlCommand("Insert into Unions (UnionName) values (@UnionName);", connection);
                command.Parameters.AddWithValue("@UnionName", unionName);

                connection.Open();
                command.ExecuteNonQuery();

            }
            catch (SqlException sqlEx)
            {
                throw new Exception("A database error occurred while creating the union.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the union.", ex);
            }
            finally
            {
                connection.Close();
            }
        }

        public void DeleteUnion(string unionName)
        {
            using var connection = new SqlConnection(_connectionstring);
            try
            {
                using var command = new SqlCommand("Delete from Unions where UnionName = @UnionName;", connection);
                command.Parameters.AddWithValue("@UnionName", unionName);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("A database error occurred while deleting the union.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the union.", ex);
            }
            finally
            {
                connection.Close();
            }
        }

        public void GetUnionById(int unionId)
        {
            using var connection = new SqlConnection(_connectionstring);
            try
            {
                using var command = new SqlCommand("Select * from Unions where ID = @UnionId;", connection);
                command.Parameters.AddWithValue("@UnionId", unionId);
                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    // Process union data here
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("A database error occurred while retrieving the union.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the union.", ex);
            }
            finally
            {
                connection.Close();
            }
        }

        public void UpdateUnion(string unionName)
        {
            using var connection = new SqlConnection(_connectionstring);
            try
            {
                using var command = new SqlCommand("Update Unions set UnionName = @UnionName;", connection);
                command.Parameters.AddWithValue("@UnionName", unionName);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("A database error occurred while updating the union.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the union.", ex);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
