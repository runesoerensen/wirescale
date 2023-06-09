# Wirescale design draft

This document is a design proposal for Wirescale, a system that allow users access to resources on a secure WireGuard network.

The system is comprised of a [CLI](#wirescale-cli), a [public Auth0 integration](#auth0-integration), an [API](#wirescale-api) and a [WireGuard "server" peer](#wireguard-server-peer).

```mermaid
sequenceDiagram
    Participant CLI as User/CLI

    CLI->>+Auth0: Open browser: https://foo.us.auth0.com/authorize
    Auth0-->>-CLI: authorization_token (audience: webscale API)

    CLI->>+Auth0: POST https://foo.us.auth0.com/oauth/token
    Auth0-->>-CLI: access_token

    CLI->>CLI: Generate Wireguard keypair
    CLI->>+Wirescale API: POST /wireguard_peers (upload `pubkey`)

    Wirescale API-->>+WireGuard Server: Get WireGuard peer configuration
    WireGuard Server-->>-Wirescale API: Return WireGuard configuration

    Wirescale API-->Wirescale API: Assign available IP address on WireGuard network (e.g. `10.8.0.2`)

    Wirescale API->>+WireGuard Server: Create peer (user's `pubkey` + user WireGuard IP)
    WireGuard Server-->>-Wirescale API: Peer created
    Wirescale API-->>-CLI: Return information to connect to WireGuard Server <br /> (e.g. server's pubkey, user's assigned IP, server's public IP/endpoint)

    activate CLI
    critical Establish connection between User and Wireguard Server
        CLI->>CLI: Configure local WireGuard interface
        CLI-->WireGuard Server: Connection established!
    end
    deactivate CLI

    loop
        CLI-->>+WireGuard Server: GET http://10.8.0.1 (server's WireGuard IP address)<br /> (WireGuard server is running nginx and listening only on it's WireGuard peer IP address)
        WireGuard Server-->>-CLI: Respond to WireGuard peer (10.8.0.2): "Welcome to nginx!"
    end
```


## Wirescale CLI

The Wirescale CLI lets users sign in to the Wirescale service like this:

    wirescale login
    ... pops the browser to enter SSO credential
    ... on successful login
    You are logged into the service. WireGuard is activated.

The `login` command initiates a flow that authenticates and authorizes the user to configure and establish a connection to the WireGuard Server peer. This involves:

### Retrieving Wirescale API Credentials
The CLI first authenticates the user with Auth0 using the [Authorization Code Flow with PKCE](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow-with-proof-key-for-code-exchange-pkce) to retrieve credentials used to access the [Wirescale API](#wirescale-api). It will:

* Start a simple HTTP server on the loopback interface that the CLI can use to capture parameters passed to the `redirect_uri` described below.
* Open the browser to initiate the authorization flow using the OAuth Authorization URL for [the configured Auth0 client](#auth0-integration) and the parameters [described here](https://auth0.com/docs/api/authentication#authorization-code-flow-with-pkce). A few notes on a few of those:
    - `audience`: Will be the URI for the [Wirescale API](#wirescale-api), e.g. `https://api.wirescale.com`.
    - `redirect_uri`: Will be the [loopback redirect URI](https://www.rfc-editor.org/rfc/rfc8252#section-7.3) mentioned above, e.g. `http://127.0.0.1:5025/callback`.
    - `scope`: This *could* be used to request Wirescale API specific permissions/scopes (e.g. using `create:wireguard_peer`). This in turn could be enforced with RBAC authorization policies.
* After the user successfully authenticates, the CLI will capture the authorization code and exchange it for an [access token](https://auth0.com/docs/api/authentication#authorization-code-flow-with-pkce45).


### Configuring the WireGuard server and client peers

The CLI will now generate a WireGuard keypair (if a private key is not already present) and call the Wirescale API's [peer creation endpoint](#creating-a-wireguard-client-peer) with the public key. On success, the CLI will use the information returned from the API to configure the local WireGuard peer and connect to the server peer.

## Wirescale API

The Wirescale API is an HTTP service that can access and configure [the WireGuard server](#wireguard-server-peer) to accept WireGuard connections from authorized users, and provide the user with the information necessary to configure their local WireGuard peer interfaces. 

The API is publicly accessible on the hostname specified as the [Auth0 API/resource server identifier](#auth0-integration). The API only serves requests over HTTPS, and can authorize Wirescale users and verify the integrity of access tokens (JWT signed using RS256) without knowledge of any secrets. The API has middleware configured with the Auth0 authority URL, which enables it to retrieve the public key used to validate the token signatures.

### Creating a WireGuard client peer
The API has a single HTTP endpoint (e.g. `POST /wireguard_peers`) that requires a `public_key` parameter and will:

* Access the [WireGuard server](#wireguard-server-peer) and retrieve the server's public WireGuard key, port, network and current peer configuration.
* Assign an unused IP in the WireGuard network address range for the new client peer connection.
* Configure the WireGuard server with the user-provided `public_key` and assigned IP address.
* Return a response with information needed to configure the user's local WireGuard interface (i.e. the WireGuard server's public key, IP and port, the address range for the WireGuard network, and assigned IP address for the user's public key.

## WireGuard Server Peer

The WireGuard server supports multiple client peers managed primarily by the [Wirescale API](#creating-a-wireguard-client-peer). The server is accessible on a static IP and port to which authorized WireGuard peers can connect.

For this demo, the server will also start an nginx server configured to [listen](https://nginx.org/en/docs/http/ngx_http_core_module.html#listen) only on the server's WireGuard IP address and accessible only from the WireGuard network's peers.


## Auth0 Integration

The Auth0 integration acts as the OAuth authorization server, and holds client and resource server/API configuration used to request, set and verify the access token audience, scopes/permissions, loopback redirect uri etc. Auth0 also manages user accounts, connections to identity providers and the login pages users interact with in the browser during login.


