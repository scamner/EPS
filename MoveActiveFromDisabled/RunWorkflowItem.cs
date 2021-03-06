﻿using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Text;

namespace ItemToRun
{
    public class RunWorkflowItem
    {
        public Utilities.ItemRunResult RunItem(int EmpID, RunPayload RunPayload)
        {
            DataLayer.EPSEntities db = new DataLayer.EPSEntities();
            Utilities util = new Utilities();

            try
            {
                Employee emp = db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();
                String domain = util.GetParam("ADDomain", "Active Directory domain");
                String adminName = util.GetParam("ADUsername", "Active Directory admin user");
                String password = util.GetParam("ADPassword", "Active Directory admin user password");
                String disabledFolderPath = util.GetParam("DisabledFolderPath", "disabled users folder path");

                if (disabledFolderPath.EndsWith("\\"))
                {
                    disabledFolderPath = disabledFolderPath.Remove(disabledFolderPath.Length - 1, 1);
                }

                PrincipalContext context = new PrincipalContext(ContextType.Domain, domain, adminName, password);
                UserPrincipal user = UserPrincipal.FindByIdentity(context, emp.Username);
                DirectoryEntry DE = (DirectoryEntry)user.GetUnderlyingObject();
                String activeUsersRoot = util.GetParam("ActiveUsersFolderPath", "active users folder path");

                if (!System.IO.Directory.Exists(disabledFolderPath))
                {
                    return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("The disabled users folder '{0}' could not be found.", disabledFolderPath), TimeDone = DateTime.Now };
                }

                if (user == null)
                {
                    return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("{0} could not be found in Active Directory.", emp.Username), TimeDone = DateTime.Now };
                }

                String userFolder = "";

                foreach (string dirPath in Directory.GetDirectories(disabledFolderPath, "*", SearchOption.AllDirectories))
                {
                    if (userFolder != "")
                    {
                        break;
                    }

                    int yearFolder = 0;
                    DirectoryInfo di = new DirectoryInfo(dirPath);
                    string currentFolder = di.Name;

                    if (currentFolder.Length == 4 && int.TryParse(currentFolder, out yearFolder) == true)
                    {
                        foreach (string uf in Directory.GetDirectories(di.FullName, "*", SearchOption.TopDirectoryOnly))
                        {
                            if (uf.EndsWith("\\" + emp.Username))
                            {
                                userFolder = uf;
                                break;
                            }
                        }
                    }
                }

                if (userFolder == "")
                {
                    return new Utilities.ItemRunResult { ResultID = 5, ResultText = String.Format("{0} {1}'s folder was not found in the disabled folder path.", emp.FirstName, emp.LastName), TimeDone = DateTime.Now };
                }

                userFolder = userFolder + "\\ActiveUserFiles";

                if (!Directory.Exists(userFolder))
                {
                    return new Utilities.ItemRunResult { ResultID = 5, ResultText = String.Format("{0} {1}'s active user folder was not found in the disabled folder path.", emp.FirstName, emp.LastName), TimeDone = DateTime.Now };
                }

                String activeUserFolder = String.Format("{0}\\{2}, {1}", activeUsersRoot, emp.FirstName, emp.LastName);

                if (!Directory.Exists(activeUserFolder))
                {
                    Directory.CreateDirectory(activeUserFolder);
                }

                foreach (string dirPath in Directory.GetDirectories(userFolder, "*", SearchOption.AllDirectories))
                {
                    string newPath = dirPath.Replace(userFolder, activeUserFolder);
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                }

                foreach (string oldFile in Directory.GetFiles(userFolder, "*.*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(oldFile, System.IO.FileAttributes.Normal);
                }

                foreach (string oldFile in Directory.GetFiles(userFolder, "*.*", SearchOption.AllDirectories))
                {
                    String newFile = oldFile.Replace(userFolder, activeUserFolder);

                    if (!File.Exists(newFile))
                    {
                        File.SetAttributes(oldFile, System.IO.FileAttributes.Normal);
                        File.Move(oldFile, newFile);
                    }
                }

                System.IO.Directory.Delete(userFolder, true);

                return new Utilities.ItemRunResult { ResultID = 2, ResultText = String.Format("{0} {1}'s active user folder was moved from '{3}' to '{2}'.", emp.FirstName, emp.LastName, activeUserFolder, userFolder), TimeDone = DateTime.Now };
            }
            catch (Exception ex)
            {
                return new Utilities.ItemRunResult { ResultID = 4, ResultText = String.Format("Error: {0}", ex.Message), TimeDone = DateTime.Now };
            }
        }
    }
}