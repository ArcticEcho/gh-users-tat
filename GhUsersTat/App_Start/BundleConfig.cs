using System.Web;
using System.Web.Optimization;

namespace GhUsersTat
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles
                .Add(new ScriptBundle("~/bundles/jquery")
                .Include("~/Scripts/jquery/jquery-{version}.js"));

            bundles
                .Add(new ScriptBundle("~/bundles/jqueryval")
                .Include("~/Scripts/jquery.validate/jquery.validate*"));

            bundles
                .Add(new Bundle("~/bundles/bootstrap")
                .Include("~/Scripts/bootstrap/bootstrap.js"));

            bundles
                .Add(new StyleBundle("~/Content/css")
                .Include("~/Content/bootstrap/bootstrap.css", "~/Content/main.min.css"));
        }
    }
}
