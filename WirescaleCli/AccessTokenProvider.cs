using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Builders;
using Auth0.AuthenticationApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace WirescaleCli;

public class AcessTokenProvider
{
    private readonly string _auth0ClientId;
    private readonly string _auth0RedirectUri;
    private readonly AuthenticationApiClient _authenticationApiClient;

    public AcessTokenProvider(string auth0Domain, string auth0ClientId, string auth0RedirectUri)
    {
        _authenticationApiClient = new AuthenticationApiClient(auth0Domain);
        _auth0ClientId = auth0ClientId;
        _auth0RedirectUri = auth0RedirectUri;
    }

    public async Task<string> GetAccessToken()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        var authorizationUrlBuilder = _authenticationApiClient.BuildAuthorizationUrl()
            .WithAudience("http://localhost:3000")
            .WithClient(_auth0ClientId)
            .WithResponseType(AuthorizationResponseType.Code)
            .WithRedirectUrl(_auth0RedirectUri)
            .WithValue("code_challenge", codeChallenge)
            .WithValue("code_challenge_method", "S256")
            .WithScope("openid");

        var authorizationCode = await CaptureAuthorizationCode(authorizationUrlBuilder);

        var accessTokenResponse = await _authenticationApiClient.GetTokenAsync(new AuthorizationCodePkceTokenRequest
        {
            Code = authorizationCode,
            CodeVerifier = codeVerifier,
            RedirectUri = _auth0RedirectUri,
            ClientId = _auth0ClientId,
        });

        return accessTokenResponse.AccessToken;
    }

    private async Task<String> CaptureAuthorizationCode(AuthorizationUrlBuilder authorizationUrlBuilder)
    {
        var callbackHttpListener = new HttpListener();
        callbackHttpListener.Prefixes.Add($"{_auth0RedirectUri}/");

        callbackHttpListener.Start();

        OpenBrowser(authorizationUrlBuilder.Build());

        var context = await callbackHttpListener.GetContextAsync();
        var callbackRequest = context.Request;


        var authorizationCode = HttpUtility.ParseQueryString(callbackRequest.Url.Query)["code"];

        var response = context.Response;
        var responseString = "<html><body><p>Authorization code received successfully. You can now close this window.</p></body></html>";
        var buffer = Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        await responseOutput.WriteAsync(buffer, 0, buffer.Length);
        responseOutput.Close();

        callbackHttpListener.Stop();

        return authorizationCode;
    }

    private static void OpenBrowser(Uri uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri.ToString())
            {
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Failed to open browser: {exception.Message}");
        }
    }

    private static string GenerateCodeVerifier()
    {
        const int CodeVerifierLength = 64;

        using (var randomNumberGenerator = RandomNumberGenerator.Create())
        {
            var bytes = new byte[CodeVerifierLength];
            randomNumberGenerator.GetBytes(bytes);

            return Base64UrlEncoder.Encode(bytes);
        }
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));

            return Base64UrlEncoder.Encode(challengeBytes);
        }
    }
}
