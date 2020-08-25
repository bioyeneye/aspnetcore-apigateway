using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AuxApiGateway.Extensions
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                model.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name => model.Enum.Add(new OpenApiString($"{Convert.ToInt64(Enum.Parse(context.Type, name))} - {name}")));
            }
        }
    }
    public static class ServiceExtensions
    {
        public static IServiceCollection AddGatewayAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var microserviceSetting = configuration.GetSection(nameof(ApiGatewaySetting)).Get<ApiGatewaySetting>();
            services.AddAuthentication(sharedOptions =>
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
                    x.TokenValidationParameters = new TokenValidationParameters()
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
            return services;
        }
        public static IServiceCollection AddSwaggerDoc(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AutoMedSys Api Gateway",
                    Version = "v1",
                    License = new OpenApiLicense
                    {
                        Name = "Microsoft Licence",
                        Url = new Uri("https://automedsys.net/licence"),
                    },
                    Description = "Contents for patient services"
                });
                c.SchemaFilter<EnumSchemaFilter>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(XmlCommentsFilePath);
                c.EnableAnnotations();
            });
            services.AddSwaggerGenNewtonsoftSupport();
            return services;
        }

        static string XmlCommentsFilePath
        {
            get
            {
                //string basePath = PlatformServices.Default.Application.ApplicationBasePath;
                //string fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                //return Path.Combine(basePath, fileName);

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                return Path.Combine(AppContext.BaseDirectory, xmlFile);

            }
        }

    }
}
