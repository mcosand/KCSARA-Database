/*
 * Copyright 2015-2016 Matthew Cosand
 */
namespace website
{
  using System;
  using System.IO;
  using Kcsar.Database.Model;
  using Kcsar.Database.Model.Events;
  using Kcsara.Database.Web;
  using Kcsara.Database.Web.Models;
  using Kcsara.Database.Web.Services;
  using log4net;
  using Microsoft.AspNet.Builder;
  using Microsoft.AspNet.Hosting;
  using Microsoft.AspNet.Http;
  using Microsoft.AspNet.Identity.EntityFramework;
  using Microsoft.AspNet.Mvc.ApplicationModels;
  using Microsoft.AspNet.Mvc.Formatters;
  using Microsoft.Data.Entity;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using website.Models;
  using website.Services;

  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      // Set up configuration sources.
      var builder = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json")
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
          .AddJsonFile($"appsettings.local.json", optional: true);

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
      services.AddSingleton<ICurrentPrincipalProvider, CurrentPrincipalProvider>();

      services.AddTransient<Lazy<IKcsarContext>, Lazy<IKcsarContext>>(svc => new Lazy<IKcsarContext>(() => new KcsarContext(Configuration["Data:DataStore:ConnectionString"], () => svc.GetService<ICurrentPrincipalProvider>().CurrentPrincipal.Identity.Name)));
      services.AddSingleton<Func<IKcsarContext>, Func<IKcsarContext>>(svc => () => new KcsarContext(Configuration["Data:DataStore:ConnectionString"], () => svc.GetService<ICurrentPrincipalProvider>().CurrentPrincipal.Identity.Name));
      services.AddSingleton(svc => LogManager.GetLogger("log"));
      services.AddSingleton(svc => new Lazy<IEncryptionService>(() => new EncryptionService(Configuration["encryptKey"])));

      services.AddSingleton(svc => new Lazy<IMembersService>(() => new MembersService(
        svc.GetRequiredService<Func<IKcsarContext>>(),
        svc.GetRequiredService<Lazy<IEncryptionService>>(),
        svc.GetRequiredService<ICurrentPrincipalProvider>(),
        svc.GetRequiredService<ILog>())));
      services.AddSingleton(svc => new Lazy<IEventsService<Mission>>(() => new MissionsService(svc.GetRequiredService<Func<IKcsarContext>>(), svc.GetRequiredService<ILog>())));
      services.AddSingleton(svc => new Lazy<IEventsService<EventSummary>>(() => new EventsService<TrainingRow, EventSummary>(svc.GetRequiredService<Func<IKcsarContext>>(), svc.GetRequiredService<ILog>())));
      services.AddSingleton(svc => new Lazy<IUnitsService>(() => new UnitsService(svc.GetRequiredService<Func<IKcsarContext>>(), svc.GetRequiredService<ILog>())));
      services.AddSingleton(svc => new Lazy<IAnimalsService>(() => new AnimalsService(svc.GetRequiredService<Func<IKcsarContext>>(), svc.GetRequiredService<ILog>())));
      services.AddSingleton(svc => new Lazy<ITrainingService>(() => new TrainingService(svc.GetRequiredService<Func<IKcsarContext>>(), svc.GetRequiredService<ILog>())));

      services.AddSingleton(svc => new Lazy<IDocumentsService>(() => new DocumentsService(
        svc.GetRequiredService<Func<IKcsarContext>>(),
        svc.GetRequiredService<IHostingEnvironment>(),
        svc.GetRequiredService<ILog>())));

      services.AddTransient<IApplicationModelProvider>(svc => new CustomFilterApplicationModelProvider());

      var keystore = Configuration["KeyStorePath"];
      if (!string.IsNullOrWhiteSpace(keystore))
      {
        services.AddDataProtection();
        services.ConfigureDataProtection(configure =>
        {
          configure.PersistKeysToFileSystem(new DirectoryInfo(keystore));
        });
      }
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

      //app.UseCookieAuthentication(options =>
      //{
      //  options.LoginPath = new PathString("/account/login");
      //  options.AutomaticAuthenticate = true;
      //  options.AutomaticChallenge = true;
      //  options.AuthenticationScheme = "Microsoft.AspNet.Identity.Application";
      //});

      app.UseIdentity();
      app.UseClaimsTransformation(o => o.Transformer = new MemberIdClaimsTransformer(app.ApplicationServices.GetRequiredService<Func<IKcsarContext>>()));

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
