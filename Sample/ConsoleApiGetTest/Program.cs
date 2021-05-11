using System;
using System.Net.Http;
using System.Threading.Tasks;
using Helpers;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace ConsoleApiGetTest
{
    class Program
    {
        private static async Task Main()
        {
            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "Api1_client_id",
                ClientSecret = "secret",
                
                Scope = "service_to_service api1"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var apiClient = new HttpClient(){ BaseAddress = new Uri(Constants.SampleApi) };
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await apiClient.GetAsync(Constants.ApiController_1);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
