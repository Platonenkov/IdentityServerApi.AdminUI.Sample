using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Console = System.Console;

namespace ApiSample1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            #region With oidc client

            services.AddAuthentication(
                    o =>
                    {
                        o.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                        o.DefaultChallengeScheme = "oidc";
                    })
               //.AddCookie(IdentityServerAuthenticationDefaults.AuthenticationScheme)
               .AddOpenIdConnect("oidc", config =>
                {
                    config.Authority = "https://localhost:44310";
                    config.ClientId = "api1";

                    config.ResponseType = "code";
                    config.Scope.Add("openid profile email address roles");
                    config.GetClaimsFromUserInfoEndpoint = true;
                    config.ClaimActions.MapAll();
                })
               .AddJwtBearer(IdentityServerAuthenticationDefaults.AuthenticationScheme, config =>
                {
                    config.Authority = "https://localhost:44310";
                    config.Audience = "api1";
                    //config.TokenValidationParameters = new TokenValidationParameters
                    //{
                    //    ClockSkew = TimeSpan.FromSeconds(5),
                    //};
                    config.SaveToken = true;
                    config.Configuration = new OpenIdConnectConfiguration()
                    {
                        AuthorizationEndpoint = "https://localhost:44310",
                    };
                });

            #endregion

            //services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            //   .AddJwtBearer(IdentityServerAuthenticationDefaults.AuthenticationScheme, config =>
            //    {
            //    config.Authority = "https://localhost:44310";
            //        config.Audience = "api1";
            //        //config.TokenValidationParameters = new TokenValidationParameters
            //        //{
            //        //    ClockSkew = TimeSpan.FromSeconds(5),
            //        //};
            //    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("Scope", "open_api");
                });
                options.AddPolicy("ScacEmail", builder =>
                {
                    builder.AddRequirements(new ScacRequirement("ssj.irkut.com"));
                });
            });
            services.AddSingleton<AuthorizationHandler<ScacRequirement>, ScacRequirementHandler>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiSample1", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ApiSample1 v1"));
            }
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                   /*.RequireAuthorization("ApiPolicy"); */;
            });
        }
        public class ScacRequirement : IAuthorizationRequirement
        {
            public ScacRequirement(string email)
            {
                Email = email;
                Log.Information("Initialize Requirement by Scac Email");
            }
            public string Email { get; }
        }

        public class ScacRequirementHandler: AuthorizationHandler<ScacRequirement>
        {
            private readonly IHttpContextAccessor _HttpContextAccessor;

            public ScacRequirementHandler(IHttpContextAccessor http_context_accessor) { _HttpContextAccessor = http_context_accessor; }
            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScacRequirement requirement)
            {
                Log.Information("Handle Requirement check start");
                
                var token = _HttpContextAccessor?.HttpContext?.GetTokenAsync("access_token");
                if (token?.Result != null)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                Log.Information("token is null");

                var name = context.User.Identity?.Name ?? context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                if (name != null)
                {
                    if (name.ToLower() == "admin")
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }

                    Log.Information($"Name is not admin : {name}");

                }
                Log.Information("name is null");

                var hasClaim = context.User.HasClaim(x => x.Type == ClaimTypes.Email);
                if (!hasClaim)
                {
                    Log.Information("No email claim");

                    return Task.CompletedTask;
                }

                var email = context.User.FindFirst(x => x.Type == ClaimTypes.Email).Value;
                Log.Information(email);

                if (email.Split('@')[1].ToLower() == requirement.Email.ToLower())
                {
                    context.Succeed(requirement);
                }
                return Task.CompletedTask;
            }
        }
    }
}
