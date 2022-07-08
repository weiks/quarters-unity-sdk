using QuartersSDK.Data.Interfaces;
using System;

namespace QuartersSDK.Data
{
    public class RequestData : Serializable
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; internal set; }
        public string GrantType { get; set; }
        public string CodeVerifier { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public string RedirectUri { get; set; } = String.Empty;
        public string Code { get; set; } = String.Empty;

        public RequestData(string clientId, string clientSecret, string grantType, string codeVerifier, string redirectUri = "", string code = "", string refreshToken = "", string accessToken = "")
        {
            ClientId = !String.IsNullOrEmpty(clientId) ? clientId : throw new ArgumentNullException(nameof(clientId));
            ClientSecret = !String.IsNullOrEmpty(clientSecret) ? clientSecret : throw new ArgumentNullException(nameof(clientSecret));
            GrantType = !String.IsNullOrEmpty(grantType) ? grantType : throw new ArgumentNullException(nameof(grantType));
            CodeVerifier = codeVerifier ?? throw new ArgumentNullException(nameof(codeVerifier));
            RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
            RedirectUri = redirectUri;
            AccessToken = accessToken;
            Code = code;
        }
    }
}