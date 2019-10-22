using Newtonsoft.Json.Linq;
using System;

namespace S2S.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //INFO : https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/issues/1346#issuecomment-432426513

                string authorityURI = "https://login.microsoftonline.com/{TENANT_ID}";
                string d365Url = "https://{organization}.{crmregion}.dynamics.com/";
                string clientId = "{YOUR_AZURE_AD_APPLICATION_IDENTITY}";
                string clientSecret = "{YOUR_AZURE_AD_APPLICATION_SECRET}";

                S2SAuthentication s2sAuthentication = new S2SAuthentication(authorityURI, d365Url, clientId, clientSecret);
                var token = s2sAuthentication.RetrieveAuthTokenByUsingADAL();

                if (!string.IsNullOrEmpty(token))
                {
                    #region | CRUD |

                    CRUDSamples crudSamples = new CRUDSamples(d365Url, token);

                    JObject contactCreateData = new JObject();
                    contactCreateData.Add("firstname", "Firstname");
                    contactCreateData.Add("lastname", "Lastname");
                    contactCreateData.Add("telephone1", "123456789");

                    var createdContactURI = crudSamples.Create("contact", contactCreateData);
                    var createdDataId = WebAPIHelper.GetRecordId(createdContactURI);

                    JObject accountCreateData = new JObject();
                    accountCreateData.Add("name", "Company AD");

                    var createdAccountURI = crudSamples.Create("account", accountCreateData);
                    var createdAccountId = WebAPIHelper.GetRecordId(createdAccountURI);

                    JObject contactUpdateData = new JObject();
                    contactUpdateData.Add("jobtitle", "My awesome job");
                    contactUpdateData.Add("emailaddress1", "hello@email.com");
                    contactUpdateData.Add("parentcustomerid_account@odata.bind", $"/accounts({createdAccountId})");

                    crudSamples.Update("contact", createdDataId, contactUpdateData);
                    crudSamples.UpdateSingleField("contact", createdDataId, "telephone1", "555 999 88 33");

                    #endregion

                    #region | Query |

                    QuerySamples querySamples = new QuerySamples(d365Url, token);
                    var retrievedData = querySamples.Retrieve("contacts?$select=fullname,_parentcustomerid_value");

                    Console.WriteLine("Dynamics 365 CE Alınan data");
                    Console.WriteLine(retrievedData);

                    #endregion
                }
            }
            catch (CrmHttpResponseException crmHttpEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Dynamics 365 Web API işlemi sırasında bir hata oluştu : ");
                Console.WriteLine(crmHttpEx.Message);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Bir hata oluştu : ");
                Console.WriteLine(ex.Message);
            }


            Console.WriteLine("");
            Console.WriteLine("Uygulamayı kapatmak için bir tuşa basınız");
            Console.ReadLine();
        }
    }
}
