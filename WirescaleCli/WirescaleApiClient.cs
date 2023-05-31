using System.Net.Http.Json;

namespace WirescaleCli;

public class WirescaleApiClient
{
    private readonly string _baseUri;

    public WirescaleApiClient(string baseUri)
    {
        _baseUri = baseUri;
    }

    public async Task<WirescalePeerRegistration> RegisterPublicKey(string accessToken, WireguardKeyPair wireguardKeyPair)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            var response = await httpClient.PostAsJsonAsync($"{_baseUri}/wireguardpeer", wireguardKeyPair.PublicKey);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<WirescalePeerRegistration>();
        }
    }
}
