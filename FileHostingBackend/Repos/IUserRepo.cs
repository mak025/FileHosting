namespace FileHostingBackend.Repos
{
    public interface IUserRepo
    {
        Task CreateUserAsync(string name, string email, string address, string phoneNumber, int? unionId, int userType);
        Task DeleteUserAsync(int userId);
    }
}
