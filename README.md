# IdentityServerApi.AdminUI
dotnet new -i Skoruba.IdentityServer4.Admin.Templates::2.0.1

dotnet new skoruba.is4admin --name IdentityServerApi --title MyProject --adminemail "a_platonenkov@ssj.irkut.com" --adminpassword "qwe123" --adminrole SuperAdmin --adminclientid IdentityServerApiId --adminclientsecret IdentityServerApiSecret --dockersupport true


### For Api
add nuget IdentityServer4.AccessTokenValidation

```C#
services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, config =>
    {
      config.Authority = "https://localhost:44310";
      config.Audience = "ApiName";
    }
    
services.AddAuthorization(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
    {
        //policy.RequireAuthenticatedUser();
        policy.RequireClaim("Scope","api1");
    });
});
```
To Enable Refresh Token:
- Allow Offline Access in client settings
- add scope - offline_access


### oidc Authentication

```C#
services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
    })
    .AddCookie("cookie")
    .AddOpenIdConnect("oidc", options =>
    {
	    options.Authority = "https://localhost:5000";
        options.ClientId = "oidcClient";
        options.ClientSecret = "SuperSecretPassword";
    
        options.ResponseType = "code";
        options.UsePkce = true;
        options.ResponseMode = "query";
    
        // options.CallbackPath = "/signin-oidc"; // default redirect URI
        
        // options.Scope.Add("oidc"); // default scope
        // options.Scope.Add("profile"); // default scope
        options.Scope.Add("api1.read");
        options.SaveTokens = true;
    });
```
