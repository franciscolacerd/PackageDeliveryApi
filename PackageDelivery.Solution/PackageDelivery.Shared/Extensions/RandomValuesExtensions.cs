namespace PackageDelivery.Shared.Extensions
{
    public static class RandomValuesExtensions
    {
        public static string ToRandomStringOfInts(this int length)
        {
            const string chars = "0123456789";

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Shared.Next(s.Length)])
                .ToArray());
        }
    }
}
