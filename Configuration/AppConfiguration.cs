using DotNetEnv;

namespace Configuration
{
    public static class AppConfiguration
    {
        static AppConfiguration()
        {
            Env.Load();
        }

        public static string ConnectionString => 
            Environment.GetEnvironmentVariable("CONNECTION_STRING") 
            ?? throw new InvalidOperationException("CONNECTION_STRING environment variable is not set");

        public static string EventHubName => 
            Environment.GetEnvironmentVariable("EVENT_HUB_NAME") 
            ?? throw new InvalidOperationException("EVENT_HUB_NAME environment variable is not set");

        public static void ValidateConfiguration()
        {
            try
            {
                _ = ConnectionString;
                _ = EventHubName;
                Console.WriteLine("✓ Configuration validation passed");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"✗ Configuration validation failed: {ex.Message}");
                throw;
            }
        }
    }
}