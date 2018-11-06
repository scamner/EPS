using DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class EmployeeFunctions
    {
        DataLayer.EPSEntities db = new DataLayer.EPSEntities();

        public DirectoryEntry GetUserByPath(String Path)
        {
            String Domain = db.Parameters.Where(p => p.ParamName == "ADDomain").FirstOrDefault().ParamValue;
            Parameter adminName = db.Parameters.Where(p => p.ParamName == "ADUsername").FirstOrDefault();
            Parameter password = db.Parameters.Where(p => p.ParamName == "ADPassword").FirstOrDefault();

            string strRootForest = "LDAP://" + Domain;
            System.DirectoryServices.DirectoryEntry root = new System.DirectoryServices.DirectoryEntry(strRootForest, adminName.ParamValue, password.ParamValue);

            System.DirectoryServices.DirectorySearcher searcher = new System.DirectoryServices.DirectorySearcher(root);
            searcher.SearchScope = SearchScope.Subtree;
            searcher.ReferralChasing = ReferralChasingOption.All;

            string vbSearchCriteria = "(distinguishedName=" + Path + ")";

            searcher.Filter = "(&(objectClass=user)" + vbSearchCriteria + ")";

            SearchResult result = searcher.FindOne();

            System.DirectoryServices.DirectoryEntry ADsObject = result.GetDirectoryEntry();

            return ADsObject;
        }

        public Boolean IsAManager(String Path, String adminName, String password, String Domain)
        {
            string strRootForest = "LDAP://" + Domain;
            System.DirectoryServices.DirectoryEntry root = new System.DirectoryServices.DirectoryEntry(strRootForest, adminName, password);

            System.DirectoryServices.DirectorySearcher searcher = new System.DirectoryServices.DirectorySearcher(root);
            searcher.SearchScope = SearchScope.Subtree;
            searcher.ReferralChasing = ReferralChasingOption.All;

            string vbSearchCriteria = "(manager=" + Path + ")";

            searcher.Filter = "(&(objectClass=user)" + vbSearchCriteria + ")";

            SearchResult result = searcher.FindOne();

            return result == null ? false : true;
        }

        public List<ADUser> SearchAD(String FirstName, String LastName, Boolean ForUser, int AuditUserID)
        {
            List<ADUser> lstUsers = new List<ADUser>();
            String Domain = db.Parameters.Where(p => p.ParamName == "ADDomain").FirstOrDefault().ParamValue;
            Parameter adminName = db.Parameters.Where(p => p.ParamName == "ADUsername").FirstOrDefault();
            Parameter password = db.Parameters.Where(p => p.ParamName == "ADPassword").FirstOrDefault();

            string strRootForest = "LDAP://" + Domain;
            System.DirectoryServices.DirectoryEntry root = new System.DirectoryServices.DirectoryEntry(strRootForest, adminName.ParamValue, password.ParamValue);

            System.DirectoryServices.DirectorySearcher searcher = new System.DirectoryServices.DirectorySearcher(root);
            searcher.SearchScope = SearchScope.Subtree;
            searcher.ReferralChasing = ReferralChasingOption.All;

            string vbSearchCriteria = null;

            if (!(string.IsNullOrEmpty(FirstName)))
            {
                vbSearchCriteria = vbSearchCriteria + "(givenName=" + FirstName.TrimStart().TrimEnd() + "*)";
            }

            if (!(string.IsNullOrEmpty(LastName)))
            {
                vbSearchCriteria = vbSearchCriteria + "(sn=" + LastName.TrimStart().TrimEnd() + "*)";
            }

            searcher.Filter = "(&(objectClass=user)" + vbSearchCriteria + ")";

            SearchResultCollection vbResults = searcher.FindAll();
            int vbCount = vbResults.Count;

            if (vbCount == 0)
            {
                throw new Exception("Account cannot be found in Active Directory.");
            }

            for (int i = 0; i <= vbCount - 1; i++)
            {
                SearchResult result = vbResults[i];

                System.DirectoryServices.DirectoryEntry ADsObject = result.GetDirectoryEntry();
                string vbUsername = Domain + "\\" + result.Properties["sAMAccountName"][0].ToString();
                string vbFname = "";
                string vbLname = "";
                string vbEmail = "";
                string vbEmpNum = "";
                string vbManagerPath = "";
                Guid vbManagerGUID;
                int? vbManagerID = null;

                if (result.Properties["givenName"].Count > 0)
                {
                    vbFname = result.Properties["givenName"][0].ToString();
                }

                if (result.Properties["sn"].Count > 0)
                {
                    vbLname = result.Properties["sn"][0].ToString();
                }

                if (result.Properties["mail"].Count > 0)
                {
                    vbEmail = result.Properties["mail"][0].ToString();
                }

                if (result.Properties["employeeNumber"].Count > 0)
                {
                    vbEmpNum = result.Properties["employeeNumber"][0].ToString();
                }

                if (result.Properties["manager"].Count > 0)
                {
                    vbManagerPath = result.Properties["manager"][0].ToString();
                    vbManagerGUID = GetUserByPath(vbManagerPath).Guid;

                    AddMissingManagers(vbManagerPath, AuditUserID);

                    Employee manager = db.Employees.Where(e => e.ADGUID == vbManagerGUID).FirstOrDefault();

                    vbManagerID = manager.EmpID;
                }

                Boolean isAManger = IsAManager(result.Properties["distinguishedName"][0].ToString(), adminName.ParamValue, password.ParamValue, Domain);

                ADUser user = new ADUser();
                user.Username = vbUsername.Replace(Domain + "\\", "");
                user.FirstName = vbFname;
                user.LastName = vbLname;
                user.Email = vbEmail;
                user.ADGUID = ADsObject.Guid.ToString();
                user.ManagerID = vbManagerID;
                user.EmpNum = vbEmpNum;
                user.IsManager = isAManger;
                user.ManagerPath = vbManagerPath;

                lstUsers.Add(user);
            }

            for (int i = 0; i <= lstUsers.Count - 1; i++)
            {
                string username = lstUsers[i].Username.Replace(Domain + "\\", "").ToString().ToUpper().TrimEnd();

                if (ForUser == true)
                {
                    List<User> lstExistingUsers = db.Users.ToList();

                    if (lstExistingUsers.Any(s => s.Username.ToString().ToUpper().TrimEnd() == username))
                    {
                        lstUsers[i].Exists = true;
                    }
                    else
                    {
                        lstUsers[i].Exists = false;
                    }
                }
                else
                {
                    List<Employee> lstExistingEmps = db.Employees.ToList();

                    if (lstExistingEmps.Any(s => s.Username.ToString().ToUpper().TrimEnd() == username))
                    {
                        lstUsers[i].Exists = true;
                    }
                    else
                    {
                        lstUsers[i].Exists = false;
                    }
                }
            }

            return lstUsers;
        }

        public void AddEmployee(String ADGUID, String Email, String FirstName, String LastName, String Username, String IsManager, String ManagerID, String EmpNum, int AuditUserID)
        {
            Employee emp = new Employee
            {
                ADGUID = new Guid(ADGUID),
                Email = Email,
                FirstName = FirstName,
                EmpNum = EmpNum,
                LastName = LastName,
                Username = Username,
                IsManager = Convert.ToBoolean(IsManager)
            };

            if (!String.IsNullOrEmpty(ManagerID) && ManagerID != "0")
            {
                emp.ReportsTo = Convert.ToInt32(ManagerID);
            }

            db.Employees.Add(emp);
            db.SaveChanges();

            Employees_Log log = new Employees_Log
            {
                ADGUID = new Guid(ADGUID),
                Email = Email,
                FirstName = FirstName,
                EmpNum = "",
                LastName = LastName,
                Username = Username,
                IsManager = Convert.ToBoolean(IsManager),
                EmpID = emp.EmpID,
                ChangeDate = DateTime.Now,
                ChangedBy = AuditUserID,
                ChangeType = "Insert"
            };

            if (!String.IsNullOrEmpty(ManagerID) && ManagerID != "0")
            {
                log.ReportsTo = Convert.ToInt32(ManagerID);
            }

            db.Employees_Log.Add(log);
            db.SaveChanges();
        }

        public void AddMissingManagers(String ManagerPath, int AuditUserID)
        {
            List<System.DirectoryServices.DirectoryEntry> mgrsToAdd = new List<System.DirectoryServices.DirectoryEntry>();
            String mgrToAdd = "";

            System.DirectoryServices.DirectoryEntry mgrAD = GetUserByPath(ManagerPath);
            Employee manager = db.Employees.Where(e => e.ADGUID == mgrAD.Guid).FirstOrDefault();

            if (manager == null)
            {
                mgrToAdd = mgrAD.Path;

                if (!mgrsToAdd.Contains(mgrAD))
                {
                    mgrsToAdd.Add(mgrAD);
                }

                while (mgrToAdd != "")
                {
                    if (mgrAD.Properties["manager"].Count > 0)
                    {
                        String nextMgrPath = mgrAD.Properties["manager"][0].ToString();

                        mgrAD = GetUserByPath(nextMgrPath);
                        manager = db.Employees.Where(e => e.ADGUID == mgrAD.Guid).FirstOrDefault();

                        if (manager == null)
                        {
                            mgrToAdd = mgrAD.Path;

                            if (!mgrsToAdd.Contains(mgrAD))
                            {
                                mgrsToAdd.Add(mgrAD);
                            }
                        }
                        else
                        {
                            mgrToAdd = "";
                        }
                    }
                    else
                    {
                        mgrToAdd = "";
                    }
                }

                for (int i = mgrsToAdd.Count(); i-- > 0;)
                {
                    System.DirectoryServices.DirectoryEntry m = mgrsToAdd[i];

                    if (m.Properties["manager"].Count > 0)
                    {
                        Guid thisMgrGUID = GetUserByPath(m.Properties["manager"][0].ToString()).Guid;
                        manager = db.Employees.Where(e => e.ADGUID == thisMgrGUID).FirstOrDefault();
                    }
                    else
                    {
                        manager = null;
                    }

                    AddEmployee(m.Guid.ToString(),
                        m.Properties["mail"].Count > 0 ? m.Properties["mail"][0].ToString() : "",
                        m.Properties["givenName"].Count > 0 ? m.Properties["givenName"][0].ToString() : "",
                        m.Properties["sn"].Count > 0 ? m.Properties["sn"][0].ToString() : "",
                        m.Username,
                        "true",
                        manager != null ? manager.EmpID.ToString() : "",
                        m.Properties["employeeNumber"].Count > 0 ? m.Properties["employeeNumber"][0].ToString() : "",
                        AuditUserID);
                }
            }
        }
    }

    public class ADUser : User
    {
        public string ADGUID { get; set; }
        public Boolean Exists { get; set; }

        [Required(ErrorMessage = "At least part of a first name is required.")]
        public new String FirstName { get; set; }

        [Required(ErrorMessage = "At least part of a last name is required.")]
        public new String LastName { get; set; }

        public Boolean ForUser { get; set; }

        public int? ManagerID { get; set; }

        public String ManagerPath { get; set; }

        public String EmpNum { get; set; }

        public Boolean IsManager { get; set; }
    }
}

