using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Helpers;

namespace ConsoleClientLogin
{
    public class Program
    {
        static async Task Main()
        {
            Console.Title = "Console ResourceOwner Flow";

            var response = await RequestTokenAsync();
            if (response.IsError)
            {
                response.Error.ConsoleRed();
                return;
            }
            response.Show();

            Console.WriteLine("\nApi request\n");
            await CallServiceAsync(response.AccessToken);
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
            if (disco.IsError) throw new Exception(disco.Error);
            var token_request = new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,

                //ClientSecret = "secret",

                UserName = "admin",
                Password = "qwe123",

                ClientId = "console_login_client",
                GrantType = "password",
                Scope = "openid profile api1 offline_access",

            };
            var response = await client.RequestPasswordTokenAsync(token_request);

            return response;
        }

        static async Task CallServiceAsync(string token)
        {
            var baseAddress = Constants.SampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = await client.GetStringAsync(Constants.ApiController_1);

            "\n\nService claims:".ConsoleGreen();
            Console.WriteLine(JArray.Parse(response));
        }
    }
}
