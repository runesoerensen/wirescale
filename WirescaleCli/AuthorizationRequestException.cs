namespace WirescaleCli;

public class AuthorizationRequestException : Exception
{
	public AuthorizationRequestException(string message)
		: base(message)
	{
	}
}
