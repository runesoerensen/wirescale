FROM debian:bullseye-slim

RUN apt update && apt upgrade && apt install -y wireguard iptables iproute2 curl systemd

COPY wg-wirescale.conf /etc/wireguard/
COPY entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

# Install wgrest 
COPY install-wgrest.sh /install-wgrest.sh
RUN chmod +x /install-wgrest.sh
RUN /install-wgrest.sh

# Expose the WireGuard port
EXPOSE 51820/udp

ENTRYPOINT ["/entrypoint.sh"]
