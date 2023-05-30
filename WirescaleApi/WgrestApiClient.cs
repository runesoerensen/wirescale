using System.Text.Json.Serialization;

namespace WirescaleApi;

public class WgrestApiClient : IWgrestApiClient
{
    private readonly HttpClient _httpClient;

    public WgrestApiClient(HttpClient client)
    {
        _httpClient = client;
    }

    public async Task<List<WgrestDevice>> GetWgrestDevices()
    {
        var devicesResponse = await _httpClient.GetAsync("devices/");
        devicesResponse.EnsureSuccessStatusCode();

        return await devicesResponse.Content.ReadFromJsonAsync<List<WgrestDevice>>();
    }

    public async Task<List<WgrestPeer>> GetWgrestPeers(string deviceName)
    {
        var peersEndpointPath = $"devices/{deviceName}/peers/";
        var peersResponse = await _httpClient.GetAsync(peersEndpointPath);
        peersResponse.EnsureSuccessStatusCode();

        return await peersResponse.Content.ReadFromJsonAsync<List<WgrestPeer>>();
    }

    public async Task<WgrestPeer> CreateWgrestPeer(string deviceName, WgrestPeer wireguardPeer)
    {
        var peersEndpointPath = $"devices/{deviceName}/peers/";
        var createPeerResponse = await _httpClient.PostAsJsonAsync(peersEndpointPath, wireguardPeer);
        createPeerResponse.EnsureSuccessStatusCode();

        return await createPeerResponse.Content.ReadFromJsonAsync<WgrestPeer>();
    }

    public async Task<WgrestDeviceOptions> GetWgrestDeviceOptions(string deviceName)
    {
        var optionsResponse = await _httpClient.GetAsync($"devices/{deviceName}/options/");
        optionsResponse.EnsureSuccessStatusCode();

        return await optionsResponse.Content.ReadFromJsonAsync<WgrestDeviceOptions>();
    }
}

public class WgrestDevice
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("listen_port")]
    public int ListenPort { get; set; }

    [JsonPropertyName("public_key")]
    public string PublicKey { get; set; }

    [JsonPropertyName("networks")]
    public List<string> Networks { get; set; }
}

public class WgrestDeviceOptions
{
    [JsonPropertyName("host")]
    public string Host { get; set; }
}

public class WgrestPeer
{
    [JsonPropertyName("public_key")]
    public string PublicKey { get; set; }

    [JsonPropertyName("url_safe_public_key")]
    public string UrlSafePublicKey { get; set; }

    [JsonPropertyName("allowed_ips")]
    public List<string> AllowedIps { get; set; }
}
