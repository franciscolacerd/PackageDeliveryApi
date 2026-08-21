using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PackageDelivery.Features.Authentication.Models;
using PackageDelivery.Infrastructure.Context;
using PackageDelivery.Infrastructure.Entities;
using PackageDelivery.Shared.Models;
using PackageDelivery.Shared.Security;
using System.Security.Claims;

namespace PackageDelivery.Features.Authentication.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<AspNetUser> _userManager;
        private readonly PackageDeliveryDbContext _dbContext;
        private readonly TokenProviderOptions _options;

        public TokenService(UserManager<AspNetUser> userManager, PackageDeliveryDbContext dbContext, TokenProviderOptions options)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _options = options;
        }

        public async Task<TokenPair?> IssueForCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);
            if (user is null || !user.Active) return null;
            if (!await _userManager.CheckPasswordAsync(user, password)) return null;

            return await IssueAsync(user, cancellationToken);
        }

        public async Task<TokenPair?> RotateRefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var hash = RefreshTokenHasher.Hash(refreshToken);
            var stored = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
            if (stored is null) return null;

            if (stored.RevokedAtUtc is not null)
            {
                await _dbContext.RefreshTokens
                    .Where(t => t.UserId == stored.UserId && t.RevokedAtUtc == null)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, DateTime.UtcNow), cancellationToken);
                return null;
            }

            if (DateTime.UtcNow >= stored.ExpiresAtUtc) return null;

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == stored.UserId, cancellationToken);
            if (user is null || !user.Active) return null;

            var pair = await IssueAsync(user, cancellationToken);

            stored.RevokedAtUtc = DateTime.UtcNow;
            stored.ReplacedByTokenHash = RefreshTokenHasher.Hash(pair.RefreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return pair;
        }

        private async Task<TokenPair> IssueAsync(AspNetUser user, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                IssuedAt = now,
                NotBefore = now,
                Expires = now.Add(_options.Expiration),
                SigningCredentials = _options.SigningCredentials,
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = user.UserName!,
                    [ClaimTypes.NameIdentifier] = user.Id.ToString(),
                    [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
                }
            };

            var accessToken = new JsonWebTokenHandler().CreateToken(descriptor);
            var refreshToken = GenerateRefreshToken();

            _dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = RefreshTokenHasher.Hash(refreshToken),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.Add(_options.RefreshTokenExpiration)
            });
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new TokenPair(accessToken, refreshToken,
                now.Add(_options.Expiration), now.Add(_options.RefreshTokenExpiration));
        }

        private static string GenerateRefreshToken()
        {
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            return Convert.ToHexString(bytes);
        }
    }
}
