namespace WirescaleApi;

public interface IWgrestApiClient
{
    Task<WgrestPeer> CreateWgrestPeer(string deviceName, WgrestPeer wireguardPeer);
    Task<List<WgrestDevice>> GetWgrestDevices();
    Task<List<WgrestPeer>> GetWgrestPeers(string deviceName);
    Task<WgrestDeviceOptions> GetWgrestDeviceOptions(string deviceName);
}
