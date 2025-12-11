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


        }
    }
