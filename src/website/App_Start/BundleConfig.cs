/*
 * Copyright 2012-2014 Matthew Cosand
 */
using System.Web;
using System.Web.Optimization;

namespace Kcsara.Database.Web
{
  public class BundleConfig
  {
    public static void RegisterBundles(BundleCollection bundles)
    {
      bundles.Add(new ScriptBundle("~/scripts/ng-core").Include(
        "~/Scripts/modernizr-*",
        "~/Scripts/jquery-{version}.js",
        "~/Scripts/bootstrap.js",
        "~/Scripts/bootstrap-dialog.js",
        "~/Scripts/moment.js",
        "~/Scripts/angular.js",
        "~/Scripts/angular-modal-service.js",
        "~/Scripts/np-autocomplete.js",
        "~/Scripts/app.js",
        "~/scripts/site/modules/*.js"
        ));

      bundles.Add(new ScriptBundle("~/scripts/ng-site").Include(
        "~/scripts/site/models/*.js",
        "~/scripts/site/directives/*.js",
        "~/scripts/site/services/*.js"
        ));

      bundles.Add(new ScriptBundle("~/scripts/ng-shim").Include(
        "~/scripts/knockout-{version}.js",
        "~/Content/script/jquery.tablesorter.js"
        ));

      bundles.Add(new ScriptBundle("~/script/core").Include(
          "~/Scripts/jquery-{version}.js",
          "~/Content/script/json2.js",
          "~/Content/script/errorHandling.js",
          "~/Scripts/jquery-ui-{version}.js",
          "~/Content/script/date.js",
          "~/Content/script/jquery.tablesorter.js",
          "~/Content/script/timepicker.js",
          "~/Scripts/knockout-{version}.js",
          "~/Scripts/knockout.mapping-latest.js",
          "~/Content/script/jquery.iframe-transport.js",
          "~/Content/script/jquery.fileupload.js",
          "~/Content/script/jquery.fileupload-process.js",
          "~/Content/script/jquery.fileupload-ui.js",
          "~/Content/script/jquery.fileupload-jquery-ui-kcsara.js",
          "~/Scripts/nprogress.js",
          "~/Scripts/modernizr-*",

          "~/Scripts/ViewModels.js",
          "~/Content/script/suggest.person.js",
          "~/Content/script/common.js"));


      bundles.Add(new StyleBundle("~/Content/themes/base/jquery-ui").Include(
          "~/Content/themes/base/core.css",
          "~/Content/themes/base/resizable.css",
          "~/Content/themes/base/selectable.css",
          "~/Content/themes/base/accordion.css",
          "~/Content/themes/base/autocomplete.css",
          "~/Content/themes/base/button.css",
          "~/Content/themes/base/dialog.css",
          "~/Content/themes/base/slider.css",
          "~/Content/themes/base/tabs.css",
          "~/Content/themes/base/datepicker.css",
          "~/Content/themes/base/progressbar.css",
          "~/Content/themes/base/theme.css"
          ));
      bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
        "~/Content/bootstrap.css",
        "~/Content/bootstrap-dialog.css",
        "~/Content/np-autocomplete.min.css",
        "~/Content/font-awesome.css",
        "~/Content/AdminLTE.css",
        "~/Content/skins/skin-yellow.css",
        "~/Content/site-next.css"
        ));

      bundles.Add(new StyleBundle("~/Content/site-style").Include(
          "~/Content/site.css",
          "~/Content/common.css",
          "~/Content/suggest.css",
          "~/Content/jquery.fileupload-ui.css",
          "~/Content/nprogress.css",
          "~/Content/font-awesome.css"
          ));

      bundles.Add(new StyleBundle("~/style/print").Include(
          "~/Content/print-styles.css"));

      bundles.Add(new StyleBundle("~/scripts/leaflet").Include(
        "~/scripts/lib/leaflet.js",
        "~/scripts/lib/TileLayer.GeoJSON.js",
        "~/scripts/lib/leaflet.google.js"
       ));
    }
  }
}
