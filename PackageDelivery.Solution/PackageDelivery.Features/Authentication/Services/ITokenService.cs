using PackageDelivery.Features.Authentication.Models;

namespace PackageDelivery.Features.Authentication.Services
{
    public interface ITokenService
    {
        Task<TokenPair?> IssueForCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);

        Task<TokenPair?> RotateRefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}
