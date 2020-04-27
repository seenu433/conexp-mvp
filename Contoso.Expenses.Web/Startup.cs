using Contoso.Expenses.Common.Models;
using Contoso.Expenses.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contoso.Expenses.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(env.ContentRootPath)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                            .AddEnvironmentVariables();
            Configuration = builder.Build();
            //Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // see https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2
            string connectionString = Configuration["ConnectionStrings:DBConnectionString"];
            services.AddDbContext<ContosoExpensesWebContext>(options =>                    
                    options.UseMySQL(connectionString));

            services.Configure<ConfigValues>(Configuration.GetSection("ConfigValues"));

            services.AddSingleton<QueueInfo>(queueInfo =>
            {
                return new QueueInfo()
                {
                    ConnectionString = Configuration["ConnectionStrings:NATSConnectionString"],
                    QueueName = Configuration["TopicName"]
                };
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ContosoExpensesWebContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            context.Database.EnsureCreated();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();

        }
    }
}
