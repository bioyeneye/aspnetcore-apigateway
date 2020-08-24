using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;

namespace AuxApiGateway
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
            services.AddControllers();
            var microserviceSetting = Configuration.GetSection(nameof(ApiGatewaySetting)).Get<ApiGatewaySetting>();
            services.AddAuthentication(
                    sharedOptions =>
                    {
                        sharedOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        sharedOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
                {
                    x.Authority = microserviceSetting.Authority;
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidateIssuer = microserviceSetting.ValidateIssuer,
                        ValidateAudience = microserviceSetting.ValidateAudience,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = microserviceSetting.ValidateIssuerSigningKey,
                        ValidIssuer = microserviceSetting.Authority,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(microserviceSetting.Secret)),
                        ClockSkew = TimeSpan.FromMinutes(5),
                        ValidAudiences = microserviceSetting.Audience.Split(',')
                        // ValidAudiences = new[] {"orders", "basket", "locations", "marketing", "mobileshoppingagg", "webshoppingagg"}
                    };
                    x.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("OnAuthenticationFailed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("OnTokenValidated: " + context.SecurityToken);
                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddOcelot();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}