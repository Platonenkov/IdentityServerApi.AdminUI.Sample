using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace Helpers
{
    public static class Service
    {
        public static async Task GetUserInfoConsoleAsync(string accessToken)
        {
                foreach (var (claim, value) in await GetUserInfoAsync(accessToken))
                    $"{claim} : {value}".ConsoleYellow();
        }

        public static async Task<Dictionary<string,string>> GetUserInfoAsync(string accessToken)
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var user_data = await client.GetUserInfoAsync(
                new UserInfoRequest() { Address = disco.UserInfoEndpoint, Token = accessToken });

            if (!user_data.IsError)
                return user_data.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);
           
            user_data.Error.ConsoleRed();
            return new Dictionary<string, string>();

        }

        public static async Task<string> CallServiceAsync(string token, string controller)
        {
            var baseAddress = Constants.SampleApi;

            var client = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);

            try
            {
                var response = await client.GetStringAsync(controller);
                return response;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }
        public static async Task CallServiceConsoleAsync(string token)
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
