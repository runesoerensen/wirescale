namespace WirescaleCli;

public class WireguardCommandException : Exception
{
	public WireguardCommandException(string message)
		: base(message)
	{
	}
}
