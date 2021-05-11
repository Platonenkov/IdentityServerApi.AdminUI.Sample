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

            "Access Token".ConsoleYellow();
            await CallServiceAsync(response.AccessToken);

            "Identity Token".ConsoleYellow();
            await CallServiceAsync(response.IdentityToken);
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
            if (disco.IsError) throw new Exception(disco.Error);
            var token_request = new PasswordTokenRequest()
            {
                Address = disco.TokenEndpoint,
                
                //ClientSecret = "secret",

                UserName = "admin",
                Password = "qwe123",

                ClientId = "console_login_client",
                GrantType = "password",
                Scope = "openid email profile api1 offline_access",
            };

            var response = await client.RequestPasswordTokenAsync(token_request);
            var user_data = await client.GetUserInfoAsync(
                new UserInfoRequest() { Address = disco.UserInfoEndpoint, Token = response.AccessToken });
            if(user_data.IsError)
                user_data.Error.ConsoleRed();
            else
            {
                foreach (var claim in user_data.Claims)
                    $"{claim.Type} : {claim.Value}".ConsoleYellow();
            }
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

            try
            {
                var response = await client.GetStringAsync(Constants.ApiController_1);
                "\n\nService claims:".ConsoleGreen();
                Console.WriteLine(JArray.Parse(response));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            try
            {
                Console.WriteLine("\n Запрос в защищённую зону\n");
                var response_2 = await client.GetStringAsync(Constants.ApiController_2);
                "\n\nService claims:".ConsoleGreen();
                Console.WriteLine(JArray.Parse(response_2));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
