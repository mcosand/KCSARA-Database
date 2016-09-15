/*
 * Copyright 2016 Matthew Cosand
 */
using System.Configuration;
using System.Web.Optimization;

namespace Kcsara.Database.Web
{
  public class BundleConfig
  {
    public static void RegisterBundles(BundleCollection bundles)
    {
      bool noMinify;
      if (!bool.TryParse(ConfigurationManager.AppSettings["no-minify"] ?? "false", out noMinify))
      {
        noMinify = false;
      }

      BundleTable.EnableOptimizations = !noMinify;

      bundles.Add(new ScriptBundle("~/wwwroot/js/core").Include(
        "~/wwwroot/lib/angular/angular.js",
        "~/wwwroot/lib/angular-animate/angular-animate.js",
        "~/wwwroot/lib/angular-aria/angular-aria.js",
        "~/wwwroot/lib/angular-messages/angular-messages.js",
        "~/wwwroot/lib/angular-material/angular-material.js",
        "~/wwwroot/js/sar-common.js",
        "~/wwwroot/js/directives/server-error.js",
        "~/wwwroot/js/site.js",
        "~/wwwroot/js/page/login.js",
        "~/wwwroot/js/page/registerlogin.js"
        ));

      bundles.Add(new StyleBundle("~/wwwroot/css/core").Include(
        "~/wwwroot/lib/angular-material/angular-material.css",
        "~/wwwroot/lib/font-awesome/css/font-awesome.css",
        "~/wwwroot/css/site.css",
        "~/wwwroot/css/vertical-stepper.css"
        ));
    }
  }
}
