using DataBIDV.Models;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Repositories
{
    public class BIDV_Authenticator : AuthenticatorBase
    {
        readonly string _baseUrl;
        readonly string _clientId;
        readonly string _clientSecret;
        public BIDV_Authenticator(string baseUrl, string clientId, string clientSecret) : base("")
        {
            _baseUrl = baseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, Token);
        }

        async Task<string> GetToken()
        {
            var options = new RestClientOptions(_baseUrl)
            {
                ClientCertificates = new X509CertificateCollection() { new X509Certificate2(File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\cert.pem"))) },
                RemoteCertificateValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; },
            };
            using var client = new RestClient(options);

            var request = new RestRequest(_baseUrl);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("scope", "read");
            request.AddParameter("client_id", _clientId);
            request.AddParameter("client_secret", _clientSecret);
            var response = await client.PostAsync<TokenResponse>(request);
            return $"{response!.TokenType} {response!.AccessToken}";
        }
    }
}
