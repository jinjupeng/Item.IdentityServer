using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;

namespace Item.IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 直接看IdentityServer4官网，讲的最清楚（https://identityserver4.readthedocs.io/en/latest/quickstarts/1_client_credentials.html）
            //var builder = services.AddIdentityServer(options =>
            //{
            //    options.Events.RaiseErrorEvents = true;
            //    options.Events.RaiseInformationEvents = true;
            //    options.Events.RaiseFailureEvents = true;
            //    options.Events.RaiseSuccessEvents = true;
            //})
            var builder = services.AddIdentityServer()

            // in-memory, code config
            .AddTestUsers(InMemoryConfig.Users().ToList())
            .AddInMemoryApiResources(InMemoryConfig.GetApiResources())
            .AddInMemoryClients(InMemoryConfig.GetClients())
            .AddInMemoryApiScopes(InMemoryConfig.ApiScopes);


            if (Environment.IsDevelopment())
            {
                //This is for dev only scenarios when you don’t have a certificate to use.
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddControllers();

            //配置认证服务
            services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
            {
                // 意思是：直接相信"http://localhost:5000"的认证，而不用再直接实现一个认证了
                options.Authority = "http://localhost:5000";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // 添加IdentityServer中间件到Pipeline
            app.UseIdentityServer();

            // app.UseHttpsRedirection();

            app.UseRouting();

            // 先认证
            app.UseAuthentication();
            // 后授权
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
