using FileHostingBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FileHostingBackend.Repos
{
    public class UserRepo : IUserRepo 
    {
        private readonly FileHostDBContext _dbContext; // Database context reference
        private readonly IUnionRepo _unionRepo; // Added union repository reference

        public UserRepo(FileHostDBContext dbContext, IUnionRepo unionrepo)
        {
            _dbContext = dbContext;
            _unionRepo = unionrepo;
            
        }

        //SERVICE CHANGES: CreateUserAsync now takes a User object directly and all error handling and user validation has been moved to the service layer.
        public async Task CreateUserAsync(User user) 
        { 
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
        }
            


        public async Task DeleteUserAsync(int userId) // Delete a user by their ID
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId); // Find the user by ID
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