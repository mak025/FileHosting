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
        private readonly string _connectionString;

        public UserRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CreateUser(string name, string email, string address, string phoneNumber, object? union, int userType) // rewrite using EFCore - dbcontext.beigntransaction
        {
            // Opens the connection to the SQL Server
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Starts the sequence of checks and commands
            using var transaction = connection.BeginTransaction();
            try
            {
                // Determine UnionId to - If none is defined in the invite - uses the first in the table
                int unionId;
                if (union is int providedUnionId && providedUnionId > 0)
                {
                    unionId = providedUnionId;
                }
                else
                {
                    // try get first existing union - if nonoe found, then creates a default
                    using (var cmdGet = new SqlCommand("SELECT TOP(1) UnionId FROM [Union] ORDER BY UnionId;", connection, transaction))
                    {
                        var obj = cmdGet.ExecuteScalar();
                        if (obj != null && obj != DBNull.Value)
                        {
                            unionId = Convert.ToInt32(obj);
                        }
                        else
                        {
                            // create a default union and return its id | Use EFCore for creating new admin if none exists.
                            using var cmdInsertUnion = new SqlCommand("INSERT INTO [Union] (UnionName) VALUES (@Name); SELECT SCOPE_IDENTITY();", connection, transaction);
                            cmdInsertUnion.Parameters.AddWithValue("@Name", "DefaultUnion");
                            var inserted = cmdInsertUnion.ExecuteScalar();
                            unionId = Convert.ToInt32(inserted);
                        }
                    }
                }

                // Derive Discriminator from enum value to match EF's TPH values (class names)
                string discriminator;
                try
                {
                    discriminator = ((User.UserType)userType).ToString();
                }
                catch
                {
                    discriminator = "User";
                }

                // Insert new user with UnionId and Discriminator
                using var cmdUser = new SqlCommand(
                    "INSERT INTO Users (Name, Email, Address, PhoneNumber, UnionId, Type, Discriminator) " +
                    "VALUES (@Name, @Email, @Address, @PhoneNumber, @UnionId, @Type, @Discriminator);", connection, transaction);

                cmdUser.Parameters.AddWithValue("@Name", name);
                cmdUser.Parameters.AddWithValue("@Email", email);
                cmdUser.Parameters.AddWithValue("@Address", address ?? string.Empty);
                cmdUser.Parameters.AddWithValue("@PhoneNumber", phoneNumber ?? string.Empty);
                cmdUser.Parameters.AddWithValue("@UnionId", unionId);
                cmdUser.Parameters.AddWithValue("@Type", userType);
                cmdUser.Parameters.AddWithValue("@Discriminator", discriminator);

                cmdUser.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (SqlException sqlEx)
            {
                try { transaction.Rollback(); } catch { }
                throw new Exception("A database error occurred while creating the user.", sqlEx);
            }
            catch (Exception ex)
            {
                try { transaction.Rollback(); } catch { }
                throw new Exception("An error occurred while creating the user.", ex);
            }
            finally
            {
                connection.Close();
            }
        }


        public void GetUserById(int userId)
        {
            try
            {
                var user = _dbContext.Users.Find(userId);
                if (user == null)
                {
                    throw new Exception("Brugeren blev ikke fundet");
                }
                return user;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Der opstod en databasefejl under hentning af brugeren.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Der opstod en fejl under hentning af brugeren.", ex);
            }
        }

        public void UpdateUser(string name, string email, string address, string phoneNumber, int userType)
        {
            try
            {
                var user = _dbContext.Users.Find(userId);
                if (user == null)
                {
                    throw new Exception("Brugeren blev ikke fundet");
                }
                user.Name = name;
                user.Email = email;
                user.Address = address;
                user.PhoneNumber = phoneNumber;
                user.UserType = userType;
                _dbContext.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Der opstod en databasefejl under opdatering af brugeren.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Der opstod en fejl under opdatering af brugeren.", ex);
            }
        }

        public void DeleteUser(int userId)
        {
            try
            {
                var user = _dbContext.users.Find(userId);
                if (user != null)
                {
                    _dbContext.users.Remove(user);
                    _dbContext.SaveChanges();
                }

            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Der opstod en databasefejl under sletning af brugeren.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Der opstod en fejl under sletning af brugeren.", ex);

            }
        }
    }
}