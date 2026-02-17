# Azure Event Hubs TLS Troubleshooting Guide

## Problem: Invalid TLS Version Error

If you're getting the error:
```
Invalid TLS version TrackingId:XXXXX, SystemTracker:gateway5, Timestamp:...
```

This typically occurs on macOS with .NET 8.0 when the TLS negotiation fails.

## Solutions (Try in Order)

### 1. Use the provided run script (Recommended)
```bash
./run.sh
```

This script sets the necessary environment variables for macOS.

### 2. Manual Environment Variables
Run with these environment variables:
```bash
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
dotnet run --project EventHubsConsole.csproj
```

### 3. Install/Update OpenSSL on macOS
```bash
# Install via Homebrew
brew install openssl@3

# Link it
brew link openssl@3 --force

# Verify
openssl version
```

### 4. Update .NET SDK
Make sure you have the latest .NET 8.0 SDK:
```bash
dotnet --version
```
Download latest from: https://dotnet.microsoft.com/download

### 5. Verify Azure Configuration

#### Check Connection String Format
Your connection string should look like:
```
Endpoint=sb://YOUR-NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=YOUR-POLICY;SharedAccessKey=YOUR-KEY;
```

**Note:** Do NOT include `EntityPath` in the connection string if you're specifying the Event Hub name separately.

#### Check Azure Event Hub Settings

```bash
# Login to Azure
az login

# Verify namespace exists
az eventhubs namespace show \
  --name YOUR-NAMESPACE \
  --resource-group YOUR-RG

# Check public network access
az eventhubs namespace show \
  --name YOUR-NAMESPACE \
  --resource-group YOUR-RG \
  --query "publicNetworkAccess"

# Verify Event Hub exists
az eventhubs eventhub show \
  --name YOUR-EVENTHUB \
  --namespace-name YOUR-NAMESPACE \
  --resource-group YOUR-RG

# Check SAS policy permissions
az eventhubs namespace authorization-rule show \
  --name YOUR-POLICY \
  --namespace-name YOUR-NAMESPACE \
  --resource-group YOUR-RG
```

### 6. Network and Firewall

#### Option A: Enable public access (for development)
```bash
az eventhubs namespace update \
  --name YOUR-NAMESPACE \
  --resource-group YOUR-RG \
  --public-network-access Enabled
```

#### Option B: Add your IP to firewall
```bash
# Get your public IP
curl ifconfig.me

# Add it to Event Hub firewall
az eventhubs namespace network-rule add \
  --name YOUR-NAMESPACE \
  --resource-group YOUR-RG \
  --ip-address YOUR-IP
```

### 7. Alternative: Use WebSockets Transport

If AMQP over TCP is blocked, try WebSockets:

In your code, change:
```csharp
ConnectionOptions = new EventHubConnectionOptions
{
    TransportType = EventHubsTransportType.AmqpWebSockets  // Changed from AmqpTcp
}
```

### 8. Enable Diagnostic Logging

Add this to your code for detailed logging:
```csharp
Environment.SetEnvironmentVariable("AZURE_LOG_LEVEL", "verbose");
```

## Common Issues

### Issue: Firewall blocking port 5671/5672
**Solution:** Use WebSockets (port 443) or configure firewall

### Issue: Private endpoint without proper DNS
**Solution:** Verify DNS resolution or use public endpoint

### Issue: SAS Key expired or wrong permissions
**Solution:** Regenerate key or create new policy with Send rights

### Issue: Old OpenSSL version on macOS
**Solution:** Update via Homebrew (see step 3)

## Test Connection

Simple test:
```bash
# Test DNS resolution
nslookup YOUR-NAMESPACE.servicebus.windows.net

# Test port connectivity
nc -zv YOUR-NAMESPACE.servicebus.windows.net 5671
```

## Additional Resources

- [Azure Event Hubs Troubleshooting](https://aka.ms/azsdk/net/eventhubs/exceptions/troubleshoot)
- [.NET on macOS](https://learn.microsoft.com/dotnet/core/install/macos)
- [Azure Event Hubs Documentation](https://learn.microsoft.com/azure/event-hubs/)
