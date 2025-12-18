using FileHostingBackend.Models;
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
            await _dbContext.Database.BeginTransactionAsync();
            
            try
            {
                Union union;
                
                if (unionIdFromInvite.HasValue && unionIdFromInvite.Value > 0)
                {
                    union = await _dbContext.Union.FirstOrDefaultAsync(u => u.UnionId == unionIdFromInvite.Value);
                    
                    if (union == null)
                    {
                        union = await _unionRepo.GetOrCreateDefaultUnionAsync();
                    }
                }
                else
                {
                    union = await _unionRepo.GetOrCreateDefaultUnionAsync();
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
                    Union = union,
                    Type = typeEnums
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                await _dbContext.Database.CommitTransactionAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Der opstod en fejl i databasen ved oprettelse af bruger.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Der opstod en vejl ved oprettelse af bruger.", ex);
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    _dbContext.Users.Remove(user);
                    await _dbContext.SaveChangesAsync();
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