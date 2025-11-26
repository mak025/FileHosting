using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Models;
using Microsoft.Data.SqlClient;

namespace FileHostingBackend.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly string _connectionString;

        public UserRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateUser(string name, string email, string address, string phoneNumber, object? union, int userType)
        {
            using var connection = new SqlConnection(_connectionString);
            try
            {
                using var command = new SqlCommand("INSERT INTO Users (Name, Email, Address, PhoneNumber, Type) VALUES (@Name, @Email, @Address, @PhoneNumber, @Type);", connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Address", address);
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                command.Parameters.AddWithValue("@Type", userType);
              
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("A database error occurred while creating the user.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the user.", ex);
            }
            finally
            {
                connection.Close();
            }
        }

        public void GetUserById(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            try
            {
                using var command = new SqlCommand("SELECT * FROM Users WHERE ID = @UserId;", connection);
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    // Process user data here
                }
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("A database error occurred while retrieving the user.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the user.", ex);
            }
            finally
            {
                connection.Close();
            }
        }
        public void UpdateUser(int userId, string name, string email, string address, string phoneNumber, object? union, int userType)
        {
            using var connection = new SqlConnection(_connectionString);
            try
            {
                using var command = new SqlCommand("UPDATE Users SET Name = @Name, Email = @Email, Address = @Address, PhoneNumber = @PhoneNumber, Type = @Type WHERE ID = @UserId;", connection);

                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Address", address);
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                command.Parameters.AddWithValue("@Type", userType);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("A database error occurred while updating the user.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the user.", ex);
            }
            finally
            {
                connection.Close();
            }
        }

        public void DeleteUser(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            try
            {
                using var command = new SqlCommand("DELETE FROM Users WHERE ID = @UserId;", connection);
                command.Parameters.AddWithValue("@UserId", userId);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (SqlException sqlEx)
            {
                throw new Exception("A database error occurred while deleting the user.", sqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting the user.", ex);
            }
            finally
            {
                connection.Close();
            }


        }
    }
}
