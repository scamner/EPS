using EPS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EPS.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.Disabled)]
    [OutputCache(NoStore = true, Duration = 0)]
    public class CheckSessionController : Controller
    {
        Utilities util = new Utilities();

        [AllowAnonymous]
        public JsonResult CheckLoginTimeout()
        {
            if (Request.Cookies[FormsAuthentication.FormsCookieName] == null) return Json(0, JsonRequestBehavior.AllowGet);

            //This line is VERY important to keep this call from updating the timeout on the cookie.
            Response.Cookies.Remove(FormsAuthentication.FormsCookieName);

            var cookie = Request.Cookies[FormsAuthentication.FormsCookieName].Value;
            var ticket = FormsAuthentication.Decrypt(cookie);
            var secondsRemaining = Convert.ToInt32(Math.Round((ticket.Expiration - DateTime.Now).TotalSeconds, 0));
            return Json(secondsRemaining, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ResetTimeout()
        {
            try
            {
                String Username = HttpContext.User.Identity.Name;

                FormsAuthenticationTicket tkt;
                string cookiestr;
                HttpCookie ck;
                tkt = new FormsAuthenticationTicket(1, Username, DateTime.Now, DateTime.Now.AddMinutes((int)FormsAuthentication.Timeout.TotalMinutes), true, Username);
                cookiestr = FormsAuthentication.Encrypt(tkt);
                ck = new HttpCookie(FormsAuthentication.FormsCookieName, cookiestr);
                ck.Path = FormsAuthentication.FormsCookiePath;
                Response.Cookies.Add(ck);

                return Json(new { Error = "", SessionTimeout = (int)FormsAuthentication.Timeout.TotalSeconds }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string errMessage = util.ParseError(ex);
                return Json(new { Error = errMessage }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}