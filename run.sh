#!/bin/bash

# Script to run Event Hubs Console with proper TLS configuration for macOS

# Set OpenSSL configuration for macOS
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
export DOTNET_SYSTEM_NET_SECURITY_USESECURECHANNEL=1

# Optional: Force TLS 1.2+
export OPENSSL_CONF=/dev/null

echo "🔒 TLS Configuration applied for macOS"
echo "🚀 Running Event Hubs Console..."
echo ""

dotnet run --project EventHubsConsole.csproj
