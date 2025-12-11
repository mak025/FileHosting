using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FileHostingBackend.Repos
{
    public class UserRepo : IUserRepo
    {     

        private readonly FileHostDBContext _dbContext;

        public UserRepo(FileHostDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateUserAsync(string name, string email, string address, string phoneNumber, int? unionIdFromInvite, int userType)
        { 
        
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                int unionId;
                if (unionIdFromInvite.HasValue && unionIdFromInvite.Value > 0)
                {
                    bool exists = await _dbContext.Union
                        .AnyAsync(u => u.UnionId == unionIdFromInvite.Value);
                    if (exists)
                    {
                        unionId = unionIdFromInvite.Value;
                    }
                    else
                    {
                        unionId = await GetorCreateDefaultUnionAsync();
                    }
                }
                else
                {

                    unionId = await GetorCreateDefaultUnionAsync();

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
        public void UpdateUser(string name, string email, string address, string phoneNumber, int userType)
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
