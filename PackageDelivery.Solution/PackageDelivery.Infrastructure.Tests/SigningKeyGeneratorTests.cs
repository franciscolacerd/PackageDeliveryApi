using System.Security.Cryptography;

namespace PackageDelivery.Infrastructure.Tests
{
    [TestFixture]
    public class SigningKeyGeneratorTests
    {
        // [Explicit] → não corre no test run normal; corre-lo à mão quando precisas da chave.
        [Test, Explicit("Utilitário: gera uma signing key para TokenProviderOptions:SecretKey.")]
        public void Generate_HmacSha512_SigningKey()
        {
            var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            TestContext.Out.WriteLine("TokenProviderOptions:SecretKey =");
            TestContext.Out.WriteLine(key);

            Assert.That(key.Length, Is.GreaterThanOrEqualTo(64));
        }
    }
}