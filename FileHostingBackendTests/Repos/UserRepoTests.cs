using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FileHostingBackend.Repos;
using FileHostingBackend.Models;

namespace FileHostingBackend.Repos.Tests
{
    public class UserRepoTests
    {
        [Fact]
        public async Task DeleteUserAsync_RemovesUserFromDb()
        {
            // Arrange: create in-memory database options with a unique name
            var options = new DbContextOptionsBuilder<FileHostDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            int seededUserId;

            // Seed the database with a Union and a User
            await using (var seedContext = new FileHostDBContext(options))
            {
                var union = new Union { UnionName = "DefaultUnion" };
                seedContext.Union.Add(union);

                var user = new User
                {
                    Name = "Test User",
                    Email = "test@example.com",
                    Address = "Addr",
                    PhoneNumber = "000",
                    Union = union,
                    Type = User.UserType.Member
                };

                seedContext.Users.Add(user);
                await seedContext.SaveChangesAsync();

                seededUserId = user.ID;
            }

            // Act: create a new context instance and call the repo delete method
            await using (var actContext = new FileHostDBContext(options))
            {
                var unionRepoMock = new Mock<IUnionRepo>();
                var repo = new UserRepo(actContext, unionRepoMock.Object);

                // Ensure the user exists before deletion
                var existing = await actContext.Users.FindAsync(seededUserId);
                Xunit.Assert.NotNull(existing);

                await repo.DeleteUserAsync(seededUserId);
            }

            // Assert: verify the user no longer exists using a fresh context
            await using (var assertContext = new FileHostDBContext(options))
            {
                var user = await assertContext.Users.FindAsync(seededUserId);
                Xunit.Assert.Null(user);
            }
        }
    }
}