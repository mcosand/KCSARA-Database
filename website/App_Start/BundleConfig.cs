using System.Web;
using System.Web.Optimization;

namespace Kcsara.Database.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;
            
            bundles.Add(new ScriptBundle("~/script/core").Include(
                "~/Scripts/jquery-1.10.2.js",
                "~/Content/script/json2.js",
                "~/Content/script/errorHandling.js",
                "~/Scripts/jquery-ui-1.10.3.js",
                "~/Content/script/date.js",
                "~/Content/script/jquery.tablesorter.js",
                "~/Content/script/timepicker.js",
                "~/Scripts/knockout-2.3.0.js",
                "~/Scripts/knockout.mapping-latest.js",
                "~/Content/script/jquery.iframe-transport.js",
                "~/Content/script/jquery.fileupload.js",
                "~/Content/script/jquery.fileupload-process.js",
                "~/Content/script/jquery.fileupload-ui.js",
                "~/Content/script/jquery.fileupload-jquery-ui-kcsara.js",

                "~/Scripts/ViewModels.js",
                "~/Content/script/suggest.person.js",
                "~/Content/script/common.js"));            
            //bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            //            "~/Content/script/jquery-1.*"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
            //            "~/Content/script/jquery-ui*"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Content/script/jquery.unobtrusive*",
            //            "~/Content/script/jquery.validate*"));

            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Content/script/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/themes/base/jquery-ui").Include(
                "~/Content/themes/base/jquery.ui.core.css",
                "~/Content/themes/base/jquery.ui.resizable.css",
                "~/Content/themes/base/jquery.ui.selectable.css",
                "~/Content/themes/base/jquery.ui.accordion.css",
                "~/Content/themes/base/jquery.ui.autocomplete.css",
                "~/Content/themes/base/jquery.ui.button.css",
                "~/Content/themes/base/jquery.ui.dialog.css",
                "~/Content/themes/base/jquery.ui.slider.css",
                "~/Content/themes/base/jquery.ui.tabs.css",
                "~/Content/themes/base/jquery.ui.datepicker.css",
                "~/Content/themes/base/jquery.ui.progressbar.css",
                "~/Content/themes/base/jquery.ui.theme.css"
                ));
            bundles.Add(new StyleBundle("~/Content/site-style").Include(
                "~/Content/site.css",
                "~/Content/common.css",
                "~/Content/suggest.css",
                "~/Content/jquery.fileupload-ui.css"
                ));

            bundles.Add(new StyleBundle("~/style/print").Include(
                "~/Content/print-styles.css"));
        }
    }
}