using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace Service_To_Service
{
    public class Program
    {
        public const string Authority = "https://localhost:44310";
        private static async Task Main()
        {
            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(Authority);
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "service_to_service_id",
                ClientSecret = "secret",

                Scope = "service_to_service"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            //var apiClient = new HttpClient();
            //apiClient.SetBearerToken(tokenResponse.AccessToken);

            //var response = await apiClient.GetAsync("https://localhost:6001/identity");
            //if (!response.IsSuccessStatusCode)
            //{
            //    Console.WriteLine(response.StatusCode);
            //}
            //else
            //{
            //    var content = await response.Content.ReadAsStringAsync();
            //    Console.WriteLine(JArray.Parse(content));
            //}
        }
    }
}