#!/bin/bash

echo "Testing API Gateway..."
echo "======================"

# Test gateway status
echo "1. Testing Gateway Status:"
curl -s http://localhost:5000/status | jq '.' || echo "Gateway not running"
echo ""

# Test gateway info
echo "2. Testing Gateway Info:"
curl -s http://localhost:5000/gateway/info | jq '.' || echo "Gateway not running"
echo ""

# Test health check
echo "3. Testing Health Check:"
curl -s http://localhost:5000/health || echo "Gateway not running"
echo ""

# Test proxied weather endpoint
echo "4. Testing Proxied Weather Forecast:"
curl -s http://localhost:5000/gateway/weatherforecast | jq '.' || echo "Backend API not running"
echo ""

echo "Test completed."