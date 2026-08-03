using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Infrastructure.Entities;
using PackageDelivery.Infrastructure.Tests._strapper;

namespace PackageDelivery.Infrastructure.Tests
{
    public class CreateUserTests
    {
        private ServiceProvider serviceProvider;
        private PackageDeliveryDbContext dbContext;
        private UserManager<AspNetUser> userManager;

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = Bootstrapper.Bind();

            this.dbContext = serviceProvider.GetRequiredService<PackageDeliveryDbContext>();
            this.userManager = serviceProvider.GetRequiredService<UserManager<AspNetUser>>();
        }

        [TearDown]
        public async Task TearDown()
        {
            await this.serviceProvider.DisposeAsync();
            await this.dbContext.DisposeAsync();
            this.userManager.Dispose();
        }

        [Test]
        public async Task Handle_CreateUser_Success_ReturnsExpectedResult_AndPersistsToDatabase()
        {
            var userName = $"test{Guid.NewGuid():N}@example.com";
            var password = "SecurePassword123!";

            var user = new AspNetUser
            {
                UserName = userName,
                Email = userName,
                Active = true,
                CreatedBy = userName,
                CreatedDate = DateTime.Now,
                CreatedDateUtc = DateTime.Now
            };

            var result = await this.userManager.CreateAsync(user, password);

            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();

            var userInDb = await this.userManager.FindByEmailAsync(user.UserName);
            userInDb.Should().NotBeNull();
            userInDb!.UserName.Should().Be(user.UserName);

            await this.userManager.DeleteAsync(userInDb);

            userInDb = await this.userManager.FindByEmailAsync(user.UserName);
            userInDb.Should().BeNull();
        }
    }
}
