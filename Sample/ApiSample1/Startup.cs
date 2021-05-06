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
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

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

            services.AddAuthentication()
               .AddJwtBearer(IdentityServerAuthenticationDefaults.AuthenticationScheme, config =>
                {
                    config.Authority = "https://localhost:44310";
                    config.Audience = "api1";
                    config.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.FromSeconds(5),
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("Scope","open_api");
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
            }
            public string Email { get; }
        }

        public class ScacRequirementHandler : AuthorizationHandler<ScacRequirement>
        {
            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScacRequirement requirement)
            {
                var hasClaim = context.User.HasClaim(x => x.Type == ClaimTypes.Email);
                if (!hasClaim)
                {
                    return Task.CompletedTask;
                }

                var email = context.User.FindFirst(x => x.Type == ClaimTypes.Email).Value;
                if (email.Split('@')[1].ToLower() == requirement.Email.ToLower())
                {
                    context.Succeed(requirement);
                }
                Debug.WriteLine(email);
                return Task.CompletedTask;
            }
        }
    }
}
