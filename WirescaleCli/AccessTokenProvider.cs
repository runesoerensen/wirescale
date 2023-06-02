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

    public async Task<string> GetAccessToken(string audience)
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        var authorizationUrlBuilder = _authenticationApiClient.BuildAuthorizationUrl()
            .WithAudience(audience)
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
        var callbackListenerRequest = context.Request;

        string response = string.Empty;

        var parsedQueryString = HttpUtility.ParseQueryString(callbackListenerRequest.Url.Query);
        try
        {
            var errorCode = parsedQueryString["error"];
            if (!string.IsNullOrEmpty(errorCode))
            {
                // Return an appropriate response based on the error code (possible codes are described here: https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.2.1
                response = GetErrorResponse(errorCode);

                throw new AuthorizationRequestException(response);
            }
            response = "Authorization code was received successfully";

            return parsedQueryString["code"];

        }
        // Make sure a response is returned to the user and the http listener is stopped
        finally
        {
            var httpListenerResponse = context.Response;
            var responseString = $"<html><body><p>{response}.<br /><br /> You can now close this window.</p></body></html>";

            var buffer = Encoding.UTF8.GetBytes(responseString);
            httpListenerResponse.ContentLength64 = buffer.Length;
            var responseOutput = httpListenerResponse.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            responseOutput.Close();

            callbackHttpListener.Stop();
        }
    }

    private static string GetErrorResponse(string? errorCode)
    {
        return errorCode switch
        {
            "access_denied" => "The authorization request was denied. Please try logging in again, and make sure to approve the authorization request when prompted",
            "temporarily_unavailable" => "The authorization server is currently unable to handle the request. Please try again later",
            "server_error" => "The authorization server encountered an unexpected condition that prevented it from fulfilling the request. Please try again later",
            // The remaining error types should not be encountered by the end user
            _ => $"An unexpected error occurred during the authorization request: {errorCode}. You may file a bug report with a copy of this error message",
        };
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
