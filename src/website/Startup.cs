using System;
using Kcsar.Database.Model;
using Kcsara.Database.Web.Services;
using log4net;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using website.Models;
using website.Services;

namespace website
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      // Set up configuration sources.
      var builder = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json")
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      if (env.IsDevelopment())
      {
        // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
        builder.AddUserSecrets();
      }

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; set; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // Add framework services.
      services.AddEntityFramework()
          .AddSqlServer()
          .AddDbContext<ApplicationDbContext>(options =>
              options.UseSqlServer(Configuration["Data:AuthStore:ConnectionString"]));

      //services.AddTransient<IKcsarContext, KcsarContext>(svc => new KcsarContext(Configuration["Data:DataStore:ConnectionString"]));
      services.AddTransient<Lazy<IKcsarContext>, Lazy<IKcsarContext>>(svc => new Lazy<IKcsarContext>(() => new KcsarContext(Configuration["Data:DataStore:ConnectionString"])));
      services.AddSingleton<Func<IKcsarContext>, Func<IKcsarContext>>(svc => () => new KcsarContext(Configuration["Data:DataStore:ConnectionString"]));
      services.AddSingleton(svc => LogManager.GetLogger("log"));

      services.AddSingleton(svc => new Lazy<IEventsService<Kcsara.Database.Web.Models.Mission>>(() => new MissionsService(svc.GetRequiredService<Func<IKcsarContext>>(), svc.GetRequiredService<ILog>())));
      services.AddSingleton(svc => new Lazy<IEventsService<Kcsara.Database.Web.Models.EventSummary>>(() => new EventsService<TrainingRow, Kcsara.Database.Web.Models.EventSummary>(svc.GetRequiredService<Func<IKcsarContext>>(), svc.GetRequiredService<ILog>())));

      services.AddIdentity<ApplicationUser, IdentityRole>()
          .AddUserManager<ApplicationUserManager>()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

      services.AddMvc(options =>
      {
        var jsonOutputFormatter = new JsonOutputFormatter();
        Kcsara.Database.Web.Utils.DecorateJsonSettings(jsonOutputFormatter.SerializerSettings);
        options.OutputFormatters.Insert(0, jsonOutputFormatter);
        var inputFormatter = new JsonInputFormatter();
        Kcsara.Database.Web.Utils.DecorateJsonSettings(inputFormatter.SerializerSettings);
        options.InputFormatters.Insert(0, inputFormatter);
      });

      // Add application services.
      services.AddTransient<IEmailSender, AuthMessageSender>();
      services.AddTransient<ISmsSender, AuthMessageSender>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      if (env.IsDevelopment())
      {
        app.UseBrowserLink();
        app.UseDeveloperExceptionPage();
        app.UseDatabaseErrorPage();
      }
      else
      {
        app.UseExceptionHandler("/Home/Error");

        // For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
        try
        {
          using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
              .CreateScope())
          {
            serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
                 .Database.Migrate();
          }
        }
        catch { }
      }

      app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

      app.UseStaticFiles();

      app.UseIdentity();

      // To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

      app.UseMvc(routes =>
      {
        routes.MapRoute(
                  name: "default",
                  template: "{controller=Home}/{action=Index}/{id?}");
      });
    }

    // Entry point for the application.
    public static void Main(string[] args) => WebApplication.Run<Startup>(args);
  }
}
