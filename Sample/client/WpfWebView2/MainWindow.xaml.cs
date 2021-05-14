using IdentityModel.OidcClient;
using System;
using System.Linq;
using System.Windows;
using Helpers;
using IdentityModel.OidcClient.Browser;

namespace WpfWebView2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OidcClientOptions _Options;
        private OidcClient _Client;
        private LoginResult _LoginResult;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var options = new OidcClientOptions()
            {
                Authority = Constants.Authority,

                ClientId = "wpf_sample_id",
                Scope = "openid profile api1 email offline_access",

                RedirectUri = "http://127.0.0.1/sample-wpf-app",
                Browser = new WpfEmbeddedBrowser(),
                Policy = new Policy
                {
                    RequireIdentityTokenSignature = false
                }
            };
            _Options = options;
        }

        private async void Button_login_OnClick(object Sender, RoutedEventArgs E)
        {
            if (_Options is null)
            {
                MessageBox.Show(this, "Options is not correct");
                return;
            }
            _Client = new OidcClient(_Options);

            try
            {
                _LoginResult = await _Client.LoginAsync();
            }
            catch (Exception exception)
            {
                txbMessage.Text = $"Unexpected Error: {exception.Message}";
                Clean();
                return;
            }

            if (_LoginResult.IsError)
            {
                txbMessage.Text = _LoginResult.Error == "UserCancel" ? "The sign-in window was closed before authorization was completed." : _LoginResult.Error;
                Clean();
            }
            else
            {
                var name = _LoginResult.User.Identity.Name;
                var email = _LoginResult.User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                txbMessage.Text = $"You are login as\t{name}\t{email}";
                RefreshToken.Text = _LoginResult.RefreshToken;
                AccessToken.Text = _LoginResult.AccessToken;
                IdentityToken.Text = _LoginResult.IdentityToken;
                UserInfo.ItemsSource = await Helpers.Service.GetUserInfoAsync(_LoginResult.AccessToken);
            }

        }

        private async void Button_logout_OnClick(object Sender, RoutedEventArgs E)
        {
            if (!CheckClient())
                return;

            var logout = await _Client.LogoutAsync(new LogoutRequest() { BrowserDisplayMode = DisplayMode.Hidden });
            if (logout.IsError)
            {
                txbMessage.Text = logout.Error;
                return;
            }
            Clean();
        }
        private void Clean()
        {
            txbMessage.Text = "Unauthorized";
            RefreshToken.Text = string.Empty;
            AccessToken.Text = string.Empty;
            IdentityToken.Text = string.Empty;
            AccessPoliceApiTxt.Text = string.Empty;
            AccessBaseApiTxt.Text = string.Empty;
            UserInfo.ItemsSource = null;
            _Client = null;
            _LoginResult = null;
        }

        private async void Button_LoadAccessPoliceApi_OnClick(object Sender, RoutedEventArgs E)
        {
            var response = await Service.CallServiceAsync(AccessToken.Text, Helpers.Constants.ApiController_2);
            AccessPoliceApiTxt.Text = response;
        }

        private async void Button_LoadAccessBaseApi_OnClick(object Sender, RoutedEventArgs E)
        {
            var response = await Service.CallServiceAsync(AccessToken.Text, Helpers.Constants.ApiController_1);
            AccessBaseApiTxt.Text = response;
        }

        private async void Refresh_token_Click(object Sender, RoutedEventArgs E)
        {
            if (!CheckClient()) return;

            var refreshResult = await _Client.RefreshTokenAsync(RefreshToken.Text);
            if (_LoginResult.IsError)
            {
                Console.WriteLine($"Error: {refreshResult.Error}");
            }
            else
            {
                RefreshToken.Text = refreshResult.RefreshToken;
                AccessToken.Text = refreshResult.AccessToken;

                Console.WriteLine("\n\n");
                Console.WriteLine($"access token:   {_LoginResult.AccessToken}");
                Console.WriteLine($"refresh token:  {_LoginResult?.RefreshToken ?? "none"}");
            }

        }

        private bool CheckClient(bool showMessage = true)
        {
            if (_Client is not null) return true;
            if(showMessage)
                MessageBox.Show(this, "first connect to the server");
            return false;

        }
    }
}
