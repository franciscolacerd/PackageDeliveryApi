namespace PackageDelivery.Shared.Policies
{
    public static class ExceptionJitter
    {
        public static IEnumerable<TimeSpan> Get5RetryCount()
        {
            var random = new Random();
            return new[]
            {
                TimeSpan.FromSeconds(1 + random.NextDouble()),
                TimeSpan.FromSeconds(2 + random.NextDouble()),
                TimeSpan.FromSeconds(4 + random.NextDouble()),
                TimeSpan.FromSeconds(8 + random.NextDouble()),
                TimeSpan.FromSeconds(16 + random.NextDouble())
            };
        }
    }
}
