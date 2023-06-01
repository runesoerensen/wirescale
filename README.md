# Wirescale

## Overview
Wirescale is a system designed to facilitate secure user access to resources on a WireGuard network. It features a Command Line Interface (CLI), an Auth0 integration for user authentication, a Wirescale API, and a WireGuard server peer that controls access to protected resources. With Wirescale, you can securely authenticate users and seamlessly manage WireGuard peer connections.

Note that this software is merely a draft developed as part of a code challenge. It is not intended for production use. For more information about this system [please refer to the design document](https://github.com/runesoerensen/wirescale/blob/main/rfd/wirescale-design-draft.md).

## Installation and Setup

The setup process involves building the CLI, and deploying the server-side components (Wirescale API, and WireGuard server peer) using Docker Compose.

Please ensure that Docker, Docker Compose and WireGuard are installed on your machine.

### Installing Docker and Docker Compose

Follow the instructions on the Docker website for your specific OS:

- [Install Docker on Ubuntu](https://docs.docker.com/engine/install/ubuntu/)
- [Install Docker on macOS](https://docs.docker.com/docker-for-mac/install/)
- [Install Docker Compose](https://docs.docker.com/compose/install/)

### Installing WireGuard

#### Installing WireGuard on Ubuntu

WireGuard is available in the default repositories of Ubuntu 20.04 and later. Run the following commands to install it:

```bash
sudo apt update
sudo apt install wireguard
```

#### Installing WireGuard on macOS

For macOS, WireGuard can be installed using Homebrew:

```bash
brew install wireguard-tools
```

Note that WireGuard needs to be available from the commandline, so installing WireGuard using homebrew is preferred over the App Store distributed WireGuard on OSX.

Please consult the [official WireGuard installation guide](https://www.wireguard.com/install/) for the most up-to-date instructions and other platforms.


### Setting Up Wirescale Server Components

To setup Wirescale's server components, run:

```bash
git clone https://github.com/runesoerensen/wirescale.git
cd wirescale
docker-compose up
```

This will build and run container images running the Wirescale API, the WireGuard server peer, as well as an nginx server.

### Building and Publishing the CLI

The Wirescale CLI is a .NET Core console application, so you need to have the .NET Core SDK (version 7.0 or later) installed on your machine to build and publish it.

#### Installing .NET Core SDK on macOS

Please refer to [the .NET 7.0 download page](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) for installation instructions and downloads for your platform.

#### Compiling and Publishing the CLI

Navigate to the CLI project directory to build and publish executable the CLI for your platform:

```bash
cd WirescaleCli/
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-arm64 --self-contained
dotnet publish -c Release -r win-x64 --self-contained

# To publish a single executable file add the PublishSingleFile property, e.g.:
dotnet publish -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained
```

This will compile the Wirescale CLI for Ubuntu, macOS, and Windows respectively. The resulting binaries will be located in the `publish` subdirectory of the build output directory.

Please refer to [.NET Runtime Identifier catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog) for a complete list of supported platforms and architectures.

Now, you can run the CLI with:

```bash
./path_to_binary/wirescalecli
```

## Using Wirescale

To authenticate with the Wirescale service, you can use the `login` command:

```bash
wirescalecli login
```

This will open a browser where you can sign in (or create an account) using the Auth0 integration. On successful login, the CLI will generate a WireGuard public key to the Wirescale API and configure WireGuard locally.

To verify the connection, you can run:

```bash
curl http://10.8.0.1
```

If everything is set up correctly, you should see a "Welcome to nginx!" message.
