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
        private readonly FileHostDBContext _dbContext = new FileHostDBContext();

        public UserRepo(string connectionString)
        {
            _connectionString = connectionString;
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

                User.UserType typeEnums;
                if (Enum.IsDefined(typeof(User.UserType), userType))
                {
                    typeEnums = (User.UserType)userType;
                }
                else
                {
                    typeEnums = User.UserType.Member;
                }
                var user = new User
                {
                    Name = name,
                    Email = email,
                    Address = address,
                    PhoneNumber = phoneNumber,
                    UnionId = unionId,
                    Type = typeEnums
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Der opstod en fejl i databasen ved oprettelse af bruger.", ex);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Der opstod en vejl ved oprettelse af bruger.", ex);
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

        public void UpdateUser(int userId, string name, string email, string address, string phoneNumber, int userType)
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