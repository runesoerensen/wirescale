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

# Configure iptables rules to route TCP 80 traffic on wireguard interface to nginx server.
# This approach does not preserve the source IP address on the nginx server.
# Alternatives include proxying requests using the PROXY protocol, using the `x-forwarded-for` HTTP header,
# making the nginx server a peer on the wireguard network, or simply running nginx locally.

nginx_ip_address=$(getent hosts nginx-server | awk '{ print $1 }')

iptables -t nat -A PREROUTING -i wg-wirescale -p tcp --dport 80 -j DNAT --to-destination $nginx_ip_address:80
iptables -t nat -A POSTROUTING -o eth0 -p tcp --dport 80 -d $nginx_ip_address -j MASQUERADE

# Only listen on Docker IP (e.g. don't allow access to wireguard network peers)
wgrest --static-auth-token "$WGREST_STATIC_AUTH_TOKEN" --listen "$(hostname -i):8000"
