﻿using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemToRun
{
    public class RunWorkflowItem
    {
        public Utilities.ItemRunResult RunItem(int EmpID, RunPayload RunPayload)
        {
            DataLayer.EPSEntities db = new DataLayer.EPSEntities();
            Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();

            try
            {
                Utilities util = new Utilities();

                String myName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                LibraryItem li = db.LibraryItems.Where(l => l.LibraryPath.EndsWith(myName + ".dll")).FirstOrDefault();

                String htmlOptions = li.HtmlOptions;

                Employee manager = db.Employees.Where(e => e.EmpID == emp.ReportsTo).FirstOrDefault();

                if (manager == null)
                {
                    throw new Exception(String.Format("There is no manager assigned to {0} {1}.", emp.FirstName, emp.LastName));
                }

                if (String.IsNullOrEmpty(manager.Email))
                {
                    throw new Exception(String.Format("The manager assigned to {0} {1} does not have an email set.", emp.FirstName, emp.LastName));
                }

                String emailServer = util.GetParam("SMTPServer", "smtp (email) server name");
                String from = util.GetParam("EmailFrom", "email address to send from");
                String to = manager.Email;
                String body = util.GetParam("FinalDisableNotifyBody", "message to disable distro group to notify them of a disabled employee");
                String subject = util.GetParam("FinalDisableNotifySubject", "subject line for the email to send the disable distro group to notify them of a disabled employee");

                List<RunPayloadItem> thisPL = RunPayload.RunPayloadItems.Where(p => p.ItemID == li.ItemID).ToList();

                string numDays = thisPL.Where(p => p.ElementID == "NumDays").FirstOrDefault().ElementValue;
                string ticketNumber = thisPL.Where(p => p.ElementID == "TicketNumber").FirstOrDefault().ElementValue;

                body = body.Replace("[NumDays]", numDays);
                body = body.Replace("[TicketNumber]", ticketNumber);

                util.SendEmail(EmpID, from, to, null, null, subject, body);

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("The email was sent to '{0}' regarding {1} {2} for their final notification.", manager.Email, emp.FirstName, emp.LastName), TimeDone = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now };
            }
        }
    }
}
