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


        public async Task CreateUserAsync(User user)
        { 
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
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