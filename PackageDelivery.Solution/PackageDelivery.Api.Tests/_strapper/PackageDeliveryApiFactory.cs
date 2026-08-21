using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PackageDelivery.Infrastructure.Entities;

namespace PackageDelivery.Api.Tests._strapper
{
    public sealed class PackageDeliveryApiFactory : WebApplicationFactory<Program>
    {
        public async Task SeedUserAsync(string username, string password)
        {
            using var scope = Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AspNetUser>>();

            if (await userManager.FindByNameAsync(username) is not null)
                return;

            var user = new AspNetUser
            {
                UserName = username,
                Email = username,
                Active = true,
                CreatedBy = "integration-tests",
                CreatedDate = DateTime.UtcNow,
                CreatedDateUtc = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Failed to seed the test user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
