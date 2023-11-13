namespace Backend.Web
{
    public class AppConfiguration
    {
        public string Npgsql { get; }
        public string Redis { get; }
        public int BufferSize { get; }
        public TimeSpan BufferExpiration { get; }
        public TimeSpan CacheReplicationCompensation { get; }

        public AppConfiguration(IConfiguration configuration)
        {
            Npgsql = ReadConfigValue<string>(configuration, "ConnectionStrings:Npgsql");
            Redis = ReadConfigValue<string>(configuration, "ConnectionStrings:Redis");
            BufferSize = ReadConfigValue<int>(configuration, "Concurrency:BufferSize");
            BufferExpiration = TimeSpan.FromMilliseconds(ReadConfigValue<double>(configuration, "Concurrency:BufferExpirationMilliseconds"));
            CacheReplicationCompensation = TimeSpan.FromMilliseconds(ReadConfigValue<double>(configuration, "Concurrency:CacheReplicationCompensationMilliseconds"));
        }

        private static T ReadConfigValue<T>(IConfiguration configuration, string key)
        {
            var configValue = configuration.GetValue<T>(key);

            if (EqualityComparer<T>.Default.Equals(configValue, default))
                throw new InvalidProgramException($"Invalid configuration: '{key}'");

            return configValue;
        }
    }
}
