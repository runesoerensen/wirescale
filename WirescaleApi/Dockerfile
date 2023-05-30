FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY WirescaleApi.csproj .
RUN dotnet restore

# Copy the remaining source code
COPY . .

# Build the application
RUN dotnet publish -c Release -o out

# Use a lighter base image for the runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

EXPOSE 3000

ENTRYPOINT ["dotnet", "WirescaleApi.dll"]