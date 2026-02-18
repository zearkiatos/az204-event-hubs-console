using System;
using System.Threading.Tasks;
using System.Net;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.WriteLine("🔧 Configuring Azure Event Hub Client...");
            
            string connectionString = AppConfiguration.ConnectionString;
            string eventHubName = AppConfiguration.EventHubName;

            if (connectionString == null || eventHubName == null)
            {
                Console.WriteLine("Error: Missing configuration. Please set the CONNECTION_STRING and EVENT_HUB_NAME environment variables.");
                return;
            }

            var clientOptions = new EventHubProducerClientOptions
            {
                ConnectionOptions = new EventHubConnectionOptions
                {
                    TransportType = EventHubsTransportType.AmqpWebSockets
                },
                RetryOptions = new EventHubsRetryOptions
                {
                    Mode = EventHubsRetryMode.Exponential,
                    MaximumRetries = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    MaximumDelay = TimeSpan.FromSeconds(10)
                }
            };

            await using var producerClient = new EventHubProducerClient(connectionString, eventHubName, clientOptions);

            try
            {
                Console.WriteLine("Sending events to Event Hub...");

                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

                eventBatch.TryAdd(new EventData("First event from C#"));
                eventBatch.TryAdd(new EventData("Second event: Testing from Event Hub Sender"));
                eventBatch.TryAdd(new EventData("Third event: Hola Azure Event Hubs"));

                await producerClient.SendAsync(eventBatch);
                Console.WriteLine("Events sent successfully! ✅");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending events: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
