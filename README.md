# IdentityServerApi.AdminUI
dotnet new -i Skoruba.IdentityServer4.Admin.Templates::2.0.1

dotnet new skoruba.is4admin --name IdentityServerApi --title MyProject --adminemail "a_platonenkov@ssj.irkut.com" --adminpassword "qwe123" --adminrole SuperAdmin --adminclientid IdentityServerApiId --adminclientsecret IdentityServerApiSecret --dockersupport true


### For Api
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, config =>
    {
      config.Authority = "https://localhost:44310";
      config.Audience = "ApiName";
    }
