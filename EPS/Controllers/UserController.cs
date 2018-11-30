using DataLayer;
using EPS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EPS.Controllers
{
    public class UserController : Controller
    {
        DataLayer.EPSEntities db = new DataLayer.EPSEntities();
        Models.Utilities util = new Models.Utilities();

        public ActionResult Index()
        {
            return View(new UserModel());
        }

        public ActionResult CheckPassword(String Username, String Password)
        {
            try
            {
                User user = db.Users.Where(u => u.Username == Username).FirstOrDefault();
                Boolean isGood = false;
                String Domain = db.Parameters.AsNoTracking().Where(p => p.ParamName == "ADDomain").FirstOrDefault().ParamValue;

                if (user != null)
                {
                    using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, Domain))
                    {
                        isGood = pc.ValidateCredentials(Username, Password);
                    }
                }

                if (isGood == false)
                {
                    return Content("<script type='text/javascript'>LoginFailed();</script>");
                }
                else
                {
                    System.Web.HttpContext.Current.Session["CurrentUser"] = user;

                    WriteLogonCookie(user);

                    return JavaScript("window.location='" + Url.Action("Index", "Dashboard") + "'");
                }
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("User/CheckPassword", ex, null);

                return Content(String.Format("<script type='text/javascript'>ShowMessage('{0}', 'show');</script>", error));
            }
        }

        public void WriteLogonCookie(User user)
        {
            FormsAuthenticationTicket tkt;
            string cookiestr;
            HttpCookie ck;
            tkt = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddMinutes((int)FormsAuthentication.Timeout.TotalMinutes), true, user.Username);
            cookiestr = FormsAuthentication.Encrypt(tkt);
            ck = new HttpCookie(FormsAuthentication.FormsCookieName, cookiestr);
            ck.Path = FormsAuthentication.FormsCookiePath;
            Response.Cookies.Add(ck);

            Login login = new Login { LoginDate = DateTime.Now, UserID = user.UserID };
            db.Logins.Add(login);
            db.SaveChanges();
        }

        public ActionResult Logout()
        {
            try
            {
                Session.Clear();
                Session.RemoveAll();
                Session.Abandon();

                FormsAuthentication.SignOut();

                return View();
            }
            catch (Exception ex)
            {
                String error = util.ParseError(ex);
                util.WriteErrorToLog("User/Logout", ex, null);

                return Content(String.Format("<script type='text/javascript'>alert('{0}');</script>", error));
            }
        }
    }
}