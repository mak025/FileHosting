using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using Microsoft.EntityFrameworkCore;

namespace FileHostingBackend.Services
{

    public class UserService
    {
        private readonly FileHostDBContext _dbContext;
        private readonly IUserRepo _userRepo;
        private readonly IUnionRepo _unionRepo;

        public UserService(IUserRepo userRepo, IUnionRepo unionRepo, FileHostDBContext dbContext)
        { 
            _userRepo = userRepo;
            _unionRepo = unionRepo;
            _dbContext = dbContext;
        }

        //SERVICE CHANGES: CreateUserAsync now handles all user creation logic, including union assignment and error handling.
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
                await _userRepo.CreateUserAsync(user);

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
            await _userRepo.DeleteUserAsync(userId);
        }
    }
}
