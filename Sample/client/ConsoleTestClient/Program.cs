using IdentityModel.Client;
using IdentityModel.OidcClient;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleTestClient
{
    class Program
    {
        private const string SampleApi = "https://localhost:44365";
        public const string Authority = "https://localhost:44310";
        static OidcClient _oidcClient;
        static HttpClient _apiClient = new HttpClient { BaseAddress = new Uri(SampleApi) };

        public static async Task Main()
        {
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("|  Sign in with OIDC    |");
            Console.WriteLine("+-----------------------+");
            Console.WriteLine("");
            Console.WriteLine("Press any key to sign in...");
            Console.ReadKey();

            await SignIn();
        }

        private static async Task SignIn()
        {
            // create a redirect URI using an available port on the loopback address.
            // requires the OP to allow random ports on 127.0.0.1 - otherwise set a static port
            var browser = new SystemBrowser(57637);
            string redirectUri = string.Format($"http://127.0.0.1:{browser.Port}");

            var options = new OidcClientOptions
            {
                Authority = Authority,

                ClientId = "console_pkce",

                RedirectUri = redirectUri,
                PostLogoutRedirectUri = redirectUri,
                Scope = "openid profile api1",
                FilterClaims = false,
                Browser = browser
            };

            var serilog = new LoggerConfiguration()
                .MinimumLevel.Error()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
                .CreateLogger();

            options.LoggerFactory.AddSerilog(serilog);

            _oidcClient = new OidcClient(options);
            var result = await _oidcClient.LoginAsync(new LoginRequest());

            ShowResult(result);
            await NextSteps(result);
        }

        private static void ShowResult(LoginResult result)
        {
            if (result.IsError)
            {
                Console.WriteLine("\n\nError:\n{0}", result.Error);
                return;
            }

            Console.WriteLine("\n\nClaims:");
            foreach (var claim in result.User.Claims)
            {
                Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
            }

            Console.WriteLine($"\nidentity token: {result.IdentityToken}");
            Console.WriteLine($"access token:   {result.AccessToken}");
            Console.WriteLine($"refresh token:  {result?.RefreshToken ?? "none"}");
        }

        private static async Task NextSteps(LoginResult result)
        {
            var currentAccessToken = result.AccessToken;
            var currentRefreshToken = result.RefreshToken;

            var menu = "  x...exit  c...call api   ";
            if (currentRefreshToken != null) menu += "r...refresh token   ";

            while (true)
            {
                Console.WriteLine("\n\n");

                Console.Write(menu);
                var key = Console.ReadKey();
                try
                {
                    if (key.Key == ConsoleKey.X)
                    {
                        await _oidcClient.LogoutAsync();
                        return;
                    }
                    if (key.Key == ConsoleKey.C) await CallApi(currentAccessToken);
                    if (key.Key == ConsoleKey.R)
                    {
                        var refreshResult = await _oidcClient.RefreshTokenAsync(currentRefreshToken);
                        if (result.IsError)
                        {
                            Console.WriteLine($"Error: {refreshResult.Error}");
                        }
                        else
                        {
                            currentRefreshToken = refreshResult.RefreshToken;
                            currentAccessToken = refreshResult.AccessToken;

                            Console.WriteLine("\n\n");
                            Console.WriteLine($"access token:   {result.AccessToken}");
                            Console.WriteLine($"refresh token:  {result?.RefreshToken ?? "none"}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static async Task CallApi(string currentAccessToken)
        {
            _apiClient.SetBearerToken(currentAccessToken);
            var response = await _apiClient.GetAsync("WeatherForecast");

            if (response.IsSuccessStatusCode)
            {
                var json = JArray.Parse(await response.Content.ReadAsStringAsync());
                Console.WriteLine("\n\n");
                Console.WriteLine(json);
            }
            else
            {
                Console.WriteLine($"Error: {response.ReasonPhrase}");
            }
        }
    }
}

