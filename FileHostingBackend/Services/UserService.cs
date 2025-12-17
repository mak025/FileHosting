using FileHostingBackend.Repos;

namespace FileHostingBackend.Services
{
    public class UserService(IUserRepo userRepo)
    {

        private readonly IUserRepo _userRepo = userRepo;

        public async Task CreateUserAsync(string name, string email, string address, string phoneNumber, int? union, int userType)
        {
            await _userRepo.CreateUserAsync(name, email, address, phoneNumber, union, userType);
        }

        public async Task DeleteUserAsync(int userId)
        {
            await _userRepo.DeleteUserAsync(userId);
        }
    }
}
