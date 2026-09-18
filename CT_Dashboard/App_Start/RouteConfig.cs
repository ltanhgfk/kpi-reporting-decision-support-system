using System.Web.Mvc;
using System.Web.Routing;

namespace CT_Dashboard
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Route cho trang chi tiết chỉ tiêu
            routes.MapRoute(
                name: "ChiTieuDetail",
                url: "Dashboard/ChiTieuDetail/{maChiTieu}",
                defaults: new { controller = "Dashboard", action = "ChiTieuDetail", maChiTieu = UrlParameter.Optional },
                constraints: new { maChiTieu = @"^[A-Za-z0-9_-]+$" } // Chỉ cho phép ký tự chữ, số, gạch dưới, gạch ngang
            );

            // Route cho trang chi tiết nhóm chỉ tiêu
            routes.MapRoute(
                name: "NhomChiTieuDetail",
                url: "Dashboard/NhomChiTieuDetail/{nhomChiTieu}",
                defaults: new { controller = "Dashboard", action = "NhomChiTieuDetail", nhomChiTieu = UrlParameter.Optional },
                constraints: new { nhomChiTieu = @"^[A-Za-z0-9_-]+$" }
            );

            // Route mặc định
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Dashboard", id = UrlParameter.Optional }
            );
        }
    }
}