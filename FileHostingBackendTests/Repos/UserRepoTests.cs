using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading.Tasks;
using Xunit;
using FileHostingBackend.Repos;
using FileHostingBackend.Models; // Adjust namespace if needed

namespace FileHostingBackend.Repos.Tests
{
    public class UserRepoTests
    {
        [Fact]
        public async Task DeleteUserAsync_RemovesUserFromDb()
        {
            //// Arrange
            //var options = new DbContextOptionsBuilder<FileHostDBContext>()
            //    .UseInMemoryDatabase(databaseName: "DeleteUserAsyncTestDb")
            //    .Options;
            

            //// Seed the database with a user
            //using (var context = new FileHostDBContext(options))
            //{
            //    context.Users.Add(new User { ID = 1, Name = "testuser" });
            //    context.SaveChanges();
            //}

            //// Act
            //using (var context = new FileHostDBContext(options))
            //{
            //    var unionRepo = new UnionRepo();
            //    var repo = new UserRepo(context, unionRepo);
            //    await repo.DeleteUserAsync(1);
            //}

            //// Assert
            //using (var context = new AppDbContext(options))
            //{
            //    var user = await context.Users.FindAsync(1);
            //    Assert.Null(user);
            //}
        }
    }
}