using IdentityModel.OidcClient;
using System;
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

                Scope = "openid profile api1 offline_access email",

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

            LoginResult loginResult;
            try
            {
                loginResult = await _Client.LoginAsync();
            }
            catch (Exception exception)
            {
                txbMessage.Text = $"Unexpected Error: {exception.Message}";
                Clean();
                return;
            }

            if (loginResult.IsError)
            {
                txbMessage.Text = loginResult.Error == "UserCancel" ? "The sign-in window was closed before authorization was completed." : loginResult.Error;
                Clean();
            }
            else
            {
                txbMessage.Text = loginResult.User.Identity.Name;
                RefreshToken.Text = loginResult.RefreshToken;
                AccessToken.Text = loginResult.AccessToken;
                IdentityToken.Text = loginResult.IdentityToken;
                UserInfo.ItemsSource = await Helpers.Service.GetUserInfoAsync(loginResult.AccessToken);
            }

        }

        private async void Button_logout_OnClick(object Sender, RoutedEventArgs E)
        {
            if (_Client is null)
            {
                MessageBox.Show(this, "First you must connect to server");
                return;
            }

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
            UserInfo.ItemsSource = null;
            _Client = null;

        }

        private async void Button_LoadFromBaseApi_OnClick(object Sender, RoutedEventArgs E)
        {
            var response = await Service.CallServiceAsync(AccessToken.Text, Helpers.Constants.ApiController_1);
            BaseApiTxt.Text = response;
        }
        private async void Button_LoadFromPoliceApi_OnClick(object Sender, RoutedEventArgs E)
        {
            var response = await Service.CallServiceAsync(IdentityToken.Text, Helpers.Constants.ApiController_2);
            PoliceApiTxt.Text = response;
        }
    }
}
