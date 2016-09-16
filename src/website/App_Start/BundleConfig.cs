
using System.Web.Optimization;

namespace Kcsara.Database.Web
{
  public class BundleConfig
  {
    public static void RegisterBundles(BundleCollection bundles)
    {
      bundles.Add(new ScriptBundle("~/wwwroot/js/auth-core").Include(
        "~/wwwroot/lib/angular/angular.js",
        "~/wwwroot/lib/angular-animate/angular-animate.js",
        "~/wwwroot/lib/angular-aria/angular-aria.js",
        "~/wwwroot/lib/angular-messages/angular-messages.js",
        "~/wwwroot/lib/angular-material/angular-material.js",
        "~/wwwroot/js/auth/sar-common.js",
        "~/wwwroot/js/auth/directives/server-error.js",
        "~/wwwroot/js/auth/site.js",
        "~/wwwroot/js/auth/page/login.js",
        "~/wwwroot/js/auth/page/registerlogin.js"
        ));

      bundles.Add(new StyleBundle("~/wwwroot/css/auth-core").Include(
        "~/wwwroot/lib/angular-material/angular-material.css",
        "~/wwwroot/lib/font-awesome/css/font-awesome.css",
        "~/wwwroot/css/auth/site.css",
        "~/wwwroot/css/auth/vertical-stepper.css"
        ));

      bundles.Add(new ScriptBundle("~/js/am.js").Include(
        "~/wwwroot/lib/angular/angular.js",
        "~/wwwroot/lib/angular-ui-router/release/angular-ui-router.js",
        "~/wwwroot/lib/angular-aria/angular-aria.js",
        "~/wwwroot/lib/angular-animate/angular-animate.js",
        "~/wwwroot/lib/angular-messages/angular-messages.js",
        "~/wwwroot/lib/angular-material/angular-material.js",
        "~/wwwroot/lib/oidc-client/dist/oidc-client.js",
        "~/wwwroot/js/site.js",
        "~/wwwroot/js/routes.js",
        "~/wwwroot/js/services/*.js",
        "~/wwwroot/js/directives/*.js"
        ));

      bundles.Add(new StyleBundle("~/css/am.css").Include(
        "~/wwwroot/lib/angular-material/angular-material.css",
        "~/wwwroot/css/site.css",
        "~/wwwroot/css/main-nav.css"
        ));

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
        "~/scripts/site/modules/*.js",
        "~/scripts/ng-file-upload.js"
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
