using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EPS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        public static string getCurrentRoot()
        {
            string url = HttpContext.Current.Request.ApplicationPath;
            return url == "/" ? "/" : url + "/";
        }
    }
}
