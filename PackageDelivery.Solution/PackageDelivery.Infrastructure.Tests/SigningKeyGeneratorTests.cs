using System.Security.Cryptography;

namespace PackageDelivery.Infrastructure.Tests
{
    [TestFixture]
    public class SigningKeyGeneratorTests
    {
        [Test, Explicit("Utility: generates a signing key for TokenProviderOptions:SecretKey.")]
        public void Generate_HmacSha512_SigningKey()
        {
            var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            TestContext.Out.WriteLine("TokenProviderOptions:SecretKey =");
            TestContext.Out.WriteLine(key);

            Assert.That(key.Length, Is.GreaterThanOrEqualTo(64));
        }
    }
}