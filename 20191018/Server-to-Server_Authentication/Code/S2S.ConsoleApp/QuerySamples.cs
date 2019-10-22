using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace S2S.ConsoleApp
{
    public class QuerySamples
    {
        #region | Private Definitions |

        readonly string _accessToken = string.Empty;
        readonly string _d365Url = string.Empty;

        List<KeyValuePair<string, string>> _headerList = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("OData-MaxVersion","4.0"),
            new KeyValuePair<string, string>("OData-Version","4.0"),
            new KeyValuePair<string, string>("Prefer","odata.include-annotations=*")
        };

        #endregion

        #region | Constructors |

        public QuerySamples(string d365Url, string accessToken)
        {
            _d365Url = d365Url;
            _accessToken = accessToken;
        }

        #endregion

        #region | Public Methods |

        public string Retrieve(string query)
        {
            string result = string.Empty;
            string apiUrl = $"{_d365Url}/api/data/v9.1";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                foreach (var item in _headerList)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                }

                HttpResponseMessage response = httpClient.GetAsync($"{apiUrl}/{query}").GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    result = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new CrmHttpResponseException(response.Content);
                }
            }

            return result;
        }

        #endregion
    }
}
