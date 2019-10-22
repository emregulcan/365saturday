using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace S2S.ConsoleApp
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/dynamics365/customer-engagement/developer/webapi/web-api-helper-code-crmhttpresponseexception-class
    /// </summary>
    public class CrmHttpResponseException : System.Exception
    {
        #region | Private Definitions |

        private static string _stackTrace;

        #endregion

        #region | Properties |

        public override string StackTrace
        {
            get { return _stackTrace; }
        }

        #endregion

        #region | Constructors |

        public CrmHttpResponseException(HttpContent content) : base(ExtractMessageFromContent(content))
        {

        }

        public CrmHttpResponseException(HttpContent content, Exception innerexception) : base(ExtractMessageFromContent(content), innerexception)
        {

        }

        #endregion

        #region | Public Methods |

        public static string ExtractMessageFromContent(HttpContent content)
        {
            string result = string.Empty;

            string downloadedContent = content.ReadAsStringAsync().Result;

            if (content.Headers.ContentType.MediaType.Equals("text/plain"))
            {
                result = downloadedContent;
            }
            else if (content.Headers.ContentType.MediaType.Equals("application/json"))
            {
                JObject jcontent = (JObject)JsonConvert.DeserializeObject(downloadedContent);
                IDictionary<string, JToken> d = jcontent;

                // An error message is returned in the content under the 'error' key. 
                if (d.ContainsKey("error"))
                {
                    JObject error = (JObject)jcontent.Property("error").Value;
                    result = (String)error.Property("message").Value;
                }
                else if (d.ContainsKey("Message"))
                {
                    result = (String)jcontent.Property("Message").Value;
                }

                if (d.ContainsKey("StackTrace"))
                {
                    _stackTrace = (String)jcontent.Property("StackTrace").Value;
                }
            }
            else if (content.Headers.ContentType.MediaType.Equals("text/html"))
            {
                result = "HTML content that was returned is shown below.";
                result += "\n\n" + downloadedContent;
            }
            else
            {
                result = $"No handler is available for content in the {content.Headers.ContentType.MediaType} format.";
            }

            return result;
        }

        #endregion
    }
}
