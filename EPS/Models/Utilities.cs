using DataLayer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace EPS.Models
{
    public class Utilities
    {
        public string ParseError(Exception Error)
        {
            string retMessage = GetErrorMessage(Error.Message);

            Exception innerErr = Error;
            while (innerErr.InnerException != null)
            {
                retMessage = retMessage + " - " + GetErrorMessage(innerErr.InnerException.Message);

                innerErr = innerErr.InnerException;
            }

            retMessage = retMessage.Replace("'", "").Replace(Environment.NewLine, " ");
            retMessage = retMessage.Replace("Transaction count after EXECUTE indicates a mismatching number of BEGIN and COMMIT statements. Previous count = 1, current count = 0.", "");

            return retMessage;
        }

        static string GetErrorMessage(String error)
        {
            Boolean IsXML = false;
            Boolean isJson = false;
            string retMessage = "";

            try
            {
                var text = XDocument.Parse(error);
                IsXML = true;
            }
            catch (Exception ex)
            {
                string dummy = ex.Message;
            }

            try
            {
                var text = JToken.Parse(error);
                isJson = true;
            }
            catch (Exception ex)
            {
                string dummy = ex.Message;
            }

            if (IsXML)
            {
                var doc = XDocument.Parse(error);
                var nspace = doc.Root.GetNamespaceOfPrefix("m");

                if (nspace == null)
                {
                    retMessage = error;
                }
                else
                {
                    var msg = from node in doc.Root.Elements(nspace + "innererror")
                                        .Elements(nspace + "message")
                              select node.Value;

                    if (msg.LastOrDefault() == null)
                    {
                        msg = from node in doc.Root.Elements(nspace + "message")
                              select node.Value;
                    }

                    if (msg.LastOrDefault() == null)
                    {
                        retMessage = error.Replace("'", "");
                    }
                    else
                    {
                        retMessage = msg.LastOrDefault().Replace("'", "");
                    }
                }
            }

            else if (isJson)
            {
                dynamic data = JObject.Parse(error);
                var msg = data.error;

                retMessage = msg.message;

                while (msg.innererror != null || msg.internalexception != null)
                {
                    if (msg.innererror != null)
                    {
                        retMessage = retMessage + " - " + msg.innererror.message;
                        msg = msg.innererror;
                    }
                    else if (msg.internalexception != null)
                    {
                        retMessage = retMessage + " - " + msg.internalexception.message;
                        msg = msg.internalexception;
                    }
                }
            }

            else
            {
                retMessage = error.Replace("'", "").Replace(Environment.NewLine, " ");
            }

            return retMessage;
        }

        public void WriteErrorToLog(String URL, Exception ex, String AdditionalMessage)
        {
            try
            {
                DataLayer.EPSEntities db = new DataLayer.EPSEntities();
                User CurrentUser = (User)System.Web.HttpContext.Current.Session["CurrentUser"];

                string ErrorText = string.Format("{2}Error Message: {0}, Inner Exception Message: {1}, Trace Data: {3}", ex.Message, ex.InnerException == null ? "None." : ex.InnerException.Message, AdditionalMessage == "" ? "" : AdditionalMessage + " - ", ex.StackTrace);

                ErrorLog error = new ErrorLog { ErrorMessage = URL + " - " + ErrorText, Username = CurrentUser == null ? "Not Logged In" : CurrentUser.Username, ErrorDate = DateTime.Now };

                db.ErrorLogs.Add(error);
                db.SaveChanges();
            }
            catch (Exception ex1)
            {
                string dummy = ex1.Message;
            }
        }

        public User GetLoggedOnUser()
        {
            try
            {
                User CurrentUser = (User)System.Web.HttpContext.Current.Session["CurrentUser"];

                if (CurrentUser == null)
                {
                    String Username = System.Web.HttpContext.Current.User.Identity.Name;

                    DataLayer.EPSEntities db = new DataLayer.EPSEntities();

                    User user = db.Users.Where(u => u.Username == Username).FirstOrDefault();

                    System.Web.HttpContext.Current.Session["CurrentUser"] = user;

                    return user;
                }
                else
                {
                    return CurrentUser;
                }
            }
            catch (Exception exUser)
            {
                string dummy = exUser.Message;
                return null;
            }
        }
    }
}