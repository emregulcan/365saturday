using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace S2S.ConsoleApp
{
    public class S2SAuthentication
    {
        #region | Private Definitions |

        readonly string _authorityURI = string.Empty;
        readonly string _d365Url = string.Empty;
        readonly string _clientId = string.Empty;
        readonly string _clientSecret = string.Empty;

        #endregion

        #region | Constructors |

        public S2SAuthentication(string authorityURI, string d365Url, string clientId, string clientSecret)
        {
            _authorityURI = authorityURI;
            _d365Url = d365Url;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        #endregion

        #region | Public Methods |

        public string RetrieveAuthTokenByUsingOAuthClientCredentials()
        {
            string result = string.Empty;

            using (HttpClient httpClient = new HttpClient())
            {
                string tokenUrl = $"{_authorityURI}/oauth2/token";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);

                request.Content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret),
                    new KeyValuePair<string, string>("resource", _d365Url),
                });

                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                var content = response.Content.ReadAsStringAsync();

                var parsedData = JValue.Parse(content.Result);

                result = parsedData["access_token"].ToString();
            }

            return result;
        }

        public string RetrieveAuthTokenByUsingADAL()
        {
            string result = string.Empty;

            ClientCredential clientCredential = new ClientCredential(_clientId, _clientSecret);
            AuthenticationContext authenticationContext = new AuthenticationContext(_authorityURI, false);

            var authenticationResult = authenticationContext.AcquireTokenAsync(_d365Url, clientCredential).GetAwaiter().GetResult();
            result = authenticationResult.AccessToken;

            return result;
        }

        #endregion
    }
}
