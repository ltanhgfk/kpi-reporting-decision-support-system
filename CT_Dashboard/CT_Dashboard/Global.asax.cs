using OfficeOpenXml;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace CT_Dashboard
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            ExcelPackage.License.SetNonCommercialPersonal("Luong The Anh");
            //OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            //ExcelPackage.License = new EPPlusLicense
            //{
            //    LicenseType = LicenseType.NonCommercial,
            //    Licensee = "Luong The Anh"
            //};

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            
            //AreaRegistration.RegisterAllAreas();
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
