using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace S2S.ConsoleApp
{
    public class CRUDSamples
    {
        #region | Private Definitions |

        readonly string _accessToken = string.Empty;
        readonly string _d365Url = string.Empty;
        readonly Guid _impersonatedUserId = Guid.Empty;

        List<KeyValuePair<string, string>> _headerList = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("OData-MaxVersion","4.0"),
            new KeyValuePair<string, string>("OData-Version","4.0")
        };

        #endregion

        #region | Constructors |

        public CRUDSamples(string d365Url, string accessToken)
        {
            _d365Url = d365Url;
            _accessToken = accessToken;
        }

        public CRUDSamples(string d365Url, string accessToken, Guid impersonatedUserId)
        {
            _d365Url = d365Url;
            _accessToken = accessToken;
            _impersonatedUserId = impersonatedUserId;
        }

        #endregion

        #region | Public Methods |

        public string Create(string entityLogicalName, object data)
        {
            string result = string.Empty;

            string apiBaseUrl = $"{_d365Url}/api/data/v9.1";
            string apiFullUrl = $"{apiBaseUrl}/{entityLogicalName}s";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //INFO : Update - UpdateSingleField - Retrieve metotlarına da ekleyebilirsiniz.
                if (_impersonatedUserId != Guid.Empty)
                {
                    httpClient.DefaultRequestHeaders.Add("MSCRMCallerID", _impersonatedUserId.ToString());
                }

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiFullUrl);
                request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                var content = response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                    {
                        var responseHeaders = response.Headers;
                        var entityIdHeader = responseHeaders.GetValues("OData-EntityId").FirstOrDefault();

                        result = entityIdHeader;
                    }
                }
                else
                {
                    throw new CrmHttpResponseException(response.Content);
                }
            }

            return result;
        }

        public void Update(string entityLogicalName, Guid id, object updateContent)
        {
            string apiBaseUrl = $"{_d365Url}/api/data/v9.1";
            string apiFullUrl = $"{apiBaseUrl}/{entityLogicalName}s({id.ToString()})";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), apiFullUrl);
                request.Content = new StringContent(JsonConvert.SerializeObject(updateContent), Encoding.UTF8, "application/json");

                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                var content = response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new CrmHttpResponseException(response.Content);
                }
            }
        }

        public void UpdateSingleField(string entityLogicalName, Guid id, string fieldName, JToken value)
        {
            string apiBaseUrl = $"{_d365Url}/api/data/v9.1";
            string apiFullUrl = $"{apiBaseUrl}/{entityLogicalName}s({id.ToString()})/{fieldName}";

            JObject data = new JObject();
            data.Add("value", value);

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, apiFullUrl);
                request.Content = new StringContent(data.ToString(), Encoding.UTF8, "application/json");

                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                var content = response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new CrmHttpResponseException(response.Content);
                }
            }
        }

        #endregion
    }
}
