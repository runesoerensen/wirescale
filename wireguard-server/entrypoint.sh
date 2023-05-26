#!/bin/bash

PRIVATE_KEY_FILE="/etc/wireguard/private.key"

# Check if private key file exists
if [[ ! -f "$PRIVATE_KEY_FILE" ]]; then
  # Generate WireGuard private key
  private_key=$(wg genkey | tee $PRIVATE_KEY_FILE)
else
  # Read key from file
  private_key=$(cat $PRIVATE_KEY_FILE)
fi

# Update server configuration with private key
sed -i "s|<server_private_key>|$private_key|" /etc/wireguard/wg-wirescale.conf

# Bring up the WireGuard interface
wg-quick up /etc/wireguard/wg-wirescale.conf

# Only listen on Docker IP (e.g. don't allow access to wireguard network peers)
wgrest --static-auth-token "$WGREST_STATIC_AUTH_TOKEN" --listen "$(hostname -i):8000"
