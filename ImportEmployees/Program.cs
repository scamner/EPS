using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace ImportEmployees
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string currUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                int AuditUserID = 0;

                string pathToFile = args.Count() == 0 ? "" : args[0];
                DataLayer.EPSEntities db = new DataLayer.EPSEntities();
                EmployeeFunctions funct = new EmployeeFunctions();
                Utilities util = new Utilities();

                String domain = util.GetParam("ADDomain", "Active Directory domain");
                String adminName = util.GetParam("ADUsername", "Active Directory admin user");
                String password = util.GetParam("ADPassword", "Active Directory admin user password");

                int slashIndex = currUser.IndexOf("\\") + 1;
                currUser = currUser.Substring(slashIndex, (currUser.Length - slashIndex));
                User admin = db.Users.Where(e => e.Username == currUser).FirstOrDefault();

                if (admin == null)
                {
                    Console.WriteLine("ERROR: You are not a user in the EPS system.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                AuditUserID = admin.UserID;

                if (String.IsNullOrEmpty(pathToFile))
                {
                    Console.WriteLine("ERROR: No file was entered.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                if (!System.IO.File.Exists(pathToFile))
                {
                    Console.WriteLine("ERROR: The file " + pathToFile + " does not exist.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                String[] users = System.IO.File.ReadAllLines(pathToFile);

                if (users.Count() == 0)
                {
                    Console.WriteLine("ERROR: No users were found in the file " + pathToFile + ".");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                foreach (String u in users)
                {
                    DataLayer.Employee emp = db.Employees.Where(e => e.Username == u).FirstOrDefault();

                    if (emp == null)
                    {
                        PrincipalContext context = new PrincipalContext(ContextType.Domain, domain, adminName, password);
                        UserPrincipal user = UserPrincipal.FindByIdentity(context, u);

                        if (user == null)
                        {
                            Console.WriteLine("ERROR: " + u + " could not be found in Active Directory.");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }

                        DirectoryEntry DE = (DirectoryEntry)user.GetUnderlyingObject();

                        String isManager = funct.IsAManager(DE.Path, adminName, password, domain).ToString();
                        String vbManagerID = null;
                        String vbEmpNum = "";

                        if (DE.Properties["employeeNumber"].Count > 0)
                        {
                            vbEmpNum = DE.Properties["employeeNumber"][0].ToString();
                        }

                        if (DE.Properties["manager"].Count > 0)
                        {
                            String vbManagerPath = DE.Properties["manager"][0].ToString();
                            Guid vbManagerGUID = funct.GetUserByPath(vbManagerPath).Guid;

                            funct.AddMissingManagers(vbManagerPath, AuditUserID);

                            Employee manager = db.Employees.Where(e => e.ADGUID == vbManagerGUID).FirstOrDefault();

                            vbManagerID = manager.EmpID.ToString();
                        }

                        funct.AddEmployee(user.Guid.ToString(), user.EmailAddress == null ? "" : user.EmailAddress, user.GivenName == null ? "" : user.GivenName, user.Surname == null ? "" : user.Surname, u, isManager, vbManagerID, vbEmpNum, AuditUserID);
                        Console.WriteLine(u + " added.");
                    }
                    else
                    {
                        Console.WriteLine(u + " already exists in EPS.");
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("Users imported successfully.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}
