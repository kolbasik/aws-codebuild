using System;
using System.Collections.Generic;
using System.Reflection;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Lambda1
{
    public class Startup
    {
        public const string AppS3BucketKey = "AppS3Bucket";
        public static readonly string ApplicationName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RouteOptions>(options => options.AppendTrailingSlash = false);

            services.AddMvc();
            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new Info { Title = ApplicationName, Version = "v1" });
                    options.DescribeAllEnumsAsStrings();
                });

            // Pull in any SDK configuration from Configuration object
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            // Add S3 to the ASP.NET Core dependency injection framework.
            services.AddAWSService<Amazon.S3.IAmazonS3>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLambdaLogger(Configuration.GetLambdaLoggerOptions());

            if (env.IsProduction()) // NOTE: hotfix to adjust the api-gateway request to lambda request
            {
                app.Use((context, next) =>
                {
                    var proxyRequest = context.Items["APIGatewayRequest"] as APIGatewayProxyRequest;
                    if (proxyRequest != null)
                    {
                        context.Request.Path = new PathString("/" + proxyRequest.PathParameters["proxy"]);
                        context.Request.PathBase = new PathString($"/{proxyRequest.RequestContext.Stage}{proxyRequest.Resource.Substring(0, proxyRequest.Resource.Length - 9)}");
                    }
                    return next();
                });
            }

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = "/kolbasik",
                EnableDefaultFiles = true,
                FileProvider = new SwaggerUIFileProvider(new Dictionary<string, string>()),
                StaticFileOptions = { ContentTypeProvider = new FileExtensionContentTypeProvider() }
            });

            app.UseSwagger().UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/v1/swagger.json", ApplicationName);
            });
            app.UseMvcWithDefaultRoute();
        }
    }
}
