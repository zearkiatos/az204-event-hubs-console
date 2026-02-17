using System;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("🔧 Configuring Azure Event Hub Client...");
            
            string connectionString = AppConfiguration.ConnectionString;
            string eventHubName = AppConfiguration.EventHubName;

            if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(eventHubName))
            {
                Console.WriteLine("❌ Error: Missing configuration. Please set the CONNECTION_STRING and EVENT_HUB_NAME environment variables.");
                return;
            }

            Console.WriteLine($"📡 Event Hub: {eventHubName}");
            Console.WriteLine($"🔌 Endpoint: {ExtractEndpoint(connectionString)}");
            Console.WriteLine();

            // Configure EventHubProducerClient with connection options
            var clientOptions = new EventHubProducerClientOptions
            {
                RetryOptions = new EventHubsRetryOptions
                {
                    Mode = EventHubsRetryMode.Exponential,
                    MaximumRetries = 5,
                    Delay = TimeSpan.FromSeconds(1),
                    MaximumDelay = TimeSpan.FromSeconds(30),
                    TryTimeout = TimeSpan.FromSeconds(60)
                },
                ConnectionOptions = new EventHubConnectionOptions
                {
                    TransportType = EventHubsTransportType.AmqpWebSockets  // Uses port 443 instead of 5671
                }
            };

            await using var producerClient = new EventHubProducerClient(connectionString, eventHubName, clientOptions);

            Console.WriteLine("📤 Sending events to Event Hub...");

            using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

            eventBatch.TryAdd(new EventData("First event from C#"));
            eventBatch.TryAdd(new EventData("Second event: Testing from Event Hub Sender"));
            eventBatch.TryAdd(new EventData("Third event: Hello Azure Event Hubs"));

            await producerClient.SendAsync(eventBatch);
            Console.WriteLine("✅ Events sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"📝 Details: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"🔍 Inner: {ex.InnerException.Message}");
            }
            Console.WriteLine();
            Console.WriteLine("💡 Troubleshooting tips:");
            Console.WriteLine("   1. Verify your connection string is correct");
            Console.WriteLine("   2. Check Event Hub namespace exists");
            Console.WriteLine("   3. Verify firewall/network settings");
            Console.WriteLine("   4. Ensure SAS policy has Send permissions");
            Console.WriteLine($"   5. See: https://aka.ms/azsdk/net/eventhubs/exceptions/troubleshoot");
        }
    }

    private static string ExtractEndpoint(string connectionString)
    {
        try
        {
            var parts = connectionString.Split(';');
            var endpoint = Array.Find(parts, p => p.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase));
            return endpoint?.Substring("Endpoint=".Length) ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
