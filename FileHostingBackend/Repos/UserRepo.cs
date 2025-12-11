using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FileHostingBackend.Repos
{
    public class UserRepo : IUserRepo 
    {
        private readonly FileHostDBContext _dbContext;
        private readonly IUnionRepo _unionRepo;

        public UserRepo(FileHostDBContext dbContext, IUnionRepo unionrepo)
        {
            _dbContext = dbContext;
            _unionRepo = unionrepo;
            
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
                        unionId = await _unionRepo.GetOrCreateDefaultUnionAsync();
                    }
                }
                else
                {

                    unionId = await _unionRepo.GetOrCreateDefaultUnionAsync();

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

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _dbContext.Users
                    .Include(u => u.Union)
                    .FirstOrDefaultAsync(u => u.ID == userId);
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

        public async Task UpdateUserAsync(int userId, string name, string email, string address, string phoneNumber, int userType)
        {
            try
            {
                var user = _dbContext.Users.Find(userId);
                if (user == null)
                {
                    throw new Exception("Brugeren blev ikke fundet");
                }
                if (Enum.IsDefined(typeof(User.UserType), userType))
                {
                    user.Type = (User.UserType)userType;
                }
                else
                {
                    user.Type = User.UserType.Member;
                }
                user.Name = name;
                user.Email = email;
                user.Address = address;
                user.PhoneNumber = phoneNumber;
                user.Type = (User.UserType)userType;

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

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                var user = _dbContext.Users.Find(userId);
                if (user != null)
                {
                    _dbContext.Users.Remove(user);
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
        public async Task UpdateFilePermissionsAsync(int userId, List<StoredFileInfo> files)
        {
            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.FilePermissions)
                    .FirstOrDefaultAsync(u => u.ID == userId);
                if (user == null)
                {
                    throw new Exception("Brugeren blev ikke fundet");
                }
                user.FilePermissions = files;
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Der opstod en databasefejl under opdatering af brugerens filrettigheder.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Der opstod en fejl under opdatering af brugerens filrettigheder.", ex);
            }
        }
    }
}