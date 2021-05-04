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
using System.Linq;
using System.Threading.Tasks;
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

            services.AddAuthentication("Bearer")
               .AddJwtBearer("Bearer", config =>
                {
                    config.Authority = "https://localhost:44310";
                    config.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false, 
                    };

                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("api1");
                });
            });

            //services.AddCors(config =>
            //    config.AddPolicy("AllowAll",
            //        p => p.AllowAnyOrigin()
            //           .AllowAnyMethod()
            //           .AllowAnyHeader()
            //    ));
            //services.AddMvcCore()
            //   .AddAuthorization();

            //services.AddAuthentication("Bearer")
            //   .AddIdentityServerAuthentication(options =>
            //    {
            //        options.Authority = "https://localhost:44310";
            //        options.RequireHttpsMetadata = false;


            //        options.ApiSecret = "secret";
            //        options.ApiName = "api1";
            //    });
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
                   .RequireAuthorization("ApiPolicy"); ;
            });
        }
    }
}
