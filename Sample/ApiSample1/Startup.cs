using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;

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
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder =>
                        builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()

                );
            }); services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            #region With oidc client

            //services.AddAuthentication(
            //        o =>
            //        {
            //            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //            o.DefaultChallengeScheme = "oidc";
            //        })
            //    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            //   .AddOpenIdConnect(
            //        "oidc", config =>
            //        {
            //            config.Authority = "https://localhost:44310";
            //            config.ClientId = "api1";

            //            config.ResponseType = "code";
            //            config.Scope.Add("openid profile email");
            //            config.SaveTokens = true;
            //            config.GetClaimsFromUserInfoEndpoint = true;
            //            config.ClaimActions.MapAll();
            //        });
            //.AddJwtBearer(IdentityServerAuthenticationDefaults.AuthenticationScheme, config =>
            // {
            //     config.Authority = "https://localhost:44310";
            //     config.Audience = "api1";
            //     //config.TokenValidationParameters = new TokenValidationParameters
            //     //{
            //     //    ClockSkew = TimeSpan.FromSeconds(5),
            //     //};
            //     config.SaveToken = true;
            //     config.Configuration = new OpenIdConnectConfiguration()
            //     {
            //         AuthorizationEndpoint = "https://localhost:44310",
            //     };
            // });

            #endregion

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
               .AddJwtBearer(IdentityServerAuthenticationDefaults.AuthenticationScheme, config =>
                {
                    config.Authority = "https://localhost:44310";
                    config.Audience = "api1";
                    //config.TokenValidationParameters = new TokenValidationParameters
                    //{
                    //    ClockSkew = TimeSpan.FromSeconds(5),
                    //};
                    config.SaveToken = true;
                    config.RequireHttpsMetadata = false;
                });



            services.AddAuthorization();

            services.AddSingleton<IAuthorizationHandler, ScacRequirementHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();

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
            app.UseCors("CorsPolicy");

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


        #region Policy

        /// <summary>
        /// Провайдер политик, регистрирует и проверяет наличие
        /// </summary>
        public class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
        {
            private readonly AuthorizationOptions _Options;

            public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { _Options = options.Value; }

            #region Overrides of DefaultAuthorizationPolicyProvider

            public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
            {
                var policy_exist = await base.GetPolicyAsync(policyName);
                if (policy_exist == null)
                {
                    if(policyName == "ScacEmailPolicy")
                    {
                        policy_exist = new AuthorizationPolicyBuilder().AddRequirements(new ScacRequirement("ssj.irkut.com")).Build();
                        _Options.AddPolicy(policyName, policy_exist);
                    }
                    if(policyName == "ApiPolicy")
                    {
                        policy_exist = new AuthorizationPolicyBuilder().RequireClaim("Scope", "api1").Build();
                        _Options.AddPolicy(policyName, policy_exist);
                    }
                }

                return policy_exist;
            }

            #endregion
        }
        /// <summary>
        /// Описывает модель требования
        /// </summary>
        public class ScacRequirement : IAuthorizationRequirement
        {
            public ScacRequirement(string email)
            {
                Email = email;
                Log.Information("Initialize Requirement by Scac Email");
            }
            public string Email { get; }
        }
        /// <summary>
        /// Описывает обработчик требования политики 
        /// </summary>
        public class ScacRequirementHandler: AuthorizationHandler<ScacRequirement>
        {
            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScacRequirement requirement)
            {
                Log.Information("Handle Requirement check start");
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

        #endregion
    }
}
