using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using MyAgencyVault.BusinessLibrary.Masters;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Transactions;
using System.Data;
using System.Threading;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.IO;
using System.Globalization;


namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class PolicySavedStatus
    {
        [DataMember]
        public bool IsError { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [DataContract]
    public class PayorObject
    {
        [DataMember]
        public Guid PayorID { get; set; }
        [DataMember]
        public string PayorName { get; set; }
        [DataMember]
        public string NickName { get; set; }
    }

    [DataContract]
    public class CarrierObject
    {
        [DataMember]
        public Guid PayorID { get; set; }
        [DataMember]
        public Guid CarrierID { get; set; }
        [DataMember]
        public string CarrierName { get; set; }
    }


    [DataContract]
    public class PolicyImportStatus
    {
        [DataMember]
        public int ImportCount { get; set; }
        [DataMember]
        public int UpdateCount { get; set; }
        [DataMember]
        public int ErrorCount { get; set; }
        [DataMember]
        public List<string> ErrorList { get; set; }
    }

    [DataContract]
    public class Benefits_PolicyImportStatus
    {
        [DataMember]
        public int NewCount { get; set; }
        [DataMember]
        public int UpdateCount { get; set; }
        [DataMember]
        public int ErrorCount { get; set; }
        [DataMember]
        public List<Benefits_ErrorMsg> ErrorList { get; set; }
    }

    [DataContract]
    public class Benefits_ErrorMsg
    {
           [DataMember]
        public  string OriginalPlanID { get; set; }
           [DataMember]
        public string PolicyPlanID { get; set; }
           [DataMember]
        public string Message { get; set; }

       
        public Benefits_ErrorMsg(string origID, string policyID, string msg)
         {
             OriginalPlanID = origID;
             PolicyPlanID = policyID;
             Message = msg;
         }

        [DataContract]
        public class errMsg
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public string Val { get; set; }

            public errMsg(string n, string v)
            {
                Name = n;
                Val = v;
            }
        }
    }


    public class Policy
    {

        #region Import Policy
        public static string GetPolicyIdKeyForImport()
        {
            return System.Configuration.ConfigurationSettings.AppSettings["PolicyIDKeyName"];
        }

        public static Guid IsPolicyExistingWithImportID(string ImportID, Guid LicID)
        {
            //Guid policyID = Guid.Empty;
            Guid policy = Guid.Empty;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var  _policy = (from p in DataModel.PolicyLearnedFields 
                                         join o in DataModel.Policies  on p.PolicyId equals o.PolicyId where (p.ImportPolicyID == ImportID && o.PolicyLicenseeId == LicID && o.IsDeleted == false) select p).FirstOrDefault();
                if (_policy != null)
                {
                    policy = _policy.PolicyId;
                }
            }
            return policy;
        }
      
        static int PolicCompType(string strCompType, ObservableCollection<CompType> CompTypeTypeLst)
        {
            int intStatus = 5;

            try
            {
                if (string.IsNullOrEmpty(strCompType))
                {
                    //Default Pending
                    return intStatus = 5;
                }
                CompType objComp = CompTypeTypeLst.Where(p => p.Names.ToLower() == strCompType.ToLower()).FirstOrDefault();
                if (objComp != null)
                {
                    if (objComp.IncomingPaymentTypeID != null)
                    {
                        intStatus = Convert.ToInt32(objComp.IncomingPaymentTypeID);
                    }
                }
                else
                {
                    objComp = CompTypeTypeLst.Where(p => p.PaymentTypeName.ToLower() == strCompType.ToLower()).FirstOrDefault();

                    if (objComp != null)
                    {
                        if (objComp.IncomingPaymentTypeID != null)
                        {
                            intStatus = Convert.ToInt32(objComp.IncomingPaymentTypeID);
                        }
                    }
                }
            }
            catch
            {
            }
            return intStatus;
        }

        public static void InsertIntoMembers(DataTable dataTable)
        {
            using (SqlConnection connection = new SqlConnection(DBConnection.GetConnectionString()))
            {
                SqlTransaction transaction = null;
                connection.Open();
                try
                {
                    transaction = connection.BeginTransaction();

                    using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
                    {
                        sqlBulkCopy.DestinationTableName = "Policies";
                        sqlBulkCopy.ColumnMappings.Add("PolicyId", "PolicyId");
                        sqlBulkCopy.ColumnMappings.Add("PolicyNumber", "PolicyNumber");
                        sqlBulkCopy.ColumnMappings.Add("PolicyStatusId", "PolicyStatusId");
                        sqlBulkCopy.ColumnMappings.Add("PolicyType", "PolicyType");
                        sqlBulkCopy.ColumnMappings.Add("PolicyClientId", "PolicyClientId");

                        sqlBulkCopy.ColumnMappings.Add("PolicyLicenseeId", "PolicyLicenseeId");
                        sqlBulkCopy.ColumnMappings.Add("OriginalEffectiveDate", "OriginalEffectiveDate");
                        sqlBulkCopy.ColumnMappings.Add("TrackFromDate", "TrackFromDate");
                        sqlBulkCopy.ColumnMappings.Add("PolicyModeId", "PolicyModeId");
                        sqlBulkCopy.ColumnMappings.Add("MonthlyPremium", "MonthlyPremium");

                        sqlBulkCopy.ColumnMappings.Add("CoverageId", "CoverageId");
                        sqlBulkCopy.ColumnMappings.Add("SubmittedThrough", "SubmittedThrough");

                        sqlBulkCopy.ColumnMappings.Add("Enrolled", "Enrolled");
                        sqlBulkCopy.ColumnMappings.Add("Eligible", "Eligible");
                        sqlBulkCopy.ColumnMappings.Add("PolicyTerminationDate", "PolicyTerminationDate");
                        sqlBulkCopy.ColumnMappings.Add("TerminationReasonId", "TerminationReasonId");
                        sqlBulkCopy.ColumnMappings.Add("IsTrackMissingMonth", "IsTrackMissingMonth");
                        sqlBulkCopy.ColumnMappings.Add("IsTrackIncomingPercentage", "IsTrackIncomingPercentage");
                        sqlBulkCopy.ColumnMappings.Add("IsTrackPayment", "IsTrackPayment");
                        sqlBulkCopy.ColumnMappings.Add("IncomingPaymentTypeId", "IncomingPaymentTypeId");

                        sqlBulkCopy.ColumnMappings.Add("IsDeleted", "IsDeleted");
                        sqlBulkCopy.ColumnMappings.Add("ReplacedBy", "ReplacedBy");
                        sqlBulkCopy.ColumnMappings.Add("PayorId", "PayorId");
                        sqlBulkCopy.ColumnMappings.Add("DuplicateFrom", "DuplicateFrom");
                        sqlBulkCopy.ColumnMappings.Add("CreatedBy", "CreatedBy");
                        sqlBulkCopy.ColumnMappings.Add("CreatedOn", "CreatedOn");
                        sqlBulkCopy.ColumnMappings.Add("IsIncomingBasicSchedule", "IsIncomingBasicSchedule");
                        sqlBulkCopy.ColumnMappings.Add("IsOutGoingBasicSchedule", "IsOutGoingBasicSchedule");
                        sqlBulkCopy.ColumnMappings.Add("CarrierId", "CarrierId");

                        sqlBulkCopy.ColumnMappings.Add("SplitPercentage", "SplitPercentage");
                        sqlBulkCopy.ColumnMappings.Add("Insured", "Insured");
                        sqlBulkCopy.ColumnMappings.Add("ActivatedOn","ActivatedOn");
                        sqlBulkCopy.ColumnMappings.Add("IsLocked", "IsLocked");
                        sqlBulkCopy.ColumnMappings.Add("LastFollowUpRuns","LastFollowUpRuns");
                        sqlBulkCopy.ColumnMappings.Add("RowVersion","RowVersion");
                        sqlBulkCopy.ColumnMappings.Add("Advance", "Advance");
                        sqlBulkCopy.ColumnMappings.Add("ProductType","ProductType");
                        sqlBulkCopy.ColumnMappings.Add("UserCredentialId", "UserCredentialId");
                        sqlBulkCopy.ColumnMappings.Add("AccoutExec","AccoutExec");

                        sqlBulkCopy.ColumnMappings.Add("LastNoVarIssueDate","LastNoVarIssueDate");
                        sqlBulkCopy.ColumnMappings.Add("LastNoMissIssueDate","LastNoMissIssueDate");

                        sqlBulkCopy.WriteToServer(dataTable);
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }

            }
        }

        public static PolicyImportStatus ImportPolicy_faster(DataTable dt, ObservableCollection<User> GlobalAgentList, Guid LicID, ObservableCollection<CompType> CompTypeList)
        {
            #region Variables
            PolicyImportStatus objStatus = new PolicyImportStatus();
            string policyIDKey = System.Configuration.ConfigurationSettings.AppSettings["PolicyIDKeyName"];
            ActionLogger.Logger.WriteImportLogDetail("Import Policy: policyIDKey: " + policyIDKey, true);
            int addCount = 0;
            int updateCount = 0;
            int errorCount = 0;
            List<string> errorList = new List<string>();


            List<OutGoingPayment> OutGoingField = new List<OutGoingPayment>();
            PolicyToolIncommingShedule inSchedule = null;
            string strProductType = string.Empty;
            string covNickName = string.Empty;
            Guid houseOwner = Guid.Empty;

            #endregion
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                
                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Data model init", true);
                var AgentList = (from p in DataModel.UserCredentials
                                 join o in DataModel.UserDetails on p.UserCredentialId equals o.UserCredentialId
                                 where p.LicenseeId == LicID /*&& p.RoleId == 3*/ && p.IsDeleted == false
                                 select new
                                 {
                                     p.UserCredentialId,
                                     o.NickName,
                                     p.UserName,
                                     p.RoleId,
                                     o.FirstName,
                                     o.LastName
                                 }).ToList();
                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Agent list fetched", true);

                #region temp policies table
                DataTable dtPolicies = new DataTable("tempPolicies");

                dtPolicies.Columns.Add("PolicyId", typeof(Guid));
                dtPolicies.Columns.Add("PolicyNumber", typeof(string));
                dtPolicies.Columns.Add("PolicyStatusId", typeof(int));
                dtPolicies.Columns.Add("PolicyType", typeof(string));
                dtPolicies.Columns.Add("PolicyClientId", typeof(Guid));
                dtPolicies.Columns.Add("PolicyLicenseeId", typeof(Guid));
                dtPolicies.Columns.Add("OriginalEffectiveDate", typeof(DateTime));
                dtPolicies.Columns.Add("TrackFromDate", typeof(DateTime));
                dtPolicies.Columns.Add("PolicyModeId", typeof(int));

                dtPolicies.Columns.Add("MonthlyPremium", typeof(double));
                dtPolicies.Columns.Add("CoverageId", typeof(Guid));
                dtPolicies.Columns.Add("Submittedthrough", typeof(string));
                dtPolicies.Columns.Add("Enrolled", typeof(string));
                dtPolicies.Columns.Add("Eligible", typeof(string));
                dtPolicies.Columns.Add("PolicyTerminationDate", typeof(DateTime));
                dtPolicies.Columns.Add("TerminationReasonId", typeof(int));
                dtPolicies.Columns.Add("IsTrackMissingMonth", typeof(bool));
                dtPolicies.Columns.Add("IsTrackIncomingPercentage", typeof(bool));
                dtPolicies.Columns.Add("IsTrackPayment", typeof(bool));

                dtPolicies.Columns.Add("IncomingPaymentTypeId", typeof(int));
                dtPolicies.Columns.Add("IsDeleted", typeof(bool));
                dtPolicies.Columns.Add("ReplacedBy", typeof(Guid));
                dtPolicies.Columns.Add("PayorId", typeof(Guid));
                dtPolicies.Columns.Add("DuplicateFrom", typeof(Guid));
                dtPolicies.Columns.Add("CreatedBy", typeof(Guid));
                dtPolicies.Columns.Add("CreatedOn", typeof(DateTime));
                dtPolicies.Columns.Add("IsIncomingBasicSchedule", typeof(bool));
                dtPolicies.Columns.Add("IsOutGoingBasicSchedule", typeof(bool));
                dtPolicies.Columns.Add("CarrierId", typeof(Guid));

                dtPolicies.Columns.Add("SplitPercentage", typeof(decimal));
                dtPolicies.Columns.Add("Insured", typeof(string));
                dtPolicies.Columns.Add("ActivatedOn", typeof(DateTime));
                dtPolicies.Columns.Add("IsLocked", typeof(bool));
                dtPolicies.Columns.Add("LastFollowUpRuns", typeof(DateTime));
                dtPolicies.Columns.Add("RowVersion", typeof(byte[]));
                dtPolicies.Columns.Add("Advance", typeof(int));
                dtPolicies.Columns.Add("ProductType", typeof(string));
                dtPolicies.Columns.Add("UserCredentialId", typeof(Guid));
                dtPolicies.Columns.Add("AccoutExec", typeof(string));

                dtPolicies.Columns.Add("LastNoVarIssueDate", typeof(DateTime));
                dtPolicies.Columns.Add("LastNoMissIssueDate", typeof(DateTime));

                #endregion

                #region Temp policy learnedfields

                DataTable dtLearned = new DataTable("tempLearnedFields");
                dtLearned.Columns.Add("PolicyId", typeof(Guid));
                dtLearned.Columns.Add("Insured", typeof(string));
                dtLearned.Columns.Add("PolicyNumber", typeof(string));
                dtLearned.Columns.Add("Effective", typeof(DateTime));
                dtLearned.Columns.Add("TrackFrom", typeof(DateTime));
                dtLearned.Columns.Add("PAC", typeof(string));
                dtLearned.Columns.Add("PMC", typeof(string));

                dtLearned.Columns.Add("ModalAvgPremium", typeof(decimal));
                dtLearned.Columns.Add("PolicyModeId", typeof(int));
                dtLearned.Columns.Add("Enrolled", typeof(string));
                dtLearned.Columns.Add("Eligible", typeof(string));
                dtLearned.Columns.Add("AutoTerminationDate", typeof(DateTime));
                dtLearned.Columns.Add("Link1", typeof(string));
                dtLearned.Columns.Add("LastModifiedOn", typeof(DateTime));
                dtLearned.Columns.Add("LastModifiedUserCredentialid", typeof(Guid));
                dtLearned.Columns.Add("ClientID", typeof(Guid));
                dtLearned.Columns.Add("CompTypeID", typeof(int));

                dtLearned.Columns.Add("PayorSysID", typeof(string));
                dtLearned.Columns.Add("Renewal", typeof(string));
                dtLearned.Columns.Add("SplitPercentage", typeof(decimal));
                dtLearned.Columns.Add("CompScheduleType", typeof(string));

                dtLearned.Columns.Add("CarrierId", typeof(Guid));
                dtLearned.Columns.Add("CoverageId", typeof(Guid));
                dtLearned.Columns.Add("PayorId", typeof(Guid));

                dtLearned.Columns.Add("PreviousEffectiveDate", typeof(DateTime));
                dtLearned.Columns.Add("PreviousPolicyModeId", typeof(int));
                dtLearned.Columns.Add("CarrierNickName", typeof(string));
                dtLearned.Columns.Add("CoverageNickName", typeof(string));

                dtLearned.Columns.Add("Advance", typeof(int));
                dt.Columns.Add("ProductType", typeof(string));
                dtLearned.Columns.Add("UserCredentialId", typeof(Guid));

                dt.Columns.Add("AccoutExec", typeof(string));
                dt.Columns.Add("ImportPolicyID", typeof(string));

                #endregion

               

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow drPolicy = dtPolicies.NewRow();
                    dtPolicies.Rows.Add(drPolicy);

                    DLinq.Policy objPolicy = new DLinq.Policy();
                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: Iteration: " + i, true);

                    string importedPolicyID = dt.Columns.Contains(policyIDKey) ? Convert.ToString(dt.Rows[i][policyIDKey]).Trim() : (dt.Columns.Contains("Life Insurance Plan ID") ? Convert.ToString(dt.Rows[i]["Life Insurance Plan ID"]).Trim() : "");
                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: importedPolicyID: " + importedPolicyID, true);
                    OutGoingField = new List<OutGoingPayment>();
                    try
                    {
                        bool isNewPolicy = false;
                        Guid policyID = IsPolicyExistingWithImportID(importedPolicyID, LicID);
                        ActionLogger.Logger.WriteImportLogDetail("Import Policy: policyID: " + policyID, true);
                        if (policyID != Guid.Empty)
                        {
                            objPolicy = (from p in DataModel.Policies where p.PolicyId == policyID select p).FirstOrDefault();
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy: Existing policy", true);


                            //check if client exists or deleted in the system
                            //if deleted, then add the given as new 
                            if (objPolicy.Client != null && Convert.ToBoolean(objPolicy.Client.IsDeleted))
                            {
                                objPolicy.PolicyClientId = null;
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Client found deleted , so setting as null ", true);
                            }
                        }
                        else
                        {
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy: New policy", true);
                            isNewPolicy = true;
                            objPolicy.PolicyId = Guid.NewGuid();
                            drPolicy["PolicyId"] = objPolicy.PolicyId;

                            objPolicy.IsTrackPayment = true;
                            drPolicy["IsTrackPayment"] = objPolicy.IsTrackPayment;

                            objPolicy.PolicyLicenseeId = LicID;
                            drPolicy["PolicyLicenseeId"] = objPolicy.PolicyLicenseeId;

                            objPolicy.TerminationReasonId = null;
                            drPolicy["TerminationReasonId"] = objPolicy.TerminationReasonId;

                            objPolicy.IsTrackMissingMonth = true;
                            drPolicy["IsTrackMissingMonth"] = objPolicy.IsTrackMissingMonth;

                            objPolicy.CreatedOn = DateTime.Today;
                            drPolicy["CreatedOn"] = objPolicy.CreatedOn;

                            objPolicy.IsIncomingBasicSchedule = true;
                            drPolicy["IsIncomingBasicSchedule"] = objPolicy.IsIncomingBasicSchedule;

                            objPolicy.IsOutGoingBasicSchedule = true;
                            drPolicy["IsOutGoingBasicSchedule"] = objPolicy.IsOutGoingBasicSchedule;

                            objPolicy.CreatedBy = new Guid("AA38DF84-2E30-43CA-AED3-7276224D1B7E");
                            drPolicy["CreatedBy"] = objPolicy.CreatedBy;

                            objPolicy.IsDeleted = false;
                            drPolicy["IsDeleted"] = objPolicy.IsDeleted;
                           
                        }

                        #region Fields that should be updated when blank or woth new policy
                        try
                        {

                            if (isNewPolicy || string.IsNullOrEmpty(objPolicy.PolicyType))
                            {
                                if (dt.Columns.Contains("New?") || dt.Columns.Contains("New Business?"))
                                {
                                    string strTypeOfPolicy = dt.Columns.Contains("New?") ? Convert.ToString(dt.Rows[i]["New?"]) : (dt.Columns.Contains("New Business?") ? Convert.ToString(dt.Rows[i]["New Business?"]) : "");
                                    objPolicy.PolicyType = (!string.IsNullOrEmpty(strTypeOfPolicy) && (strTypeOfPolicy.ToLower() == "rewrite" || strTypeOfPolicy.ToLower() == "replace")) ? "Replace" : "New";
                                 
                                }
                                else if (isNewPolicy)
                                {
                                    objPolicy.PolicyType = "New";
                                }
                                drPolicy["PolicyType"] = objPolicy.PolicyType;
                            }
                            //if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Insured))
                            //{
                            //    if (dt.Columns.Contains("insured?"))
                            //    {
                            //        objPolicy.Insured = Convert.ToString(dt.Rows[i]["insured"]);
                            //    }
                            //    else
                            //    {
                            //        objPolicy.Insured = objPolicy.Client.Name;
                            //    }
                            //}
                            if (isNewPolicy || string.IsNullOrEmpty(objPolicy.PolicyNumber))
                            {
                                if (dt.Columns.Contains("Group #"))
                                {
                                    objPolicy.PolicyNumber = Convert.ToString(dt.Rows[i]["Group #"]);
                                    drPolicy["PolicyNumber"] = objPolicy.PolicyNumber;
                                }
                            }
                            if (isNewPolicy || string.IsNullOrEmpty(objPolicy.SubmittedThrough))
                            {
                                if (dt.Columns.Contains("Submitted Through"))
                                {
                                    objPolicy.SubmittedThrough = Convert.ToString(dt.Rows[i]["Submitted Through"]);
                                    drPolicy["SubmittedThrough"] = objPolicy.SubmittedThrough;
                                }
                            }
                            if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Enrolled))
                            {
                                if (dt.Columns.Contains("Number of Covered Lives"))
                                {
                                    objPolicy.Enrolled = Convert.ToString(dt.Rows[i]["Number of Covered Lives"]);
                                    drPolicy["Enrolled"] = objPolicy.Enrolled;
                                }
                            }
                            if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Eligible))
                            {
                                if (dt.Columns.Contains("eligible"))
                                {
                                    objPolicy.Eligible = Convert.ToString(dt.Rows[i]["eligible"]);
                                    drPolicy["Eligible"] = objPolicy.Eligible;
                                }
                            }
                            if (isNewPolicy || (objPolicy.Advance == null))
                            {
                                if (dt.Columns.Contains("Advanced Payment Number"))
                                {
                                    objPolicy.Advance = (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Advanced Payment Number"]))) ? Convert.ToInt32(Convert.ToString(dt.Rows[i]["Advanced Payment Number"])) : 0;
                                    drPolicy["Advance"] = objPolicy.Advance;
                                }
                            }
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy: optional fields , to be filled when blanks init done : " + policyIDKey, true);
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorList.Add(importedPolicyID);
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: optional fields exception : " + ex.Message, true);
                            continue;
                        }
                        //Incoming Schedule 
                        #region Incoming Schedule
                        bool oldInScheduleExists = false;
                        DLinq.PolicyIncomingSchedule tempSched = null;
                        //Check if incoming is already set with 0 splits
                        if (objPolicy.IsIncomingBasicSchedule == true)
                        {
                            tempSched = objPolicy.PolicyIncomingSchedules.FirstOrDefault();
                            if (tempSched != null)
                            {
                                if (tempSched.FirstYearPercentage > 0 || tempSched.RenewalPercentage > 0)
                                {
                                    oldInScheduleExists = true;
                                }
                                //else if (tempSched.FirstYearPercentage == 0 && tempSched.RenewalPercentage == 0)
                                //{
                                //    PolicyToolIncommingShedule.DeleteSchedule(objPolicy.PolicyId); //If schedule occurs with 0 split, then delete before adding new
                                //}
                            }
                        }

                        if (isNewPolicy || (objPolicy.IsIncomingBasicSchedule != true) || (!oldInScheduleExists))
                        {
                            if (dt.Columns.Contains("Commission Type") && dt.Columns.Contains("Payment Type") && dt.Columns.Contains("Commissions - First Year %") && dt.Columns.Contains("Commissions - Renewal %") && dt.Columns.Contains("Co Broker Split"))
                            {
                                if (tempSched != null && tempSched.FirstYearPercentage == 0 && tempSched.RenewalPercentage == 0)
                                {
                                    PolicyToolIncommingShedule.DeleteSchedule(objPolicy.PolicyId); //If schedule occurs with 0 split, then delete before adding new
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy old schedule is deleted", true);
                                }


                                string strCommisionType = Convert.ToString(dt.Rows[i]["Commission Type"]);
                                objPolicy.IncomingPaymentTypeId = PolicCompType(strCommisionType, CompTypeList);
                                string strOutPercentOfPremium = Convert.ToString(dt.Rows[i]["Payment Type"]);
                                string strInFirstYear = Convert.ToString(dt.Rows[i]["Commissions - First Year %"]);
                                string strInRenewYear = Convert.ToString(dt.Rows[i]["Commissions - Renewal %"]);
                                string strSplit = Convert.ToString(dt.Rows[i]["Co Broker Split"]);
                                objPolicy.SplitPercentage = (!string.IsNullOrEmpty(strSplit)) ? Convert.ToDouble(strSplit.Replace("%", "")) : 0;

                                inSchedule = new PolicyToolIncommingShedule();
                                inSchedule.PolicyId = objPolicy.PolicyId;
                                inSchedule.IncomingScheduleId = Guid.NewGuid();
                                inSchedule.ScheduleTypeId = (!string.IsNullOrEmpty(strOutPercentOfPremium) && strOutPercentOfPremium.ToLower().Contains("head")) ? 2 : 1;
                                inSchedule.FirstYearPercentage = (!string.IsNullOrEmpty(strInFirstYear)) ? Convert.ToDouble(strInFirstYear) : 0;
                                inSchedule.RenewalPercentage = (!string.IsNullOrEmpty(strInRenewYear)) ? Convert.ToDouble(strInRenewYear) : 0;
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Incoming schedule init done : " + policyIDKey, true);
                            }
                            else
                            {
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Incoming schedule not found in excel", true);
                            }
                        }
                        #endregion


                        try
                        {
                            //Track date
                            if (isNewPolicy || objPolicy.TrackFromDate == null)
                            {
                                if (dt.Columns.Contains("Track From"))
                                {
                                    if (dt.Rows[i]["Track From"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Track From"])))
                                    {
                                        string trackDate = Convert.ToString(dt.Rows[i]["Track From"]);
                                        objPolicy.TrackFromDate = Convert.ToDateTime(trackDate);
                                        drPolicy["TrackFromDate"] = objPolicy.TrackFromDate;
                                    }
                                }
                            }
                            //Mode
                            if (isNewPolicy || objPolicy.PolicyModeId == null)
                            {
                                if (dt.Columns.Contains("Modal Number"))
                                {
                                    string strMode = Convert.ToString(dt.Rows[i]["Modal Number"]);
                                    objPolicy.PolicyModeId = (!string.IsNullOrEmpty(strMode)) ? PolicyModeID(strMode) : PolicyModeID("0");
                                    drPolicy["PolicyModeId"] = objPolicy.PolicyModeId;
                                }
                            }

                            //premium
                            if (isNewPolicy || objPolicy.MonthlyPremium == null)
                            {
                                if (dt.Columns.Contains("Modal Premium"))
                                {
                                    string strPremiuum = Convert.ToString(dt.Rows[i]["Modal Premium"]);
                                    objPolicy.MonthlyPremium = (!string.IsNullOrEmpty(strPremiuum)) ? Convert.ToDecimal(strPremiuum) : 0;
                                    drPolicy["MonthlyPremium"] = objPolicy.MonthlyPremium;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorList.Add(importedPolicyID);
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy execption :premium/mode/trackDate : " + ex.Message, true);
                            continue;
                        }
                        //Payor
                        #region Payor
                        try
                        {
                            if (isNewPolicy || objPolicy.PayorId == null)
                            {
                                if (dt.Columns.Contains("Payor Commission Dept"))
                                {
                                    string strPayor = Convert.ToString(dt.Rows[i]["Payor Commission Dept"]);
                                    if (!string.IsNullOrEmpty(strPayor))
                                    {
                                        DLinq.Payor py = (from p in DataModel.Payors where p.PayorName.ToLower() == strPayor.ToLower() select p).FirstOrDefault();
                                        if (py == null)
                                        {
                                            py = (from p in DataModel.Payors where p.NickName.ToLower() == strPayor.ToLower() select p).FirstOrDefault();
                                            if (py != null)
                                            {
                                                objPolicy.PayorId = py.PayorId;
                                                objPolicy.PayorReference.Value = py;
                                                drPolicy["PayorId"] = objPolicy.PayorId;
                                            }
                                        }
                                    }
                                }
                            }
                        #endregion

                            #region Carrier
                            if (isNewPolicy || objPolicy.CarrierId == null)
                            {
                                if (dt.Columns.Contains("Carrier Commission Dept"))
                                {
                                    string strCarr = Convert.ToString(dt.Rows[i]["Carrier Commission Dept"]);
                                    if (!string.IsNullOrEmpty(strCarr))
                                    {
                                        DLinq.Carrier cr = (from p in DataModel.Carriers where p.CarrierName.ToLower() == strCarr.ToLower() select p).FirstOrDefault();
                                        if (cr == null)
                                        {
                                            DLinq.CarrierNickName crN = (from p in DataModel.CarrierNickNames where p.PayorId == objPolicy.PayorId select p).FirstOrDefault();
                                            if (crN == null)
                                            {
                                                crN = (from p in DataModel.CarrierNickNames where p.NickName == strCarr select p).FirstOrDefault();
                                            }
                                            if (crN != null)
                                            {
                                                objPolicy.CarrierId = crN.CarrierId;
                                                objPolicy.CarrierReference.Value = crN.CarrierReference.Value;
                                            }
                                        }
                                        else
                                        {
                                            objPolicy.CarrierId = cr.CarrierId;
                                            objPolicy.CarrierReference.Value = cr;
                                        }
                                        drPolicy["CarrierId"] = objPolicy.CarrierId;
                                    }
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorList.Add(importedPolicyID);
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: payor/carrier : " + ex.Message, true);
                            continue;
                        }

                        try
                        {
                            #region Product
                            if (isNewPolicy || objPolicy.CoverageId == null)
                            {
                                if (dt.Columns.Contains("Line Of Coverage"))
                                {
                                    string strProduct = Convert.ToString(dt.Rows[i]["Line Of Coverage"]);
                                    DLinq.Coverage cov = (from p in DataModel.Coverages where p.ProductName == strProduct select p).FirstOrDefault();
                                    if (cov != null)
                                    {
                                        objPolicy.CoverageId = cov.CoverageId;
                                        objPolicy.CoverageReference.Value = cov;
                                        drPolicy["CoverageId"] = objPolicy.CoverageId;
                                    }
                                }
                            }
                            #endregion

                            #region Product Type
                            if (isNewPolicy || string.IsNullOrEmpty(objPolicy.ProductType))
                            {
                                if (dt.Columns.Contains("Product Type"))
                                {
                                    strProductType = Convert.ToString(dt.Rows[i]["Product Type"]);
                                    DLinq.CoverageNickName covName = (from p in DataModel.CoverageNickNames where p.NickName == strProductType select p).FirstOrDefault();
                                    covNickName = (covName != null) ? covName.NickName : string.Empty; //to be used later in code, so kept separate
                                    objPolicy.ProductType = (!string.IsNullOrEmpty(covNickName)) ? covNickName : strProductType;
                                    drPolicy["ProductType"] = objPolicy.ProductType;
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorList.Add(importedPolicyID);
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: product : " + ex.Message, true);
                            continue;
                        }


                        #endregion

                        #region Common fields - always update with insert/Update
                        bool OutPercentOfPremium = false;
                        try
                        {
                            //Status
                            if (dt.Columns.Contains("Plan Status Description") && dt.Rows[i]["Plan Status Description"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Plan Status Description"])))
                            {
                                string strStatus = dt.Rows[i]["Plan Status Description"].ToString();
                                objPolicy.PolicyStatusId = (strStatus.ToLower() == "active") ? 0 : (strStatus.ToLower() == "pending") ? 2 : 1;
                                drPolicy["PolicyStatusId"] = objPolicy.PolicyStatusId;
                            }
                            else if (dt.Columns.Contains("Current Plan Status") && dt.Rows[i]["Current Plan Status"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Current Plan Status"])))
                            {
                                string strStatus = dt.Rows[i]["Current Plan Status"].ToString();
                                objPolicy.PolicyStatusId = (strStatus.ToLower() == "active") ? 0 : (strStatus.ToLower() == "pending") ? 2 : 1;
                                drPolicy["PolicyStatusId"] = objPolicy.PolicyStatusId;
                            }

                            //Original effective date
                            if (dt.Columns.Contains("Original Plan Start Date"))
                            {
                                if (dt.Rows[i]["Original Plan Start Date"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Original Plan Start Date"])))
                                {
                                    string effDate = Convert.ToString(dt.Rows[i]["Original Plan Start Date"]);
                                    objPolicy.OriginalEffectiveDate = Convert.ToDateTime(effDate);
                                    drPolicy["OriginalEffectiveDate"] = objPolicy.OriginalEffectiveDate;
                                }
                            }
                            //Account Exec
                            if (dt.Columns.Contains("Account Owner: Full Name"))
                            {
                                if (dt.Rows[i]["Account Owner: Full Name"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Account Owner: Full Name"])))
                                {
                                    string acctExec = Convert.ToString(dt.Rows[i]["Account Owner: Full Name"]);
                                    var objUser = AgentList.Where(d => (d.FirstName + " " + d.LastName).ToLower() == acctExec.ToLower()).FirstOrDefault(); //User.GetUserIdWise(tempGuid);// (from p in DataModel.UserCredentials where (p.UserCredentialId == tempGuid && p.RoleId == 3 && p.IsDeleted == false) select p).FirstOrDefault();

                                    Guid tempGuid = Guid.Empty;
                                    Guid.TryParse(acctExec, out tempGuid);
                                    if (tempGuid != Guid.Empty)
                                    {
                                        objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                    }
                                    //Guid tempGuid = new Guid(acctExec);
                                    // User objUser = GlobalAgentList.Where(d => d.UserCredentialID == tempGuid).FirstOrDefault();
                                    if (objUser != null /*&& objUser.Role == UserRole.Agent*/)
                                    {
                                        //Need to get nick name
                                        if (string.IsNullOrEmpty(objUser.NickName))
                                        {
                                            objPolicy.AccoutExec = objUser.NickName;
                                            objPolicy.UserCredentialId = objUser.UserCredentialId; //tempGuid;
                                        }
                                        else
                                        {
                                            objPolicy.AccoutExec = objUser.UserName;
                                            objPolicy.UserCredentialId = objUser.UserCredentialId; //tempGuid;
                                        }
                                        bool isexec = (new User().CheckAccoutExec(objUser.UserCredentialId)); //Sets the flag of accExec true for this userID 
                                        drPolicy["AccoutExec"] = objPolicy.AccoutExec;
                                    }
                                }
                            }
                            //Term Date
                            if (dt.Columns.Contains("Plan End Date"))
                            {
                                if (dt.Rows[i]["Plan End Date"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Plan End Date"])))
                                {
                                    string termDate = Convert.ToString(dt.Rows[i]["Plan End Date"]);
                                    objPolicy.PolicyTerminationDate = Convert.ToDateTime(termDate);
                                    drPolicy["PolicyTerminationDate"] = objPolicy.PolicyTerminationDate;
                                }
                            }
                            //Term reason
                            if (dt.Columns.Contains("Termination Reason"))
                            {
                                if (dt.Rows[i]["Termination Reason"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Termination Reason"])))
                                {
                                    objPolicy.TerminationReasonId = PolicTermisionID(Convert.ToString(dt.Rows[i]["Termination Reason"]));
                                    drPolicy["TerminationReasonId"] = objPolicy.TerminationReasonId;
                                }
                            }
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy: mandatory fields init done : " + policyIDKey, true);
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorList.Add(importedPolicyID);
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: mandatory fields  : " + ex.Message, true);
                            continue;
                        }
                        //Outgoing split 
                        try
                        {
                            #region Outgoing Split
                            if (dt.Columns.Contains("Producer 1: Full Name") && dt.Columns.Contains("Producer 1 First Year %") && dt.Columns.Contains("Producer 1 Renewal %"))
                            {
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 1: Full Name"])))
                                {
                                    string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 1: Full Name"]);

                                    if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                    {
                                        try
                                        {
                                            //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            //  Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            ///var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            Guid tempGuid = Guid.Empty;
                                            Guid.TryParse(strPrimaryBroker, out tempGuid);
                                            if (tempGuid != Guid.Empty)
                                            {
                                                objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                            }

                                            if (objUser != null)
                                            {
                                                string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 1 First Year %"]);
                                                string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 1 Renewal %"]);
                                                OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                                OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                            //    OutgoingRecord.IsPrimaryAgent = true;
                                                OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                                OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                                //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                                OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                                OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for pri1: " + ex.Message, true);
                                            continue;
                                        }
                                    }
                                }
                            }

                            if (dt.Columns.Contains("Producer 2: Full Name") && dt.Columns.Contains("Producer 2 First Year %") && dt.Columns.Contains("Producer 2 Renewal %"))
                            {
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 2: Full Name"])))
                                {
                                    string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 2: Full Name"]);
                                    if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                    {
                                        try
                                        {
                                            //Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            //var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            Guid tempGuid = Guid.Empty;
                                            Guid.TryParse(strPrimaryBroker, out tempGuid);
                                            if (tempGuid != Guid.Empty)
                                            {
                                                objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                            }
                                            if (objUser != null)
                                            {
                                                string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 2 First Year %"]);
                                                string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 2 Renewal %"]);
                                                OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                                OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                                //OutgoingRecord.IsPrimaryAgent = false;
                                                OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                                OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                                //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                                OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                                OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag1: " + ex.Message, true);
                                            continue;
                                        }
                                    }
                                }
                            }

                            if (dt.Columns.Contains("Producer 3: Full Name") && dt.Columns.Contains("Producer 3 First Year %") && dt.Columns.Contains("Producer 3 Renewal %"))
                            {
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 3: Full Name"])))
                                {
                                    string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 3: Full Name"]);
                                    if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                    {
                                        try
                                        {
                                            //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            //   Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            // var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            Guid tempGuid = Guid.Empty;
                                            Guid.TryParse(strPrimaryBroker, out tempGuid);
                                            if (tempGuid != Guid.Empty)
                                            {
                                                objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                            }
                                            if (objUser != null)
                                            {
                                                string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 3 First Year %"]);
                                                string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 3 Renewal %"]);
                                                OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                                OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                           //     OutgoingRecord.IsPrimaryAgent = false;
                                                OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                                OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                                //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                                OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                                OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag2: " + ex.Message, true);
                                            continue;
                                        }
                                    }
                                }
                            }

                            if (dt.Columns.Contains("Producer 4: Full Name") && dt.Columns.Contains("Producer 4 First Year %") && dt.Columns.Contains("Producer 4 Renewal %"))
                            {
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 4: Full Name"])))
                                {
                                    string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 4: Full Name"]);
                                    if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                    {
                                        try
                                        {
                                            //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            //Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            //var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            Guid tempGuid = Guid.Empty;
                                            Guid.TryParse(strPrimaryBroker, out tempGuid);
                                            if (tempGuid != Guid.Empty)
                                            {
                                                objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                            }
                                            if (objUser != null)
                                            {
                                                string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 4 First Year %"]);
                                                string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 4 Renewal %"]);
                                                OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                                OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                             //   OutgoingRecord.IsPrimaryAgent = false;
                                                OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                                OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                                //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                                OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                                OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag3: " + ex.Message, true);
                                            continue;
                                        }
                                    }
                                }
                            }

                            if (dt.Columns.Contains("Producer 5: Full Name") && dt.Columns.Contains("Producer 5 First Year %") && dt.Columns.Contains("Producer 5 Renewal %"))
                            {
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 5: Full Name"])))
                                {
                                    string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 5: Full Name"]);
                                    if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                    {
                                        try
                                        {
                                            //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            //Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            //var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            Guid tempGuid = Guid.Empty;
                                            Guid.TryParse(strPrimaryBroker, out tempGuid);
                                            if (tempGuid != Guid.Empty)
                                            {
                                                objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                            }
                                            if (objUser != null)
                                            {
                                                string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 5 First Year %"]);
                                                string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 5 Renewal %"]);
                                                OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                                OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                         //       OutgoingRecord.IsPrimaryAgent = false;
                                                OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                                OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                                //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                                OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                                OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag4: " + ex.Message, true);
                                            continue;
                                        }
                                    }
                                }
                            }

                            if (dt.Columns.Contains("Producer 6: Full Name") && dt.Columns.Contains("Producer 6 First Year %") && dt.Columns.Contains("Producer 6 Renewal %"))
                            {
                                if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 6: Full Name"])))
                                {
                                    string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 6: Full Name"]);
                                    if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                    {
                                        try
                                        {
                                            //Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            //var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            Guid tempGuid = Guid.Empty;
                                            Guid.TryParse(strPrimaryBroker, out tempGuid);
                                            if (tempGuid != Guid.Empty)
                                            {
                                                objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                            }
                                            if (objUser != null)
                                            {
                                                string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 6 First Year %"]);
                                                string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 6 Renewal %"]);
                                                OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                                OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                                OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                     //           OutgoingRecord.IsPrimaryAgent = false;
                                                OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                                OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                                //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                                OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                                OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag5: " + ex.Message, true);
                                            continue;
                                        }
                                    }
                                }
                            }
                            //Check for House if present
                            //try
                            //{
                            //    string strHouse = Convert.ToString(dt.Rows[i]["houseuc"]);
                            //    string strhousefy = Convert.ToString(dt.Rows[i]["housefy"]);
                            //    string strhousery = Convert.ToString(dt.Rows[i]["housern"]);
                            //    if (!string.IsNullOrEmpty(strHouse))
                            //    {
                            //        OutGoingPayment OutgoingRecord = new OutGoingPayment();
                            //        OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                            //        OutgoingRecord.PolicyId = objPolicy.PolicyId;
                            //        OutgoingRecord.IsPrimaryAgent = true;
                            //        OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strhousefy) ? 0 : Convert.ToDouble(strhousefy);
                            //        OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strhousery) ? 0 : Convert.ToDouble(strhousery);
                            //        //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                            //        //houseOwner = 
                            //        OutgoingRecord.PayeeUserCredentialId = new Guid(strHouse);
                            //        OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                            //        if (OutgoingRecord.FirstYearPercentage > 0 || OutgoingRecord.RenewalPercentage > 0)
                            //            OutGoingField.Add(OutgoingRecord);
                            //    }
                            //}
                            //catch (Exception ex)
                            //{
                            //    ActionLogger.Logger.WriteImportLogDetail("Exception setting house owner: " + ex.Message + ", so chcking frim licensee ", true);
                            //    continue;
                            //}
                            #endregion

                        #endregion
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy: outgoing split init done : " + policyIDKey, true);
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorList.Add(importedPolicyID);
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: outgoing fields  : " + ex.Message, true);
                            continue;
                        }
                        #region Save Data
                        try
                        {
                            if (errorList == null || !errorList.Contains(importedPolicyID))
                            {
                                #region Client
                                try
                                {

                                    if (isNewPolicy || objPolicy.PolicyClientId == null)
                                    {
                                        string client = Convert.ToString(dt.Rows[i]["Account Name"]);
                                        Client objClient = (new Client()).GetClientByClientName(client, LicID);
                                        //Get Client ID by Get Client name
                                        if (objClient == null)
                                        {
                                            //Create new client
                                            objClient = new Client();
                                            objClient.ClientId = Guid.NewGuid();
                                            string strClientValue = string.Empty;

                                            strClientValue = (client.Length > 49) ? client.Substring(0, 49) : client;

                                            //objClnt.Name = policy.ClientName;
                                            objClient.Name = strClientValue;
                                            objClient.LicenseeId = LicID;
                                            objClient.IsDeleted = false;
                                            Client.AddUpdateClient(client, LicID, objClient.ClientId);
                                            ActionLogger.Logger.WriteImportLogDetail("Import Policy: client saved as new : " + policyIDKey, true);
                                        }
                                        objPolicy.ClientReference.Value = (from p in DataModel.Clients where p.ClientId == objClient.ClientId select p).FirstOrDefault();
                                    }
                                    //following requires client reference, so added at last
                                    if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Insured))
                                    {
                                        if (dt.Columns.Contains("insured?"))
                                        {
                                            objPolicy.Insured = Convert.ToString(dt.Rows[i]["insured"]);
                                        }
                                        else
                                        {
                                            objPolicy.Insured = objPolicy.Client.Name;
                                        }
                                        drPolicy["Insured"] = objPolicy.Insured;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    errorList.Add(importedPolicyID);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: client : " + ex.Message, true);
                                    continue;
                                }
                                #endregion
                                if (isNewPolicy)
                                {
                                    InsertIntoMembers(dtPolicies);
                                    //DataModel.AddToPolicies(objPolicy);
                                    //DataModel.SaveChanges();
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy : policy saved successfully", true);
                                    PolicyToLearnPost.AddLearnedAfterImport(objPolicy.PolicyId, "", covNickName, strProductType, importedPolicyID);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy : learned fields saved successfully", true);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: Policy saved as new : " + policyIDKey, true);

                                }
                                else
                                {
                                    DataModel.SaveChanges();
                                    AddUpdatePolicyHistory(objPolicy.PolicyId);
                                    PolicyLearnedField.AddUpdateHistoryLearned(objPolicy.PolicyId);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: Policy updated : " + policyIDKey, true);
                                }


                                //Save Incoming 
                                if (inSchedule != null)
                                {
                                    inSchedule.AddUpdate();
                                    ActionLogger.Logger.WriteImportLogDetail("Incoming schedule added for the policy", true);
                                }

                                //Save Outgoing
                                //Delete old schdule- outgoing  payments witj old schedule will remian intact, new will take effect on new entries only.
                                //Check only if new outgoing schedule exists, then overwrite the old
                                //This is to exclude the case when policy is existing and excel has blank entries. Without this, existing schedule will be overwritten
                                // by 100% to house for existing policy 
                                if (isNewPolicy || OutGoingField.Count > 0)
                                {
                                    ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule adding to the policy", true);
                                    OutGoingField = CompleteOutgoingSchedule(OutGoingField, objPolicy.PolicyId, LicID);
                                    OutGoingPayment.DeletePolicyOutGoingSchedulebyPolicyId(objPolicy.PolicyId);
                                    ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule removed from the policy", true);
                                    OutGoingPayment.AddUpdate(OutGoingField);
                                    ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule added for the policy", true);
                                }

                                if (isNewPolicy) addCount++;
                                else updateCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errorList.Add(importedPolicyID);
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: saving data fields  : " + ex.Message, true);
                            continue;
                        }
                        #endregion

                    }
                    catch (Exception ex)
                    {
                        ActionLogger.Logger.WriteImportLogDetail("Exception adding policy: " + ex.Message, true);
                        errorCount++;
                        errorList.Add(importedPolicyID);
                    }
                }
            }

            return objStatus;
        }

        static OutGoingPayment GetOutgoingPaymentForImport(string agent, Guid userID, Guid LicID, string firstYear, string renewal, Guid policyId, bool OutPercentOfPremium)
        {
            try
            {
                ActionLogger.Logger.WriteImportLogDetail("GetOutgoingPaymentForImport request: " + agent, true);
                 char[] spCharac = System.Configuration.ConfigurationSettings.AppSettings["AgentCharactersToTrim"].ToCharArray();  //{ '(', '[', '-' };

                 int spIndex = agent.IndexOfAny(spCharac);
                agent = (spIndex > 0) ? agent.Substring(0, spIndex - 1) : agent;
                ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " strPrimaryBroker: " + agent, true);

      
              //  var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == agent.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == agent.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == agent.ToLower())).FirstOrDefault();
               //  Guid userID = Guid.Empty;
                                        ////Check if incoming value is Guid 
                                        //if (userID == Guid.Empty)
                                        //{
                                        //    //Guid tempGuid = Guid.Empty;
                                        //    //Guid.TryParse(agent, out tempGuid);
                                        //    //if (tempGuid != Guid.Empty)
                                        //    //{
                                        //    //    objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                        //    //}
                                        //    //  //If still null, create new 
                                        //    //if (objUser == null)
                                        //    //{
                                        //         userID  = AddNewUserFromBenefits(agent, LicID);
                                        //    //}
                                        //}
                                        ////else
                                        ////{
                                        ////    userID = objUser.UserCredentialId;
                                        ////}
                                        // ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " userID found as: " + userID, true);
                                      

                                      //   if (objUser != null)
                                         if (userID != Guid.Empty)
                                         {

                                             OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                             OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                             OutgoingRecord.PolicyId = policyId;
                                           
                                             double frst = 0; double renew = 0;
                                             double.TryParse(firstYear, out frst);
                                             double.TryParse(renewal, out renew);

                                             OutgoingRecord.FirstYearPercentage = frst;// string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                             OutgoingRecord.RenewalPercentage = renew;// string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                             //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                             OutgoingRecord.PayeeUserCredentialId = userID;// objUser.UserCredentialId;
                                             OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                             return OutgoingRecord;
                                             //  OutGoingField.Add(OutgoingRecord);
                                         }
                                         else
                                         {
                                             return null;
                                         }
              
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail("GetOutgoingPaymentForImport exception: " + ex.Message, true);
                return null;
            }
        }

        public static Benefits_PolicyImportStatus ImportPolicy_Benefits(DataTable dt, Guid LicID, ObservableCollection<CompType> CompTypeList)
        {
            #region Variables
            Benefits_PolicyImportStatus objStatus = new Benefits_PolicyImportStatus();

            /* Following to be used in case   1-many mapping exists as anticipated before, not in use for now 
            * 
            * List<string> lstKeys = System.Configuration.ConfigurationSettings.AppSettings["BenefitsPolicyKeys"].Split(',').ToList<String>();
              string policyIDKey = "";
              foreach (string s in lstKeys)
              {
                  if (dt.Columns.Contains(s))
                  {
                      policyIDKey = s;
                      break;
                  }
              }*/
            
            string policyIDKey = System.Configuration.ConfigurationSettings.AppSettings["PolicyIDKeyName_Benefits"]; // "OriginalPlanID";

            ActionLogger.Logger.WriteImportLogDetail("Import Policy: policyIDKey: " + policyIDKey, true);
            char[] spCharac = System.Configuration.ConfigurationSettings.AppSettings["AgentCharactersToTrim"].ToCharArray();  //{ '(', '[', '-' };

            //Response object structure
            int addCount = 0;
            int updateCount = 0;
            int errorCount = 0;
            List<Benefits_ErrorMsg> errorList = new List<Benefits_ErrorMsg>();


            List<OutGoingPayment> OutGoingField = new List<OutGoingPayment>();
            PolicyToolIncommingShedule inSchedule = null;
            string strProductType = string.Empty;
            string covNickName = string.Empty;
            Guid houseOwner = Guid.Empty;

            #endregion

            #region Agents List
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Data model init", true);
                var AgentList = (from p in DataModel.UserCredentials
                                 join o in DataModel.UserDetails on p.UserCredentialId equals o.UserCredentialId
                                 where p.LicenseeId == LicID /*&& p.RoleId == 3*/ && p.IsDeleted == false
                                 select new
                                 {
                                     p.UserCredentialId,
                                     o.NickName,
                                     p.UserName,
                                     p.RoleId,
                                     o.FirstName,
                                     o.LastName
                                 }).ToList();
                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Agent list fetched", true);

            #endregion

                #region Get Agency's track date default
                DateTime? dtTrack = DateTime.MinValue;
                try
                {
                    var strTrack = (from l in DataModel.Licensees where l.LicenseeId == LicID select new { l.TrackDateDefault }).FirstOrDefault();
                    if (strTrack != null)
                    {
                        if(strTrack.TrackDateDefault != null)// && strTrack.TrackDateDefault > 
                        dtTrack = strTrack.TrackDateDefault;
                    }
                    ActionLogger.Logger.WriteImportLogDetail("Import Policy trackDatefrom agency: " + dtTrack, true);
                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteImportLogDetail("track date calculation failed" + ex.Message, true);
                }

                #endregion

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DLinq.Policy objPolicy = new DLinq.Policy();
                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: Iteration: " + i, true);
                    
                    string importedPolicyID = dt.Columns.Contains(policyIDKey) ? Convert.ToString(dt.Rows[i][policyIDKey]).Trim() : "";// (dt.Columns.Contains("Life Insurance Plan ID") ? Convert.ToString(dt.Rows[i]["Life Insurance Plan ID"]).Trim() : "");
                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: importedPolicyID: " + importedPolicyID, true);
                    
                    string benefits_policyID = string.Empty;
                    if (dt.Columns.Contains("PolicyPlanID")) //|| dt.Columns.Contains("New Business?"))
                    {
                        benefits_policyID = Convert.ToString(dt.Rows[i]["PolicyPlanID"]);
                    }

                    Dictionary<string, string> errMsgPolicy = new Dictionary<string, string>();
                    //List<Benefits_ErrorMsg.errMsg> errMsgPolicy = new List<Benefits_ErrorMsg.errMsg>();
                   // List<string> errMsgPolicy = new List<string>();
                    //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, errMsgPolicy);



                    if (string.IsNullOrEmpty(importedPolicyID))
                    {
                        errMsgPolicy.Add(importedPolicyID, "Unique policy ID found missing");
                        ActionLogger.Logger.WriteImportLogDetail("Import Policy: Policy ID found null/blank, skipping record", true);
                        AddImportStatusToDB(importedPolicyID, false, false, benefits_policyID);
                       // continue;
                    }

                    OutGoingField = new List<OutGoingPayment>();
                    bool isNewPolicy = false;
                    //try
                    //{
                      

                        Guid policyID = IsPolicyExistingWithImportID(importedPolicyID, LicID);
                        ActionLogger.Logger.WriteImportLogDetail("Import Policy: policyID: " + policyID + ", benefits ID: " + benefits_policyID, true);
                        if (policyID != Guid.Empty)
                        {
                            objPolicy = (from p in DataModel.Policies where p.PolicyId == policyID select p).FirstOrDefault();
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy: Existing policy", true);


                            //check if client exists or deleted in the system
                            //if deleted, then add the given as new 
                            if (objPolicy.Client != null && Convert.ToBoolean(objPolicy.Client.IsDeleted))
                            {
                                objPolicy.PolicyClientId = null;
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Client found deleted , so setting as null ", true);
                            }
                        }
                        else
                        {
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy: New policy", true);
                            isNewPolicy = true;
                            objPolicy.PolicyId = Guid.NewGuid();
                            objPolicy.IsTrackPayment = true;
                            objPolicy.PolicyLicenseeId = LicID;
                            objPolicy.TerminationReasonId = null;
                            objPolicy.IsTrackMissingMonth = true;
                            objPolicy.CreatedOn = DateTime.Today;
                            objPolicy.IsIncomingBasicSchedule = true;
                            objPolicy.IsOutGoingBasicSchedule = true;
                            objPolicy.CreatedBy = new Guid("AA38DF84-2E30-43CA-AED3-7276224D1B7E");
                            objPolicy.IsDeleted = false;
                        }

                        #region Fields that should be updated when blank or with new policy
                        //try
                        //{

                        if (isNewPolicy || string.IsNullOrEmpty(objPolicy.PolicyType))
                        {
                            try
                            {
                                if (dt.Columns.Contains("NewBusiness")) //|| dt.Columns.Contains("New Business?"))
                                {
                                    string strTypeOfPolicy = Convert.ToString(dt.Rows[i]["NewBusiness"]); //dt.Columns.Contains("New?") ? Convert.ToString(dt.Rows[i]["New?"]) : (dt.Columns.Contains("New Business?") ? Convert.ToString(dt.Rows[i]["New Business?"]) : "");
                                    objPolicy.PolicyType = (!string.IsNullOrEmpty(strTypeOfPolicy) && (strTypeOfPolicy.ToLower() == "rewrite" || strTypeOfPolicy.ToLower() == "replace")) ? "Replace" : "New";
                                }
                                else if (isNewPolicy)
                                {
                                    objPolicy.PolicyType = "New";
                                }
                            }
                            catch (Exception ex)
                            {
                                errMsgPolicy.Add("NewBusiness", ex.Message);
                            //errorCount++;
                            ////errorList.Add(importedPolicyID, "New Business field: " + ex.Message);
                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "NewBusiness: " + ex.Message);
                            //errorList.Add(m);
                                 ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: New Business field  exception : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false,benefits_policyID);
                                //continue;
                            }
                        }
                       
                        if (isNewPolicy || string.IsNullOrEmpty(objPolicy.PolicyNumber))
                        {
                            try
                            {
                                if (dt.Columns.Contains("Group"))
                                {
                                    objPolicy.PolicyNumber = Convert.ToString(dt.Rows[i]["Group"]);
                                }
                            }
                            catch (Exception ex)
                            {
                                errMsgPolicy.Add("Group", ex.Message);
                            //errorCount++;
                            ////errorList.Add(importedPolicyID, "Group field: " + ex.Message);
                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Group: " + ex.Message);
                            //errorList.Add(m);
                                 ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: Group fields exception : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                               // continue;
                            }
                        }
                        
                        objPolicy.SubmittedThrough = ""; //Not to be sent by benefits and to be left blank as per kevin Aug 17, 2017
                       
                        if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Enrolled))
                        {
                            try
                            {
                                if (dt.Columns.Contains("NumberofCoveredLives"))
                                {
                                    objPolicy.Enrolled = Convert.ToString(dt.Rows[i]["NumberofCoveredLives"]);
                                }
                            }
                            catch (Exception ex)
                            {
                                errMsgPolicy.Add("NumberofCoveredLives", ex.Message);
                                //errorCount++;
                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "NumberofCoveredLives: " + ex.Message);
                                //errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: NumberofCoveredLives fields exception : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
                        }
                        if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Eligible))
                        {
                            try
                            {
                                if (dt.Columns.Contains("Eligible"))
                                {
                                    objPolicy.Eligible = Convert.ToString(dt.Rows[i]["Eligible"]);
                                }
                            }
                            catch (Exception ex)
                            {
                                errMsgPolicy.Add("Eligible", ex.Message);
                            //errorCount++;
                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Eligible: " + ex.Message);
                            //errorList.Add(m);
                             ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: Eligible fields exception : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
                        }

                        if (isNewPolicy || (objPolicy.Advance == null))
                        {
                            try
                            {
                                if (dt.Columns.Contains("AdvancedPaymentNumber"))
                                {
                                    objPolicy.Advance = (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["AdvancedPaymentNumber"]))) ? Convert.ToInt32(Convert.ToString(dt.Rows[i]["AdvancedPaymentNumber"])) : 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                errMsgPolicy.Add("AdvancedPaymentNumber", ex.Message);
                            //errorCount++;
                            //    Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "AdvancedPaymentNumber: " + ex.Message);
                            //    errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: AdvancedPaymentNumber fields exception : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
                        }
                        ActionLogger.Logger.WriteImportLogDetail("Import Policy: optional fields , to be filled when blanks init done : " + policyIDKey, true);
                       
                        //Incoming Schedule 
                        #region Incoming Schedule
                        bool oldInScheduleExists = false;
                        DLinq.PolicyIncomingSchedule tempSched = null;
                        //Check if incoming is already set with 0 splits
                        if (objPolicy.IsIncomingBasicSchedule == true)
                        {
                            tempSched = objPolicy.PolicyIncomingSchedules.FirstOrDefault();
                            if (tempSched != null)
                            {
                                if (tempSched.FirstYearPercentage > 0 || tempSched.RenewalPercentage > 0)
                                {
                                    oldInScheduleExists = true;
                                }
                               
                            }
                        }

                        if (isNewPolicy || (objPolicy.IsIncomingBasicSchedule != true) || (!oldInScheduleExists))
                        {
                            try
                            {
                                if (dt.Columns.Contains("CommissionType") && dt.Columns.Contains("PaymentType") && dt.Columns.Contains("CommissionsFirstYear") && dt.Columns.Contains("CommissionsRenewal") && dt.Columns.Contains("CoBrokerSplit"))
                                {
                                    //if (tempSched != null && tempSched.FirstYearPercentage == 0 && tempSched.RenewalPercentage == 0)
                                    //{
                                    //    PolicyToolIncommingShedule.DeleteSchedule(objPolicy.PolicyId); //If schedule occurs with 0 split, then delete before adding new
                                    //    ActionLogger.Logger.WriteImportLogDetail("Import Policy old schedule is deleted", true);
                                    //}


                                    string strCommisionType = Convert.ToString(dt.Rows[i]["CommissionType"]);
                                    objPolicy.IncomingPaymentTypeId = PolicCompType(strCommisionType, CompTypeList);
                                    string strOutPercentOfPremium = Convert.ToString(dt.Rows[i]["PaymentType"]);
                                    string strInFirstYear = Convert.ToString(dt.Rows[i]["CommissionsFirstYear"]);
                                    string strInRenewYear = Convert.ToString(dt.Rows[i]["CommissionsRenewal"]);
                                    string strSplit = Convert.ToString(dt.Rows[i]["CoBrokerSplit"]);
                                    double splitPer = 0;
                                    double.TryParse(strSplit, out splitPer);
                                    objPolicy.SplitPercentage = splitPer;

                                    inSchedule = new PolicyToolIncommingShedule();
                                    inSchedule.PolicyId = objPolicy.PolicyId;
                                    inSchedule.IncomingScheduleId = Guid.NewGuid();
                                    inSchedule.ScheduleTypeId = (!string.IsNullOrEmpty(strOutPercentOfPremium) && strOutPercentOfPremium.ToLower().Contains("head")) ? 2 : 1;
                                    double frst = 0; double renew = 0;
                                    double.TryParse(strInFirstYear, out frst);
                                    double.TryParse(strInRenewYear, out renew);
                                    inSchedule.FirstYearPercentage = frst; //(!string.IsNullOrEmpty(strInFirstYear)) ? Convert.ToDouble(strInFirstYear) : 0;
                                    inSchedule.RenewalPercentage = renew;// (!string.IsNullOrEmpty(strInRenewYear)) ? Convert.ToDouble(strInRenewYear) : 0;
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: Incoming schedule init done : " + policyIDKey, true);
                                }
                                else
                                {
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: Incoming schedule not found in excel", true);
                                }
                            }
                            catch (Exception ex)
                            {
                              errMsgPolicy.Add("CommissionType", ex.Message);
                            //errorCount++;
                            //    Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "CommissionType: Incoming Schedule fields error -  " + ex.Message);
                            //    errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: Incoming Schedule fields exception : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
                        }
                        #endregion


                       
                        //Mode
                        if (isNewPolicy || objPolicy.PolicyModeId == null)
                        {
                            try
                            {
                                if (dt.Columns.Contains("ModalNumber"))
                                {
                                    string strMode = Convert.ToString(dt.Rows[i]["ModalNumber"]);
                                    objPolicy.PolicyModeId = (!string.IsNullOrEmpty(strMode)) ? PolicyModeID(strMode) : PolicyModeID("0");
                                }
                            }
                            catch (Exception ex)
                            {
                                errMsgPolicy.Add("ModalNumber", ex.Message);
                                //errorCount++;
                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "ModalNumber: " + ex.Message);
                                //errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: ModalNumber fields exception : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
                        }

                        //premium
                        if (isNewPolicy || objPolicy.MonthlyPremium == null)
                        {
                            try
                            {
                                if (dt.Columns.Contains("ModalPremium"))
                                {
                                    string strPremiuum = Convert.ToString(dt.Rows[i]["ModalPremium"]);
                                    decimal prem = 0;
                                    decimal.TryParse(strPremiuum, out prem);
                                    if (prem == 0)
                                    {
                                        decimal.TryParse(strPremiuum, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"), out prem);
                                    }
                                    objPolicy.MonthlyPremium = prem;// (!string.IsNullOrEmpty(strPremiuum)) ? Convert.ToDecimal(strPremiuum) : 0;
                                }
                            }
                            catch (Exception ex)
                            {
                                 errMsgPolicy.Add("ModalPremium", ex.Message);
                                //errorCount++;
                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "ModalPremium: " + ex.Message);
                            //errorList.Add(m);
                                  ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: ModalPremium fields exception : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
                        }
                        
                        //Payor  
                        #region Payor
                        //try
                        //{
                        if (isNewPolicy || objPolicy.PayorId == null)
                        {
                            if (dt.Columns.Contains("PayorCommissionDept"))
                            {
                                try
                                {
                                    string strPayor = Convert.ToString(dt.Rows[i]["PayorCommissionDept"]);
                                    if (!string.IsNullOrEmpty(strPayor))
                                    {
                                        //Compare payors - if name matches -  payor to be global or belonging to same agency.
                                        DLinq.Payor py = (from p in DataModel.Payors where ((p.PayorName.ToLower() == strPayor.ToLower() ||  (p.PayorName.ToLower() != strPayor.ToLower() && p.NickName.ToLower() == strPayor.ToLower())))
                                                          && ((p.IsGlobal || (!p.IsGlobal && p.LicenseeId == LicID)))
                                                          select p).FirstOrDefault();
                                        //if (py == null)
                                       // {
                                            if (py != null) //If found existing 
                                            {
                                                objPolicy.PayorId = py.PayorId;
                                                objPolicy.PayorReference.Value = py;
                                            }
                                            else  // if not existing, return error  
                                            {
                                               errMsgPolicy.Add("PayorCommissionDept", "Payor not available");
                                        //errorCount++;
                                        //        Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "PayorCommissionDept: Payor not available");
                                        //        errorList.Add(m);
                                                ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: payor fields exception : Payor not available", true);
                                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                                
                                            }

                                       // }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    errMsgPolicy.Add("PayorCommissionDept", ex.Message);
                                //errorCount++;
                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "PayorCommissionDept: " + ex.Message);
                                //errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: payor fields exception : " + ex.Message, true);
                                    AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                    //continue;
                                }
                            }

                        }
                        #endregion

                        #region Carrier
                        if (isNewPolicy || objPolicy.CarrierId == null)
                        {
                            if (dt.Columns.Contains("CarrierCommissionDept"))
                            {
                                try
                                {
                                    string strCarr = Convert.ToString(dt.Rows[i]["CarrierCommissionDept"]);
                                    if (!string.IsNullOrEmpty(strCarr))
                                    {
                                        DLinq.Carrier cr = (from p in DataModel.Carriers where ( p.CarrierName.ToLower() == strCarr.ToLower() &&  (p.IsGlobal || (!p.IsGlobal && p.LicenseeId == LicID) )  )select p).FirstOrDefault();
                                        if (cr == null)
                                        {
                                            DLinq.CarrierNickName crN = (from p in DataModel.CarrierNickNames where p.NickName == strCarr select p).FirstOrDefault();
                                            // if (crN == null)
                                            //{
                                            //    crN = (from p in DataModel.CarrierNickNames where p.NickName == strCarr select p).FirstOrDefault();
                                            //}
                                            if (crN != null) //Carrier found in system by nickname
                                            {
                                                objPolicy.CarrierId = crN.CarrierId;
                                                objPolicy.CarrierReference.Value = crN.CarrierReference.Value;
                                            }
                                            else
                                            {
                                                errMsgPolicy.Add("CarrierCommissionDept", "Carrier not available");
                                            //errorCount++;
                                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "CarrierCommissionDept: Carrier not available");
                                            //errorList.Add(m);
                                            ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: carrier fields exception : carrier not available", true);
                                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                                
                                            }
                                        }
                                        else  //Carrier found by name
                                        {
                                            objPolicy.CarrierId = cr.CarrierId;
                                            objPolicy.CarrierReference.Value = cr;
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    errMsgPolicy.Add("CarrierCommissionDept", ex.Message);
                                    //errorCount++;
                                    //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "CarrierCommissionDept: " + ex.Message);
                                    //errorList.Add(m);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: carrier fields exception : " + ex.Message, true);
                                    AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                    //continue;
                                }
                            }
                        }
                   // }
                        #endregion
                    
                    #region Product
                    if (isNewPolicy || objPolicy.CoverageId == null)
                    {
                        if (dt.Columns.Contains("LineOfCoverage"))
                        {
                            try
                            {
                                string strProduct = Convert.ToString(dt.Rows[i]["LineOfCoverage"]);
                                if (!string.IsNullOrEmpty(strProduct))
                                {
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy adding LineOfCoverage : " + strProduct, true);
                                    DLinq.Coverage cov = (from p in DataModel.Coverages where p.ProductName == strProduct select p).FirstOrDefault();
                                    if (cov != null) // When product found
                                    {
                                        objPolicy.CoverageId = cov.CoverageId;
                                        objPolicy.CoverageReference.Value = cov;
                                    }
                                    else
                                    {
                                        errMsgPolicy.Add("LineOfCoverage", "Coverage not available");
                                        //errMsgPolicy.Add(new Benefits_ErrorMsg.errMsg("LineOfCoverage", "Coverage not available"));
                                        //errorCount++;
                                        //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "LineOfCoverage: Coverage not available");
                                        //errorList.Add(m);
                                        ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: Coverage fields exception : Coverage  not available", true);
                                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                        //continue;

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                
                                errMsgPolicy.Add("LineOfCoverage", ex.Message);
                                //errorCount++;
                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "LineOfCoverage: " + ex.Message);
                                //errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: LineOfCoverage : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
                        }
                    }
                    #endregion

                    #region Product Type - Not to be sent by benefits
                    /*Not to be sent by benefits 
                             * if (isNewPolicy || string.IsNullOrEmpty(objPolicy.ProductType))
                            {
                                if (dt.Columns.Contains("Product_Type"))
                                {
                                    strProductType = Convert.ToString(dt.Rows[i]["Product_Type"]);
                                    DLinq.CoverageNickName covName = (from p in DataModel.CoverageNickNames where p.NickName == strProductType select p).FirstOrDefault();
                                    covNickName = (covName != null) ? covName.NickName : string.Empty; //to be used later in code, so kept separate
                                    objPolicy.ProductType = (!string.IsNullOrEmpty(covNickName)) ? covNickName : strProductType;
                                }
                            }*/
                    #endregion
                    

                        #endregion

                    #region Common fields - always update with insert/Update
                    bool OutPercentOfPremium = false;
                    //try
                    //{
                    //Status
                    if (dt.Columns.Contains("PlanStatusDescription") && dt.Rows[i]["PlanStatusDescription"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["PlanStatusDescription"])))
                    {
                        try
                        {
                            string strStatus = dt.Rows[i]["PlanStatusDescription"].ToString();
                            objPolicy.PolicyStatusId = (strStatus.ToLower() == "active") ? 0 : (strStatus.ToLower() == "pending") ? 2 : 1;
                        }
                        catch (Exception ex)
                        {
                            errMsgPolicy.Add("PlanStatusDescription", ex.Message);
                            //errorCount++;
                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "PlanStatusDescription: " + ex.Message);
                            //errorList.Add(m);
                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: PlanStatusDescription fields  : " + ex.Message, true);
                            AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                            //continue;
                        }
                    }
                    //else if (dt.Columns.Contains("Current Plan Status") && dt.Rows[i]["Current Plan Status"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Current Plan Status"])))
                    //{
                    //    string strStatus = dt.Rows[i]["Current Plan Status"].ToString();
                    //    objPolicy.PolicyStatusId = (strStatus.ToLower() == "active") ? 0 : (strStatus.ToLower() == "pending") ? 2 : 1;
                    //}

                    //Original effective date
                    if (dt.Columns.Contains("OriginalPlanStartDate"))
                    {
                        if (dt.Rows[i]["OriginalPlanStartDate"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["OriginalPlanStartDate"])))
                        {
                            try
                            {
                                string effDate = Convert.ToString(dt.Rows[i]["OriginalPlanStartDate"]);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy string effDate: " + effDate, true);
                                //check if value in double, then fetch OA Date
                                double dblEff = 0;
                                Double.TryParse(effDate, out dblEff);
                                if (dblEff > 0)
                                {
                                    objPolicy.OriginalEffectiveDate = DateTime.FromOADate(dblEff);
                                }
                                else
                                {
                                    objPolicy.OriginalEffectiveDate = DateTime.Parse(effDate, System.Globalization.CultureInfo.CurrentCulture); //Convert.ToDateTime(effDate);
                                }
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy datetime effDate: " + objPolicy.OriginalEffectiveDate, true);
                            }
                            catch (Exception ex)
                            {
                                errMsgPolicy.Add("OriginalPlanStartDate", ex.Message);
                                //errorCount++;
                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "OriginalPlanStartDate: " + ex.Message);
                                //errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: OriginalPlanStartDate  fields  : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
                        }
                    }
                    //Account Exec
                    if (dt.Columns.Contains("AccountOwnerName"))
                    {
                        if (dt.Rows[i]["AccountOwnerName"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["AccountOwnerName"])))
                        {
                            try
                            {
                                string acctExec = Convert.ToString(dt.Rows[i]["AccountOwnerName"]).Trim();
                                var objUser = AgentList.FirstOrDefault(d => (d.FirstName + " " + d.LastName).ToLower() == acctExec.ToLower() || (!string.IsNullOrEmpty(d.NickName) && d.NickName.ToLower() == acctExec.ToLower()));//.FirstOrDefault(); //User.GetUserIdWise(tempGuid);// (from p in DataModel.UserCredentials where (p.UserCredentialId == tempGuid && p.RoleId == 3 && p.IsDeleted == false) select p).FirstOrDefault();

                                Guid tempGuid = Guid.Empty;
                                Guid.TryParse(acctExec, out tempGuid);
                                if (tempGuid != Guid.Empty)
                                {
                                    objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                }
                                //Guid tempGuid = new Guid(acctExec);
                                // User objUser = GlobalAgentList.Where(d => d.UserCredentialID == tempGuid).FirstOrDefault();
                                if (objUser != null /*&& objUser.Role == UserRole.Agent*/)
                                {
                                    ActionLogger.Logger.WriteImportLogDetail("Account owner found in system", true);
                                    //Need to get nick name
                                    if (!string.IsNullOrEmpty(objUser.NickName))
                                    {
                                        objPolicy.AccoutExec = objUser.NickName;
                                        objPolicy.UserCredentialId = objUser.UserCredentialId; //tempGuid;
                                    }
                                    else
                                    {
                                        objPolicy.AccoutExec = objUser.UserName;
                                        objPolicy.UserCredentialId = objUser.UserCredentialId; //tempGuid;
                                    }
                                    bool isexec = (new User().CheckAccoutExec(objUser.UserCredentialId)); //Sets the flag of accExec true for this userID 
                                }
                                else //User not in system, need to add 
                                {
                                    errMsgPolicy.Add("AccountOwnerName", "AccountOwner not available");

                                    //Benefits_ErrorMsg.errMsg m = new Benefits_ErrorMsg.errMsg("AccountOwnerName", "AccountOwner not available");
                                    //var output = Newtonsoft.Json.JsonConvert.SerializeObject(m);
                                    //errMsgPolicy.Add(output);
                                    //errMsgPolicy.Add(new Benefits_ErrorMsg.errMsg("AccountOwnerName", "AccountOwner not available"));
                                    //errorCount++;
                                    //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "AccountOwnerName: AccountOwner not available: ");
                                    //errorList.Add(m);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: AccountOwnerName  fields  : ", true);
                                    AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                    //continue;

                                    //ActionLogger.Logger.WriteImportLogDetail("Account owner not found in system, adding new", true);
                                    //Guid id =  AddNewUserFromBenefits(acctExec, LicID, true);
                                    //if (id == Guid.Empty)
                                    //{
                                    //    errorCount++;
                                    //    Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "AccountOwnerName field error for " + acctExec);
                                    //    errorList.Add(m);
                                    //    ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: AccountOwnerName  could not be added  ", true);
                                    //    AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                    //    continue;
                                    //}
                                    //else
                                    //{
                                    //    objPolicy.AccoutExec = acctExec;
                                    //    objPolicy.UserCredentialId = id;
                                    //}
                                }
                            }
                            catch (Exception ex)
                            {
                                errMsgPolicy.Add("AccountOwnerName", ex.Message);
                                //errorCount++;
                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "AccountOwnerName: " + ex.Message);
                                //errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: AccountOwnerName  fields  : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                //continue;
                            }
        
                        }
                    }
                    //Term Date
                    if (objPolicy.PolicyStatusId == 1)
                    {
                        ActionLogger.Logger.WriteImportLogDetail("Import Policy: status is terminated, so updating plan end date and reason ", true);
                        if (dt.Columns.Contains("PlanEndDate"))
                        {
                            if (dt.Rows[i]["PlanEndDate"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["PlanEndDate"])))
                            {
                                try
                                {
                                    string termDate = Convert.ToString(dt.Rows[i]["PlanEndDate"]);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy string termDate: " + termDate, true);
                                    //check if value in double, then fetch OA Date
                                    double dblTerm = 0;
                                    Double.TryParse(termDate, out dblTerm);
                                    if (dblTerm > 0)
                                    {
                                        objPolicy.PolicyTerminationDate = DateTime.FromOADate(dblTerm);
                                    }
                                    else
                                    {
                                        objPolicy.PolicyTerminationDate = Convert.ToDateTime(termDate);
                                    }
                                }
                                //  objPolicy.PolicyTerminationDate = Convert.ToDateTime(termDate);
                                catch (Exception ex)
                                {
                                    errMsgPolicy.Add("PlanEndDate",ex.Message);
                                    //errorCount++;
                                    //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "PlanEndDate: " + ex.Message);
                                    //errorList.Add(m);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: PlanEndDate  fields  : " + ex.Message, true);
                                    AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                    //continue;
                                }
                            }

                        }
                        //Term reason
                        if (dt.Columns.Contains("TerminationReason"))
                        {
                            if (dt.Rows[i]["TerminationReason"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["TerminationReason"])))
                            {
                                try
                                {
                                    objPolicy.TerminationReasonId = PolicTermisionID(Convert.ToString(dt.Rows[i]["TerminationReason"]));
                                }
                                catch (Exception ex)
                                {
                                    errMsgPolicy.Add("TerminationReason", ex.Message);
                                    //errorCount++;
                                    //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "TerminationReason: " + ex.Message);
                                    //errorList.Add(m);
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: TerminationReason  fields  : " + ex.Message, true);
                                    AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                    //continue;
                                }
                            }
                        }
                    }
                    else
                    {
                        ActionLogger.Logger.WriteImportLogDetail("Import Policy: status is NOT terminated, so ignoring plan end date and reason ", true);
                    }
                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: mandatory fields init done : " + policyIDKey, true);
                    
                    //Outgoing split 
                    try
                    {
                        #region Outgoing Split
                        if (dt.Columns.Contains("Producer1Name") && dt.Columns.Contains("Producer1FirstYear") && dt.Columns.Contains("Producer1Renewal"))
                        {
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer1Name"])))
                            {
                                string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer1Name"]).Trim();
                                    try
                                    {

                                        Guid userID = Guid.Empty;
                                         ////Check user on name
                                    var objUser = AgentList.Where(u => (u.RoleId==3 && (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower()))).FirstOrDefault();
                                    if (objUser == null)
                                    {
                                        errMsgPolicy.Add("Producer1Name", "Producer1 not available in the system");
                                        //errorCount++;
                                        //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer1Name: Producer1 not available in the system ");
                                        //errorList.Add(m);
                                        ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer1  not available in the system ", true);
                                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                        //continue;
                                    }
                                    else
                                    {
                                        userID = objUser.UserCredentialId;
                                        OutGoingPayment OutgoingRecord = GetOutgoingPaymentForImport(strPrimaryBroker, userID, LicID, Convert.ToString(dt.Rows[i]["Producer1FirstYear"]), Convert.ToString(dt.Rows[i]["Producer1Renewal"]), objPolicy.PolicyId, OutPercentOfPremium);
                                        if (OutgoingRecord != null)
                                        {
                                            objPolicy.PrimaryAgent = userID;
                                            //OutgoingRecord.IsPrimaryAgent = true;
                                            OutGoingField.Add(OutgoingRecord);
                                        }
                                        else
                                        {
                                            errMsgPolicy.Add("Producer1Name", "Producer1 not available in the system");
                                            //errorCount++;
                                            //    Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer1Name: Producer1 not available in the system ");
                                            //    errorList.Add(m);
                                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer1  not available in the system ", true);
                                            AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                            //continue;
                                        }
                                    }

                                      
                                    }
                                    catch (Exception ex)
                                    {
                                        errMsgPolicy.Add("Producer1Name", ex.Message);
                                    //errorCount++;
                                    //    Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer1 fields: " + ex.Message);
                                    //    errorList.Add(m);
                                        ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer1  fields  : " + ex.Message, true);
                                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                        //continue;
                                    }
                                //}
                            }
                        }

                        if (dt.Columns.Contains("Producer2Name") && dt.Columns.Contains("Producer2FirstYear") && dt.Columns.Contains("Producer2Renewal"))
                        {
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer2Name"])))
                            {
                                string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer2Name"]);

                                if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                {
                                    try
                                    {
                                        Guid userID = Guid.Empty;
                                        ////Check user on name
                                        var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                        if (objUser == null)
                                        {
                                            errMsgPolicy.Add("Producer2Name", "Producer2 not available in the system");
                                            //errorCount++;
                                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer2Name: Producer2 not available in the system ");
                                            //errorList.Add(m);
                                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer2  not available in the system ", true);
                                            AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                            //continue;
                                        }
                                        else
                                        {
                                            userID = objUser.UserCredentialId;
                                            OutGoingPayment OutgoingRecord = GetOutgoingPaymentForImport(strPrimaryBroker, userID, LicID, Convert.ToString(dt.Rows[i]["Producer2FirstYear"]), Convert.ToString(dt.Rows[i]["Producer2Renewal"]), objPolicy.PolicyId, OutPercentOfPremium);
                                            if (OutgoingRecord != null)
                                            {
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                            else
                                            {
                                                errMsgPolicy.Add("Producer2Name", "Producer2 not available in the system");
                                                //errorCount++;
                                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer2Name: Producer2 not available in the system ");
                                                //errorList.Add(m);
                                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer2  not available in the system ", true);
                                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                                //continue;
                                            }
                                        }
                                        
                                    }
                                    catch (Exception ex)
                                    {
                                        errMsgPolicy.Add("Producer2Name", ex.Message);
                                        //errorCount++;
                                        //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer2Name: " + ex.Message);
                                        //errorList.Add(m);
                                        ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag2: " + ex.Message, true);
                                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                        //continue;
                                    }
                                }
                            }
                        }

                        if (dt.Columns.Contains("Producer3Name") && dt.Columns.Contains("Producer3FirstYear") && dt.Columns.Contains("Producer3Renewal"))
                        {
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer3Name"])))
                            {
                                string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer3Name"]);

                                //int spIndex = strPrimaryBroker.IndexOfAny(spCharac);
                                //strPrimaryBroker = (spIndex > 0) ? strPrimaryBroker.Substring(0, spIndex - 1) : strPrimaryBroker;
                                //ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " strPrimaryBroker pri3: " + strPrimaryBroker, true);

                                if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                {
                                    try
                                    {
                                        Guid userID = Guid.Empty;
                                        ////Check user on name
                                        var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                        if (objUser == null)
                                        {
                                            errMsgPolicy.Add("Producer3Name", "Producer3 not available in the system");
                                            //errorCount++;
                                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer3Name: Producer3 not available in the system ");
                                            //errorList.Add(m);
                                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer3  not available in the system ", true);
                                            AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                            //continue;
                                        }
                                        else
                                        {
                                            userID = objUser.UserCredentialId;
                                            OutGoingPayment OutgoingRecord = GetOutgoingPaymentForImport(strPrimaryBroker, userID, LicID, Convert.ToString(dt.Rows[i]["Producer3FirstYear"]), Convert.ToString(dt.Rows[i]["Producer3Renewal"]), objPolicy.PolicyId, OutPercentOfPremium);
                                            if (OutgoingRecord != null)
                                            {
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                            else
                                            {
                                                errMsgPolicy.Add("Producer3Name", "Producer3 not available in the system");
                                                //errorCount++;
                                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer3Name: Producer3 not available in the system ");
                                                //errorList.Add(m);
                                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer3  not available in the system ", true);
                                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                                //continue;
                                            }
                                        }
                                        
                                    }
                                    catch (Exception ex)
                                    {
                                        errMsgPolicy.Add("Producer3Name", ex.Message);
                                        //errorCount++;
                                        //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer3Name: " + ex.Message);
                                        //errorList.Add(m);
                                        ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag3: " + ex.Message, true);
                                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                        //continue;
                                    }
                                }
                            }
                        }

                        if (dt.Columns.Contains("Producer4Name") && dt.Columns.Contains("Producer4FirstYear") && dt.Columns.Contains("Producer4Renewal"))
                        {
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer4Name"])))
                            {
                                string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer4Name"]);
                                if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                {
                                    try
                                    {
                                        Guid userID = Guid.Empty;
                                        ////Check user on name
                                        var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                        if (objUser == null)
                                        {
                                            errMsgPolicy.Add("Producer4Name", "Producer4 not available in the system");
                                            //errorCount++;
                                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer4Name: Producer4 not available in the system ");
                                            //errorList.Add(m);
                                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer4  not available in the system ", true);
                                            AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                            //continue;
                                        }
                                        else
                                        {
                                            userID = objUser.UserCredentialId;
                                            OutGoingPayment OutgoingRecord = GetOutgoingPaymentForImport(strPrimaryBroker, userID, LicID, Convert.ToString(dt.Rows[i]["Producer4FirstYear"]), Convert.ToString(dt.Rows[i]["Producer4Renewal"]), objPolicy.PolicyId, OutPercentOfPremium);
                                            if (OutgoingRecord != null)
                                            {
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                            else
                                            {
                                                errMsgPolicy.Add("Producer4Name", "Producer4 not available in the system");
                                                //errorCount++;
                                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer4Name: Producer4 not available in the system ");
                                                //errorList.Add(m);
                                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer4  not available in the system ", true);
                                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                                //continue;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errMsgPolicy.Add("Producer4Name", ex.Message);
                                        //errorCount++;
                                        //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer4Name: " + ex.Message);
                                        //errorList.Add(m);
                                        ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag4: " + ex.Message, true);
                                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                        //continue;
                                    }
                                }
                            }
                        }

                        if (dt.Columns.Contains("Producer5Name") && dt.Columns.Contains("Producer5FirstYear") && dt.Columns.Contains("Producer5Renewal"))
                        {
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer5Name"])))
                            {
                                string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer5Name"]);
                              

                                if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                {
                                    try
                                    {
                                        Guid userID = Guid.Empty;
                                        ////Check user on name
                                        var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                        if (objUser == null)
                                        {
                                            errMsgPolicy.Add("Producer5Name", "Producer5 not available in the system");
                                            //errorCount++;
                                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer5Name: Producer5 not available in the system ");
                                            //errorList.Add(m);
                                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer5  not available in the system ", true);
                                            AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                            //continue;
                                        }
                                        else
                                        {
                                            userID = objUser.UserCredentialId;
                                            OutGoingPayment OutgoingRecord = GetOutgoingPaymentForImport(strPrimaryBroker, userID, LicID, Convert.ToString(dt.Rows[i]["Producer5FirstYear"]), Convert.ToString(dt.Rows[i]["Producer5Renewal"]), objPolicy.PolicyId, OutPercentOfPremium);
                                            if (OutgoingRecord != null)
                                            {
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                            else
                                            {
                                                errMsgPolicy.Add("Producer5Name", "Producer5 not available in the system");
                                                //errorCount++;
                                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer5Name: Producer5 not available in the system ");
                                                //errorList.Add(m);
                                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer5  not available in the system ", true);
                                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                                //continue;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errMsgPolicy.Add("Producer5Name", ex.Message);
                                        //errorCount++;
                                        //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer5Name: " + ex.Message);
                                        //errorList.Add(m);
                                        ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag5: " + ex.Message, true);
                                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                        //continue;
                                    }
                                }
                            }
                        }

                        if (dt.Columns.Contains("Producer6Name") && dt.Columns.Contains("Producer6FirstYear") && dt.Columns.Contains("Producer6Renewal"))
                        {
                            if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer6Name"])))
                            {
                                string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer6Name"]);
                                if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                {
                                    try
                                    {
                                        Guid userID = Guid.Empty;
                                        ////Check user on name
                                        var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                        if (objUser == null)
                                        {
                                            errMsgPolicy.Add("Producer6Name", "Producer6 not available in the system");
                                            //errorCount++;
                                            //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer6Name: Producer6 not available in the system ");
                                            //errorList.Add(m);
                                            ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer6  not available in the system ", true);
                                            AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                            //continue;
                                        }
                                        else
                                        {
                                            userID = objUser.UserCredentialId;
                                            OutGoingPayment OutgoingRecord = GetOutgoingPaymentForImport(strPrimaryBroker, userID, LicID, Convert.ToString(dt.Rows[i]["Producer6FirstYear"]), Convert.ToString(dt.Rows[i]["Producer6Renewal"]), objPolicy.PolicyId, OutPercentOfPremium);
                                            if (OutgoingRecord != null)
                                            {
                                                OutGoingField.Add(OutgoingRecord);
                                            }
                                            else
                                            {
                                                errMsgPolicy.Add("Producer6Name", "Producer6 not available in the system");
                                                //errorCount++;
                                                //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer6Name: Producer6 not available in the system ");
                                                //errorList.Add(m);
                                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: Producer6  not available in the system ", true);
                                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                                //continue;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        errMsgPolicy.Add("Producer6Name", ex.Message);
                                        //errorCount++;
                                        //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer6Name: " + ex.Message);
                                        //errorList.Add(m);
                                        ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag6: " + ex.Message, true);
                                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                        //continue;
                                    }
                                }
                            }
                        }
                        
                        #endregion

                    #endregion


                        ActionLogger.Logger.WriteImportLogDetail("Import Policy: outgoing split init done : " + policyIDKey, true);
                    }
                    catch (Exception ex)
                    {
                        errMsgPolicy.Add("Producer1Name", "Producer1 not available in the system");
                        //errorCount++;
                        //Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, "Producer1Name: " + ex.Message);
                        //errorList.Add(m);
                        ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: outgoing fields  : " + ex.Message, true);
                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                        //continue;
                    }

                    #region Track date - checked at last [after origin date found]
                    /* Not to be sent by benefits, poulated from agency's default track date so, no exception throw from here as well*/
                    if (isNewPolicy || objPolicy.TrackFromDate == null)
                    {

                        //if (dt.Columns.Contains("Track_From"))
                        //{
                        //if (dt.Rows[i]["Track_From"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Track_From"])))
                        if (dtTrack != null && dtTrack != DateTime.MinValue)
                        {
                            objPolicy.TrackFromDate = (objPolicy.OriginalEffectiveDate != null && objPolicy.OriginalEffectiveDate > dtTrack) ? objPolicy.OriginalEffectiveDate : dtTrack;
                        }
                        ActionLogger.Logger.WriteImportLogDetail("Import Policy TRack Date : " + objPolicy.TrackFromDate, true);
                    }
                    #endregion

                    #region Save Data

                    //Check if any error, then return 
                    if(errMsgPolicy.Count > 0)
                    {
                        string output = Newtonsoft.Json.JsonConvert.SerializeObject(errMsgPolicy);
                        errorCount++;
                        Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, output);
                        errorList.Add(m);
                        continue;
                    }

                    try
                    { 
                        //Save only when policy ID not present in error list
                        if (errorList == null || (errorList != null && errorList.Count == 0) || (errorList != null && errorList.Count > 0 && errorList.Where(x => x.OriginalPlanID == importedPolicyID).ToList().Count == 0))
                        {
                            #region Client
                            try
                            {

                                if (isNewPolicy || objPolicy.PolicyClientId == null)
                                {
                                    string client = Convert.ToString(dt.Rows[i]["AccountName"]);
                                    Client objClient = (new Client()).GetClientByClientName(client, LicID);
                                    //Get Client ID by Get Client name
                                    if (objClient == null)
                                    {
                                        //Create new client
                                        objClient = new Client();
                                        objClient.ClientId = Guid.NewGuid();
                                        string strClientValue = string.Empty;

                                        strClientValue = (client.Length > 49) ? client.Substring(0, 49) : client;

                                        //objClnt.Name = policy.ClientName;
                                        objClient.Name = strClientValue;
                                        objClient.LicenseeId = LicID;
                                        objClient.IsDeleted = false;
                                        Client.AddUpdateClient(client, LicID, objClient.ClientId);
                                        ActionLogger.Logger.WriteImportLogDetail("Import Policy: client saved as new : " + policyIDKey, true);
                                    }
                                    objPolicy.ClientReference.Value = (from p in DataModel.Clients where p.ClientId == objClient.ClientId select p).FirstOrDefault();
                                }
                                //following requires client reference, so added at last
                                if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Insured))
                                {
                                    if (dt.Columns.Contains("AccountName"))
                                    {
                                        objPolicy.Insured = Convert.ToString(dt.Rows[i]["AccountName"]);
                                    }
                                    else
                                    {
                                        objPolicy.Insured = objPolicy.Client.Name;
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                errorCount++; //Allow this code here , as thisis the last error possibility
                                errMsgPolicy.Add("AccountName", ex.Message);
                                string output = Newtonsoft.Json.JsonConvert.SerializeObject(errMsgPolicy);
                                Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, output);
                                errorList.Add(m);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: client : " + ex.Message, true);
                                AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                                continue;
                            }

                           
                            #endregion
                            //try
                            //{
                            if (isNewPolicy)
                            {
                                DataModel.AddToPolicies(objPolicy);
                                DataModel.SaveChanges();
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy : policy saved successfully", true);
                                PolicyToLearnPost.AddLearnedAfterImport(objPolicy.PolicyId, "", covNickName, strProductType, importedPolicyID);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy : learned fields saved successfully", true);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Policy saved as new : " + policyIDKey, true);

                            }
                            else
                            {
                                DataModel.SaveChanges();
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy : policy updated successfully", true);
                                AddUpdatePolicyHistory(objPolicy.PolicyId);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy : policy history updated successfully", true);
                                //Learned fields to update , once policy fields are updated 
                                PolicyToLearnPost.AddLearnedAfterImport(objPolicy.PolicyId, "", covNickName, strProductType, importedPolicyID);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy : learned fields saved successfully", true);
                                PolicyLearnedField.AddUpdateHistoryLearned(objPolicy.PolicyId);
                                ActionLogger.Logger.WriteImportLogDetail("Import Policy: Policy updated : " + policyIDKey, true);
                            }


                            //Save Incoming and delete old 
                            if (inSchedule != null)
                            {
                                if (tempSched != null && tempSched.FirstYearPercentage == 0 && tempSched.RenewalPercentage == 0)
                                {
                                    PolicyToolIncommingShedule.DeleteSchedule(objPolicy.PolicyId); //If schedule occurs with 0 split, then delete before adding new
                                    ActionLogger.Logger.WriteImportLogDetail("Import Policy old schedule is deleted", true);
                                }
                                inSchedule.AddUpdate();
                                ActionLogger.Logger.WriteImportLogDetail("Incoming schedule added for the policy", true);
                            }

                            //Save Outgoing
                            //Delete old schdule- outgoing  payments witj old schedule will remian intact, new will take effect on new entries only.
                            //Check only if new outgoing schedule exists, then overwrite the old
                            //This is to exclude the case when policy is existing and excel has blank entries. Without this, existing schedule will be overwritten
                            // by 100% to house for existing policy 
                            if (isNewPolicy || OutGoingField.Count > 0)
                            {
                                ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule adding to the policy", true);
                                OutGoingField = CompleteOutgoingSchedule(OutGoingField, objPolicy.PolicyId, LicID);
                                OutGoingPayment.DeletePolicyOutGoingSchedulebyPolicyId(objPolicy.PolicyId);
                                ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule removed from the policy", true);
                                OutGoingPayment.AddUpdate(OutGoingField);
                                ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule added for the policy", true);
                            }

                            if (isNewPolicy) addCount++;
                            else updateCount++;

                            //Adding status in DB
                            AddImportStatusToDB(importedPolicyID, isNewPolicy, true, benefits_policyID);
                        }
                        
                    #endregion

                    }
                    catch (Exception ex)
                    {
                        ActionLogger.Logger.WriteImportLogDetail("Exception adding policy: " + ex.Message, true);
                        errorCount++;
                        errMsgPolicy.Add("SaveError", ex.Message);
                        string output = Newtonsoft.Json.JsonConvert.SerializeObject(errMsgPolicy);
                        Benefits_ErrorMsg m = new Benefits_ErrorMsg(importedPolicyID, benefits_policyID, output);
                        errorList.Add(m);
                        AddImportStatusToDB(importedPolicyID, isNewPolicy, false, benefits_policyID);
                    }
                }

                objStatus.NewCount = addCount;
                objStatus.UpdateCount = updateCount;
                objStatus.ErrorCount = errorCount;
                objStatus.ErrorList = errorList;
                return objStatus;
            }
        }

        static Guid AddNewUserFromBenefits(string name, Guid licenseeID, bool isExec=false)
        {
            ActionLogger.Logger.WriteImportLogDetail("AddNewUserFromBenefits request: name - " + name, true);
            try
            {
                User u = new User();
                u.UserCredentialID = Guid.NewGuid();
                u.NickName = name;
                u.LicenseeId = licenseeID;//for benefits only 
                string[] arrNames = name.Trim().Split(' ');
                if (arrNames != null && arrNames.Length > 0)
                {
                    u.FirstName = arrNames[0];
                    if (arrNames.Length > 1)
                        u.LastName = arrNames[1];
                }
                u.Company = "Benefit Pro Insurance Services Inc";
                u.Role = UserRole.Agent;
                u.IsAccountExec = isExec;
                u.AddUpdateUser();
                ActionLogger.Logger.WriteImportLogDetail("AddNewUserFromBenefits : user added successfully, name - " + name, true);

                //When account Exec is to be added , but account exec check fails, then return empty guid indicating failure.
                if (isExec  &&  !u.CheckAccoutExec(u.UserCredentialID))
                {
                    return Guid.Empty;
                }

                u.LinkNewUserToHouse();
                //On success, return new ID
                return u.UserCredentialID;
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail("AddNewUserFromBenefits exception - " + ex.Message, true);
                return Guid.Empty;
            }
        }


        static void AddImportStatusToDB(string policyID, bool isNew, bool isSuccess, string benefitsID)
        {
             try
            {
                ActionLogger.Logger.WriteImportLogDetail("AddImportStatusToDB request: policyID -  " + policyID + ", isNew - " + isNew + ", isSuccess - " + isSuccess, true);
                DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
                EntityConnection ec = (EntityConnection)ctx.Connection;
                SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
                string adoConnStr = sc.ConnectionString;

                using (SqlConnection con = new SqlConnection(adoConnStr))
                {
                    using (SqlCommand cmd = new SqlCommand("Usp_AddImportStatus", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UniquePolicyID", policyID);
                        cmd.Parameters.AddWithValue("@IsNew", isNew);
                        cmd.Parameters.AddWithValue("@IsSuccess", isSuccess);
                        cmd.Parameters.AddWithValue("@BenefitsPolicyID", benefitsID);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
             catch (Exception ex)
             {
                 ActionLogger.Logger.WriteImportLogDetail("Exception AddImportStatusToDB :  PolicyID -  " + policyID + ", ex: "  + ex.Message, true);
             }
        }

        public static PolicyImportStatus ImportPolicy(DataTable dt, ObservableCollection<User> GlobalAgentList, Guid LicID, ObservableCollection<CompType> CompTypeList)
        {
            #region Variables
            PolicyImportStatus objStatus = new PolicyImportStatus();
            string policyIDKey = System.Configuration.ConfigurationSettings.AppSettings["PolicyIDKeyName"];
            char[] spCharac = System.Configuration.ConfigurationSettings.AppSettings["AgentCharactersToTrim"].ToCharArray();  //{ '(', '[', '-' };
                         
            ActionLogger.Logger.WriteImportLogDetail("Import Policy: policyIDKey: " + policyIDKey, true);
            int addCount = 0;
            int updateCount = 0;
            int errorCount = 0;
            List<string> errorList = new List<string>();

            
            List<OutGoingPayment> OutGoingField = new List<OutGoingPayment>();
            PolicyToolIncommingShedule inSchedule = null;
            string strProductType = string.Empty;
            string covNickName = string.Empty;
            Guid houseOwner = Guid.Empty;
            
            #endregion
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
               ActionLogger.Logger.WriteImportLogDetail("Import Policy: Data model init", true);
               var AgentList = (from p in DataModel.UserCredentials
                                                        join o in DataModel.UserDetails on p.UserCredentialId equals o.UserCredentialId
                                                        where p.LicenseeId == LicID /*&& p.RoleId == 3*/ && p.IsDeleted == false
                                                        select new
                                                        {
                                                            p.UserCredentialId,
                                                            o.NickName,
                                                            p.UserName,
                                                            p.RoleId,
                                                            o.FirstName,
                                                            o.LastName
                                                        }).ToList();
               ActionLogger.Logger.WriteImportLogDetail("Import Policy: Agent list fetched", true);
               for (int i = 0; i < dt.Rows.Count; i++)
               {
                    DLinq.Policy objPolicy = new DLinq.Policy();
                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: Iteration: " + i, true);
                    string importedPolicyID = dt.Columns.Contains(policyIDKey) ? Convert.ToString(dt.Rows[i][policyIDKey]).Trim() : (dt.Columns.Contains("Life Insurance Plan ID") ? Convert.ToString(dt.Rows[i]["Life Insurance Plan ID"]).Trim() : (dt.Columns.Contains("Policy Plan ID") ? Convert.ToString(dt.Rows[i]["Policy Plan ID"]).Trim() : ""));
                    //string importedPolicyID = dt.Columns.Contains(policyIDKey) ? Convert.ToString(dt.Rows[i][policyIDKey]).Trim() : (dt.Columns.Contains("Life Insurance Plan ID") ? Convert.ToString(dt.Rows[i]["Life Insurance Plan ID"]).Trim() : "");
                    ActionLogger.Logger.WriteImportLogDetail("Import Policy: importedPolicyID: " + importedPolicyID, true);
                    OutGoingField = new List<OutGoingPayment>(); 
                   try
                   {
                       bool isNewPolicy = false;
                       Guid policyID = IsPolicyExistingWithImportID(importedPolicyID, LicID);
                       ActionLogger.Logger.WriteImportLogDetail("Import Policy: policyID: " + policyID , true);
                       if (policyID != Guid.Empty)
                       {
                           objPolicy = (from p in DataModel.Policies where p.PolicyId == policyID select p).FirstOrDefault();
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy: Existing policy", true);
                           
                           
                           //check if client exists or deleted in the system
                           //if deleted, then add the given as new 
                           if (objPolicy.Client != null && Convert.ToBoolean(objPolicy.Client.IsDeleted))
                           {
                               objPolicy.PolicyClientId = null;
                               ActionLogger.Logger.WriteImportLogDetail("Import Policy: Client found deleted , so setting as null ", true);
                           }
                       }
                       else
                       {
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy: New policy", true);
                           isNewPolicy = true;
                           objPolicy.PolicyId = Guid.NewGuid();
                           objPolicy.IsTrackPayment = true;
                           objPolicy.PolicyLicenseeId = LicID;
                           objPolicy.TerminationReasonId = null;
                           objPolicy.IsTrackMissingMonth = true;
                           objPolicy.CreatedOn = DateTime.Today;
                           objPolicy.IsIncomingBasicSchedule = true;
                           objPolicy.IsOutGoingBasicSchedule = true;
                           objPolicy.CreatedBy = new Guid("AA38DF84-2E30-43CA-AED3-7276224D1B7E");
                           objPolicy.IsDeleted = false;
                       }
                     
                       #region Fields that should be updated when blank or woth new policy
                       try
                       {

                           if (isNewPolicy || string.IsNullOrEmpty(objPolicy.PolicyType))
                           {
                               if (dt.Columns.Contains("New?") || dt.Columns.Contains("New Business?"))
                               {
                                   string strTypeOfPolicy = dt.Columns.Contains("New?") ? Convert.ToString(dt.Rows[i]["New?"]) : (dt.Columns.Contains("New Business?") ? Convert.ToString(dt.Rows[i]["New Business?"]) : "");
                                   objPolicy.PolicyType = (!string.IsNullOrEmpty(strTypeOfPolicy) && (strTypeOfPolicy.ToLower() == "rewrite" || strTypeOfPolicy.ToLower() == "replace")) ? "Replace" : "New";
                               }
                               else if(isNewPolicy)
                               {
                                   objPolicy.PolicyType = "New";
                               }
                           }
                           //if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Insured))
                           //{
                           //    if (dt.Columns.Contains("insured?"))
                           //    {
                           //        objPolicy.Insured = Convert.ToString(dt.Rows[i]["insured"]);
                           //    }
                           //    else
                           //    {
                           //        objPolicy.Insured = objPolicy.Client.Name;
                           //    }
                           //}
                           if (isNewPolicy || string.IsNullOrEmpty(objPolicy.PolicyNumber))
                           {
                               if (dt.Columns.Contains("Group #"))
                               {
                                   objPolicy.PolicyNumber = Convert.ToString(dt.Rows[i]["Group #"]);
                               }
                           }
                           if (isNewPolicy || string.IsNullOrEmpty(objPolicy.SubmittedThrough))
                           {
                               if (dt.Columns.Contains("Submitted Through"))
                               {
                                   objPolicy.SubmittedThrough = Convert.ToString(dt.Rows[i]["Submitted Through"]);
                               }
                           }
                           if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Enrolled))
                           {
                               if (dt.Columns.Contains("Number of Covered Lives"))
                               {
                                   objPolicy.Enrolled = Convert.ToString(dt.Rows[i]["Number of Covered Lives"]);
                               }
                           }
                           if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Eligible))
                           {
                               if (dt.Columns.Contains("eligible"))
                               {
                                   objPolicy.Eligible = Convert.ToString(dt.Rows[i]["eligible"]);
                               }
                           }
                           if (isNewPolicy || (objPolicy.Advance == null))
                           {
                               if (dt.Columns.Contains("Advanced Payment Number"))
                               {
                                   objPolicy.Advance = (!string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Advanced Payment Number"]))) ? Convert.ToInt32(Convert.ToString(dt.Rows[i]["Advanced Payment Number"])) : 0;
                               }
                           }
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy: optional fields , to be filled when blanks init done : " + policyIDKey, true);
                       }
                       catch (Exception ex)
                       {
                           errorCount++;
                           errorList.Add(importedPolicyID);
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy Exception: optional fields exception : " + ex.Message, true);
                           continue;
                       }
                       //Incoming Schedule 
                       #region Incoming Schedule
                       bool oldInScheduleExists = false;
                       DLinq.PolicyIncomingSchedule tempSched = null;
                       //Check if incoming is already set with 0 splits
                       if (objPolicy.IsIncomingBasicSchedule == true)
                       {
                           tempSched = objPolicy.PolicyIncomingSchedules.FirstOrDefault();
                           if (tempSched != null)
                           {
                               if (tempSched.FirstYearPercentage > 0 || tempSched.RenewalPercentage > 0)
                               {
                                   oldInScheduleExists = true;
                               }
                               //else if (tempSched.FirstYearPercentage == 0 && tempSched.RenewalPercentage == 0)
                               //{
                               //    PolicyToolIncommingShedule.DeleteSchedule(objPolicy.PolicyId); //If schedule occurs with 0 split, then delete before adding new
                               //}
                           }
                       }
                     
                       if (isNewPolicy || (objPolicy.IsIncomingBasicSchedule != true) || (!oldInScheduleExists))
                       {
                           if (dt.Columns.Contains("Commission Type") && dt.Columns.Contains("Payment Type") && dt.Columns.Contains("Commissions - First Year %") && dt.Columns.Contains("Commissions - Renewal %") && dt.Columns.Contains("Co Broker Split"))
                           {
                               if (tempSched != null && tempSched.FirstYearPercentage == 0 && tempSched.RenewalPercentage == 0)
                               {
                                   PolicyToolIncommingShedule.DeleteSchedule(objPolicy.PolicyId); //If schedule occurs with 0 split, then delete before adding new
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy old schedule is deleted", true);
                               }


                               string strCommisionType = Convert.ToString(dt.Rows[i]["Commission Type"]);
                               objPolicy.IncomingPaymentTypeId = PolicCompType(strCommisionType, CompTypeList);
                               string strOutPercentOfPremium = Convert.ToString(dt.Rows[i]["Payment Type"]);
                               string strInFirstYear = Convert.ToString(dt.Rows[i]["Commissions - First Year %"]);
                               string strInRenewYear = Convert.ToString(dt.Rows[i]["Commissions - Renewal %"]);
                               string strSplit = Convert.ToString(dt.Rows[i]["Co Broker Split"]);
                               double splitPer = 0;
                               double.TryParse(strSplit, out splitPer);
                               objPolicy.SplitPercentage = splitPer;

                               inSchedule = new PolicyToolIncommingShedule();
                               inSchedule.PolicyId = objPolicy.PolicyId;
                               inSchedule.IncomingScheduleId = Guid.NewGuid();
                               inSchedule.ScheduleTypeId = (!string.IsNullOrEmpty(strOutPercentOfPremium) && strOutPercentOfPremium.ToLower().Contains("head")) ? 2 : 1;
                               double frst = 0; double renew = 0;
                               double.TryParse(strInFirstYear, out frst);
                               double.TryParse(strInRenewYear, out renew);
                               inSchedule.FirstYearPercentage = frst; //(!string.IsNullOrEmpty(strInFirstYear)) ? Convert.ToDouble(strInFirstYear) : 0;
                               inSchedule.RenewalPercentage = renew;// (!string.IsNullOrEmpty(strInRenewYear)) ? Convert.ToDouble(strInRenewYear) : 0;
                               ActionLogger.Logger.WriteImportLogDetail("Import Policy: Incoming schedule init done : " + policyIDKey, true);
                           }
                           else
                           {
                               ActionLogger.Logger.WriteImportLogDetail("Import Policy: Incoming schedule not found in excel", true);
                           }
                       }
                       #endregion


                       try
                       {
                           //Track date
                           if (isNewPolicy || objPolicy.TrackFromDate == null)
                           {
                               if (dt.Columns.Contains("Track From"))
                               {
                                   if (dt.Rows[i]["Track From"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Track From"])))
                                   {
                                       string trackDate = Convert.ToString(dt.Rows[i]["Track From"]);
                                       ActionLogger.Logger.WriteImportLogDetail("Import Policy trackDate: " + trackDate, true);
                                       //DateTime dtTrac = DateTime.MinValue;
                                       //DateTime.TryParse(trackDate, out dtTrac);
                                       //objPolicy.TrackFromDate = dtTrac;// Convert.ToDateTime(trackDate);
                                       
                                       double dblEff = 0;
                                       Double.TryParse(trackDate, out dblEff);
                                       if (dblEff > 0)
                                       {
                                           objPolicy.TrackFromDate = DateTime.FromOADate(dblEff);
                                       }
                                       else
                                       {
                                           objPolicy.TrackFromDate = Convert.ToDateTime(trackDate);
                                       }
                                   }
                               }
                           }
                           //Mode
                           if (isNewPolicy || objPolicy.PolicyModeId == null)
                           {
                               if (dt.Columns.Contains("Modal Number"))
                               {
                                   string strMode = Convert.ToString(dt.Rows[i]["Modal Number"]);
                                   objPolicy.PolicyModeId = (!string.IsNullOrEmpty(strMode)) ? PolicyModeID(strMode) : PolicyModeID("0");
                               }
                           }

                           //premium
                           if (isNewPolicy || objPolicy.MonthlyPremium == null)
                           {
                               if (dt.Columns.Contains("Modal Premium"))
                               {
                                   string strPremiuum = Convert.ToString(dt.Rows[i]["Modal Premium"]);
                                   decimal prem = 0;
                                   decimal.TryParse(strPremiuum, out prem);
                                   if (prem == 0)
                                   {
                                       decimal.TryParse(strPremiuum, NumberStyles.Currency, CultureInfo.GetCultureInfo("en-US"), out prem);
                                   }
                                   objPolicy.MonthlyPremium = prem;// (!string.IsNullOrEmpty(strPremiuum)) ? Convert.ToDecimal(strPremiuum) : 0;
                               }
                           }
                       }
                       catch (Exception ex)
                       {
                           errorCount++;
                           errorList.Add(importedPolicyID);
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy execption :premium/mode/trackDate : " + ex.Message, true);
                           continue;
                       }
                       //Payor  
                       #region Payor
                       try
                       {
                           if (isNewPolicy || objPolicy.PayorId == null)
                           {
                               if (dt.Columns.Contains("Payor Commission Dept"))
                               {
                                   string strPayor = Convert.ToString(dt.Rows[i]["Payor Commission Dept"]);
                                   if (!string.IsNullOrEmpty(strPayor))
                                   {
                                       DLinq.Payor py = (from p in DataModel.Payors where (p.PayorName.ToLower() == strPayor.ToLower() || (p.PayorName.ToLower() != strPayor.ToLower() && p.NickName.ToLower() == strPayor.ToLower())) select p).FirstOrDefault();
                                       //if (py == null)
                                       {
                                          // py = (from p in DataModel.Payors where p.NickName.ToLower() == strPayor.ToLower() select p).FirstOrDefault();
                                           if (py != null)
                                           {
                                               objPolicy.PayorId = py.PayorId;
                                               objPolicy.PayorReference.Value = py;
                                           }
                                       }
                                   }
                               }
                           }
                       #endregion

                           #region Carrier
                           if (isNewPolicy || objPolicy.CarrierId == null)
                           {
                               if (dt.Columns.Contains("Carrier Commission Dept"))
                               {
                                   string strCarr = Convert.ToString(dt.Rows[i]["Carrier Commission Dept"]);
                                   if (!string.IsNullOrEmpty(strCarr))
                                   {
                                       DLinq.Carrier cr = (from p in DataModel.Carriers where p.CarrierName.ToLower() == strCarr.ToLower() select p).FirstOrDefault();
                                       if (cr == null)
                                       {
                                           DLinq.CarrierNickName crN = (from p in DataModel.CarrierNickNames where (p.PayorId == objPolicy.PayorId || (p.PayorId != objPolicy.PayorId && p.NickName == strCarr)) select p).FirstOrDefault();
                                          // if (crN == null)
                                           //{
                                           //    crN = (from p in DataModel.CarrierNickNames where p.NickName == strCarr select p).FirstOrDefault();
                                           //}
                                           if (crN != null)
                                           {
                                               objPolicy.CarrierId = crN.CarrierId;
                                               objPolicy.CarrierReference.Value = crN.CarrierReference.Value;
                                           }
                                       }
                                       else
                                       {
                                           objPolicy.CarrierId = cr.CarrierId;
                                           objPolicy.CarrierReference.Value = cr;
                                       }

                                   }
                               }
                           }
                           #endregion
                       }
                       catch (Exception ex)
                       {
                           errorCount++;
                           errorList.Add(importedPolicyID);
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: payor/carrier : " + ex.Message, true);
                           continue;
                       }

                       try
                       {
                           #region Product
                           if (isNewPolicy || objPolicy.CoverageId == null)
                           {
                               if (dt.Columns.Contains("Line Of Coverage"))
                               {
                                   string strProduct = Convert.ToString(dt.Rows[i]["Line Of Coverage"]);
                                   DLinq.Coverage cov = (from p in DataModel.Coverages where p.ProductName == strProduct select p).FirstOrDefault();
                                   if (cov != null)
                                   {
                                       objPolicy.CoverageId = cov.CoverageId;
                                       objPolicy.CoverageReference.Value = cov;
                                   }
                               }
                           }
                           #endregion

                           #region Product Type
                           if (isNewPolicy || string.IsNullOrEmpty(objPolicy.ProductType))
                           {
                               if (dt.Columns.Contains("Product Type"))
                               {
                                   strProductType = Convert.ToString(dt.Rows[i]["Product Type"]);
                                   DLinq.CoverageNickName covName = (from p in DataModel.CoverageNickNames where p.NickName == strProductType select p).FirstOrDefault();
                                   covNickName = (covName != null) ? covName.NickName : string.Empty; //to be used later in code, so kept separate
                                   objPolicy.ProductType = (!string.IsNullOrEmpty(covNickName)) ? covNickName : strProductType;
                               }
                           }
                           #endregion
                       }
                       catch (Exception ex)
                       {
                           errorCount++;
                           errorList.Add(importedPolicyID);
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: product : " + ex.Message, true);
                           continue;
                       }

                    
                       #endregion

                       #region Common fields - always update with insert/Update
                       bool OutPercentOfPremium = false;
                       try
                       {
                           //Status
                           if (dt.Columns.Contains("Plan Status Description") && dt.Rows[i]["Plan Status Description"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Plan Status Description"])))
                           {
                               string strStatus = dt.Rows[i]["Plan Status Description"].ToString();
                               objPolicy.PolicyStatusId = (strStatus.ToLower() == "active") ? 0 : (strStatus.ToLower() == "pending") ? 2 : 1;
                           }
                           else if (dt.Columns.Contains("Current Plan Status") && dt.Rows[i]["Current Plan Status"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Current Plan Status"])))
                           {
                               string strStatus = dt.Rows[i]["Current Plan Status"].ToString();
                               objPolicy.PolicyStatusId = (strStatus.ToLower() == "active") ? 0 : (strStatus.ToLower() == "pending") ? 2 : 1;
                           }
                           
                           //Original effective date
                           if (dt.Columns.Contains("Original Plan Start Date"))
                           {
                               if (dt.Rows[i]["Original Plan Start Date"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Original Plan Start Date"])))
                               {
                                   string effDate = Convert.ToString(dt.Rows[i]["Original Plan Start Date"]);
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy string effDate: " + effDate, true);
                                   //check if value in double, then fetch OA Date
                                   double dblEff = 0;
                                   Double.TryParse(effDate, out dblEff);
                                   if (dblEff > 0)
                                   {
                                       objPolicy.OriginalEffectiveDate = DateTime.FromOADate(dblEff);
                                   }
                                   else
                                   {
                                       objPolicy.OriginalEffectiveDate = DateTime.Parse(effDate, System.Globalization.CultureInfo.CurrentCulture); //Convert.ToDateTime(effDate);
                                   }
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy datetime effDate: " + objPolicy.OriginalEffectiveDate, true);
                               }
                           }
                           //Account Exec
                           if (dt.Columns.Contains("Account Owner: Full Name"))
                           {
                               if (dt.Rows[i]["Account Owner: Full Name"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Account Owner: Full Name"])))
                               {
                                   string acctExec = Convert.ToString(dt.Rows[i]["Account Owner: Full Name"]);
                                   var objUser = AgentList.Where(d => (d.FirstName + " " + d.LastName).ToLower() == acctExec.ToLower() || (!string.IsNullOrEmpty(d.NickName) && d.NickName.ToLower() == acctExec.ToLower())).FirstOrDefault(); //User.GetUserIdWise(tempGuid);// (from p in DataModel.UserCredentials where (p.UserCredentialId == tempGuid && p.RoleId == 3 && p.IsDeleted == false) select p).FirstOrDefault();
                                  
                                   Guid tempGuid = Guid.Empty;
                                   Guid.TryParse(acctExec, out tempGuid);
                                   if (tempGuid != Guid.Empty)
                                   {
                                       objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                   }
                                   //Guid tempGuid = new Guid(acctExec);
                                   // User objUser = GlobalAgentList.Where(d => d.UserCredentialID == tempGuid).FirstOrDefault();
                                  if (objUser != null /*&& objUser.Role == UserRole.Agent*/)
                                   {
                                       //Need to get nick name
                                       if (string.IsNullOrEmpty(objUser.NickName))
                                       {
                                           objPolicy.AccoutExec = objUser.NickName;
                                           objPolicy.UserCredentialId = objUser.UserCredentialId; //tempGuid;
                                       }
                                       else
                                       {
                                           objPolicy.AccoutExec = objUser.UserName;
                                           objPolicy.UserCredentialId = objUser.UserCredentialId; //tempGuid;
                                       }
                                       bool isexec = (new User().CheckAccoutExec(objUser.UserCredentialId)); //Sets the flag of accExec true for this userID 
                                   }
                               }
                           }
                           //Term Date
                           if (objPolicy.PolicyStatusId == 1)
                           {
                               ActionLogger.Logger.WriteImportLogDetail("Import Policy: status is terminated, so updating plan end date and reason ", true);
                               if (dt.Columns.Contains("Plan End Date"))
                               {
                                   if (dt.Rows[i]["Plan End Date"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Plan End Date"])))
                                   {
                                       string termDate = Convert.ToString(dt.Rows[i]["Plan End Date"]);
                                       ActionLogger.Logger.WriteImportLogDetail("Import Policy string termDate: " + termDate, true);
                                       //check if value in double, then fetch OA Date
                                       double dblTerm = 0;
                                       Double.TryParse(termDate, out dblTerm);
                                       if (dblTerm > 0)
                                       {
                                           objPolicy.PolicyTerminationDate = DateTime.FromOADate(dblTerm);
                                       }
                                       else
                                       {
                                           objPolicy.PolicyTerminationDate = Convert.ToDateTime(termDate);
                                       }
                                     //  objPolicy.PolicyTerminationDate = Convert.ToDateTime(termDate);
                                   }
                               }
                               //Term reason
                               if (dt.Columns.Contains("Termination Reason"))
                               {
                                   if (dt.Rows[i]["Termination Reason"] != null && !String.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Termination Reason"])))
                                   {
                                       objPolicy.TerminationReasonId = PolicTermisionID(Convert.ToString(dt.Rows[i]["Termination Reason"]));
                                   }
                               }
                           }
                           else
                           {
                               ActionLogger.Logger.WriteImportLogDetail("Import Policy: status is NOT terminated, so ignoring plan end date and reason ", true);
                           }
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy: mandatory fields init done : " + policyIDKey, true);
                       }
                       catch (Exception ex)
                       {
                           errorCount++;
                           errorList.Add(importedPolicyID);
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: mandatory fields  : " + ex.Message, true);
                           continue;
                       }
                       //Outgoing split 
                       try
                       {
                           #region Outgoing Split
                            if (dt.Columns.Contains("Producer 1: Full Name") && dt.Columns.Contains("Producer 1 First Year %") && dt.Columns.Contains("Producer 1 Renewal %"))
                           {
                               if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 1: Full Name"])))
                               {
                                   string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 1: Full Name"]);
                                   int spIndex =  strPrimaryBroker.IndexOfAny(spCharac);
                                   strPrimaryBroker = (spIndex > 0) ? strPrimaryBroker.Substring(0, spIndex - 1) : strPrimaryBroker;
                                   ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " strPrimaryBroker pri1: " + strPrimaryBroker, true);

                                   if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                   {
                                       try
                                       {
                                            //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            //  Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            ///var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.RoleId==3 && (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower()))).FirstOrDefault();
                                            //var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                           Guid tempGuid = Guid.Empty;
                                           Guid.TryParse(strPrimaryBroker, out tempGuid);
                                           if (tempGuid != Guid.Empty)
                                           {
                                               objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                           }

                                           if (objUser != null)
                                           {
                                               string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 1 First Year %"]);
                                               string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 1 Renewal %"]);
                                               OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                               OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                               OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                            //   OutgoingRecord.IsPrimaryAgent = true;

                                               double frst = 0; double renew = 0;
                                               double.TryParse(strFirstPer, out frst);
                                               double.TryParse(strRenewalPer, out renew);

                                               OutgoingRecord.FirstYearPercentage = frst;// string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                               OutgoingRecord.RenewalPercentage = renew;// string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                               //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                               OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                               OutgoingRecord.ScheduleTypeId = OutPercentOfPremium ? 1 : 2;
                                               OutGoingField.Add(OutgoingRecord);
                                           }
                                       }
                                       catch (Exception ex)
                                       {
                                           ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for pri1: " + ex.Message, true);
                                           continue;
                                       }
                                   }
                               }
                           }

                           if (dt.Columns.Contains("Producer 2: Full Name") && dt.Columns.Contains("Producer 2 First Year %") && dt.Columns.Contains("Producer 2 Renewal %"))
                           {
                               if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 2: Full Name"])))
                               {
                                   string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 2: Full Name"]);

                                   int spIndex = strPrimaryBroker.IndexOfAny(spCharac);
                                   strPrimaryBroker = (spIndex > 0) ? strPrimaryBroker.Substring(0, spIndex - 1) : strPrimaryBroker;
                                   ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " strPrimaryBroker pri2: " + strPrimaryBroker, true);

                                   if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                   {
                                       try
                                       {
                                            //Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            //var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            // var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.RoleId == 3 && (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower()))).FirstOrDefault();
                                            Guid tempGuid = Guid.Empty;
                                           Guid.TryParse(strPrimaryBroker, out tempGuid);
                                           if (tempGuid != Guid.Empty)
                                           {
                                               objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                           }
                                           if (objUser != null)
                                           {
                                               string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 2 First Year %"]);
                                               string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 2 Renewal %"]);
                                               OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                               OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                               OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                               OutgoingRecord.IsPrimaryAgent = false;

                                               double frst = 0; double renew = 0;
                                               double.TryParse(strFirstPer, out frst);
                                               double.TryParse(strRenewalPer, out renew);

                                               OutgoingRecord.FirstYearPercentage = frst;// string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                               OutgoingRecord.RenewalPercentage = renew;

                                               //OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                               //OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strRenewalPer) ? 0 : Convert.ToDouble(strRenewalPer);
                                               //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                               OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                               OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                               OutGoingField.Add(OutgoingRecord);
                                           }
                                       }
                                       catch (Exception ex)
                                       {
                                           ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag1: " + ex.Message, true);
                                           continue;
                                       }
                                   }
                               }
                           }

                           if (dt.Columns.Contains("Producer 3: Full Name") && dt.Columns.Contains("Producer 3 First Year %") && dt.Columns.Contains("Producer 3 Renewal %"))
                           {
                               if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 3: Full Name"])))
                               {
                                   string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 3: Full Name"]);

                                   int spIndex = strPrimaryBroker.IndexOfAny(spCharac);
                                   strPrimaryBroker = (spIndex > 0) ? strPrimaryBroker.Substring(0, spIndex - 1) : strPrimaryBroker;
                                   ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " strPrimaryBroker pri3: " + strPrimaryBroker, true);

                                   if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                   {
                                       try
                                       {
                                            //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            //   Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            // var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            //var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.RoleId == 3 && (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower()))).FirstOrDefault();

                                           Guid tempGuid = Guid.Empty;
                                           Guid.TryParse(strPrimaryBroker, out tempGuid);
                                           if (tempGuid != Guid.Empty)
                                           {
                                               objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                           }
                                           if (objUser != null)
                                           {
                                               string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 3 First Year %"]);
                                               string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 3 Renewal %"]);
                                               OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                               OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                               OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                               OutgoingRecord.IsPrimaryAgent = false;
                                              
                                               double frst = 0; double renew = 0;
                                               double.TryParse(strFirstPer, out frst);
                                               double.TryParse(strRenewalPer, out renew);

                                               OutgoingRecord.FirstYearPercentage = frst;// string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                               OutgoingRecord.RenewalPercentage = renew;
                                               //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                               OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                               OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                               OutGoingField.Add(OutgoingRecord);
                                           }
                                       }
                                       catch (Exception ex)
                                       {
                                           ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag2: " + ex.Message, true);
                                           continue;
                                       }
                                   }
                               }
                           }

                           if (dt.Columns.Contains("Producer 4: Full Name") && dt.Columns.Contains("Producer 4 First Year %") && dt.Columns.Contains("Producer 4 Renewal %"))
                           {
                               if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 4: Full Name"])))
                               {
                                   string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 4: Full Name"]);

                                   int spIndex = strPrimaryBroker.IndexOfAny(spCharac);
                                   strPrimaryBroker = (spIndex > 0) ? strPrimaryBroker.Substring(0, spIndex - 1) : strPrimaryBroker;
                                   ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " strPrimaryBroker pri4: " + strPrimaryBroker, true);

                                   if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                   {
                                       try
                                       {
                                            //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            //Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            //var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            // var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.RoleId == 3 && (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower()))).FirstOrDefault();

                                            Guid tempGuid = Guid.Empty;
                                           Guid.TryParse(strPrimaryBroker, out tempGuid);
                                           if (tempGuid != Guid.Empty)
                                           {
                                               objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                           }
                                           if (objUser != null)
                                           {
                                               string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 4 First Year %"]);
                                               string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 4 Renewal %"]);
                                               OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                               OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                               OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                               OutgoingRecord.IsPrimaryAgent = false;
                                               double frst = 0; double renew = 0;
                                               double.TryParse(strFirstPer, out frst);
                                               double.TryParse(strRenewalPer, out renew);

                                               OutgoingRecord.FirstYearPercentage = frst;// string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                               OutgoingRecord.RenewalPercentage = renew;
                                               //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                               OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                               OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                               OutGoingField.Add(OutgoingRecord);
                                           }
                                       }
                                       catch (Exception ex)
                                       {
                                           ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag3: " + ex.Message, true);
                                           continue;
                                       }
                                   }
                               }
                           }

                           if (dt.Columns.Contains("Producer 5: Full Name") && dt.Columns.Contains("Producer 5 First Year %") && dt.Columns.Contains("Producer 5 Renewal %"))
                           {
                               if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 5: Full Name"])))
                               {
                                   string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 5: Full Name"]);
                                   int spIndex = strPrimaryBroker.IndexOfAny(spCharac);
                                   strPrimaryBroker = (spIndex > 0) ? strPrimaryBroker.Substring(0, spIndex - 1) : strPrimaryBroker;
                                   ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " strPrimaryBroker pri5: " + strPrimaryBroker, true);


                                   if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                   {
                                       try
                                       {
                                            //User objUser = UsrLst.Where(u => u.UserName.ToLower() == strPrimaryBroker.ToLower()).FirstOrDefault();
                                            //Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            //var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            //var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.RoleId == 3 && (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower()))).FirstOrDefault();

                                            Guid tempGuid = Guid.Empty;
                                           Guid.TryParse(strPrimaryBroker, out tempGuid);
                                           if (tempGuid != Guid.Empty)
                                           {
                                               objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                           }
                                           if (objUser != null)
                                           {
                                               string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 5 First Year %"]);
                                               string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 5 Renewal %"]);
                                               OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                               OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                               OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                               OutgoingRecord.IsPrimaryAgent = false;
                                               double frst = 0; double renew = 0;
                                               double.TryParse(strFirstPer, out frst);
                                               double.TryParse(strRenewalPer, out renew);

                                               OutgoingRecord.FirstYearPercentage = frst;// string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                               OutgoingRecord.RenewalPercentage = renew;
                                               //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                               OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                               OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                               OutGoingField.Add(OutgoingRecord);
                                           }
                                       }
                                       catch (Exception ex)
                                       {
                                           ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag4: " + ex.Message, true);
                                           continue;
                                       }
                                   }
                               }
                           }

                           if (dt.Columns.Contains("Producer 6: Full Name") && dt.Columns.Contains("Producer 6 First Year %") && dt.Columns.Contains("Producer 6 Renewal %"))
                           {
                               if (!string.IsNullOrWhiteSpace(Convert.ToString(dt.Rows[i]["Producer 6: Full Name"])))
                               {
                                   string strPrimaryBroker = Convert.ToString(dt.Rows[i]["Producer 6: Full Name"]);

                                   int spIndex = strPrimaryBroker.IndexOfAny(spCharac);
                                   strPrimaryBroker = (spIndex > 0) ? strPrimaryBroker.Substring(0, spIndex - 1) : strPrimaryBroker;
                                   ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " strPrimaryBroker pri6: " + strPrimaryBroker, true);

                                   if (!string.IsNullOrWhiteSpace(strPrimaryBroker))
                                   {
                                       try
                                       {
                                            //Guid guidBrokerID = new Guid(strPrimaryBroker);
                                            //var objUser = AgentList.Where(u => u.UserCredentialId == guidBrokerID).FirstOrDefault();
                                            //var objUser = AgentList.Where(u => (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower())).FirstOrDefault();
                                            var objUser = AgentList.Where(u => (u.RoleId == 3 && (u.FirstName + " " + u.LastName).ToLower() == strPrimaryBroker.ToLower() || (u.LastName + " " + u.FirstName).ToLower() == strPrimaryBroker.ToLower() || (!string.IsNullOrEmpty(u.NickName) && u.NickName.ToLower() == strPrimaryBroker.ToLower()))).FirstOrDefault();

                                            Guid tempGuid = Guid.Empty;
                                           Guid.TryParse(strPrimaryBroker, out tempGuid);
                                           if (tempGuid != Guid.Empty)
                                           {
                                               objUser = AgentList.Where(d => d.UserCredentialId == tempGuid).FirstOrDefault();
                                           }
                                           if (objUser != null)
                                           {
                                               string strFirstPer = Convert.ToString(dt.Rows[i]["Producer 6 First Year %"]);
                                               string strRenewalPer = Convert.ToString(dt.Rows[i]["Producer 6 Renewal %"]);
                                               OutGoingPayment OutgoingRecord = new OutGoingPayment();
                                               OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                                               OutgoingRecord.PolicyId = objPolicy.PolicyId;
                                               OutgoingRecord.IsPrimaryAgent = false;
                                               double frst = 0; double renew = 0;
                                               double.TryParse(strFirstPer, out frst);
                                               double.TryParse(strRenewalPer, out renew);

                                               OutgoingRecord.FirstYearPercentage = frst;// string.IsNullOrEmpty(strFirstPer) ? 0 : Convert.ToDouble(strFirstPer);
                                               OutgoingRecord.RenewalPercentage = renew;
                                               //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                                               OutgoingRecord.PayeeUserCredentialId = objUser.UserCredentialId;
                                               OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                                               OutGoingField.Add(OutgoingRecord);
                                           }
                                       }
                                       catch (Exception ex)
                                       {
                                           ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception adding outgoing for ag5: " + ex.Message, true);
                                           continue;
                                       }
                                   }
                               }
                           }
                           //Check for House if present
                           //try
                           //{
                           //    string strHouse = Convert.ToString(dt.Rows[i]["houseuc"]);
                           //    string strhousefy = Convert.ToString(dt.Rows[i]["housefy"]);
                           //    string strhousery = Convert.ToString(dt.Rows[i]["housern"]);
                           //    if (!string.IsNullOrEmpty(strHouse))
                           //    {
                           //        OutGoingPayment OutgoingRecord = new OutGoingPayment();
                           //        OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                           //        OutgoingRecord.PolicyId = objPolicy.PolicyId;
                           //        OutgoingRecord.IsPrimaryAgent = true;
                           //        OutgoingRecord.FirstYearPercentage = string.IsNullOrEmpty(strhousefy) ? 0 : Convert.ToDouble(strhousefy);
                           //        OutgoingRecord.RenewalPercentage = string.IsNullOrEmpty(strhousery) ? 0 : Convert.ToDouble(strhousery);
                           //        //   OutgoingRecord.Payor = objPolicy.PayorNickName;
                           //        //houseOwner = 
                           //        OutgoingRecord.PayeeUserCredentialId = new Guid(strHouse);
                           //        OutgoingRecord.ScheduleTypeId = 2;// OutPercentOfPremium ? 1 : 2;
                           //        if (OutgoingRecord.FirstYearPercentage > 0 || OutgoingRecord.RenewalPercentage > 0)
                           //            OutGoingField.Add(OutgoingRecord);
                           //    }
                           //}
                           //catch (Exception ex)
                           //{
                           //    ActionLogger.Logger.WriteImportLogDetail("Exception setting house owner: " + ex.Message + ", so chcking frim licensee ", true);
                           //    continue;
                           //}
                           #endregion

                       #endregion
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy: outgoing split init done : " + policyIDKey, true);
                       }
                       catch (Exception ex)
                       {
                           errorCount++;
                           errorList.Add(importedPolicyID);
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: outgoing fields  : " + ex.Message, true);
                           continue;
                       }
                       #region Save Data
                       try
                       {
                           if (errorList == null || !errorList.Contains(importedPolicyID))
                           {
                               #region Client
                               try
                               {

                                   if (isNewPolicy || objPolicy.PolicyClientId == null)
                                   {
                                       string client = Convert.ToString(dt.Rows[i]["Account Name"]);
                                       Client objClient = (new Client()).GetClientByClientName(client, LicID);
                                       //Get Client ID by Get Client name
                                       if (objClient == null)
                                       {
                                           //Create new client
                                           objClient = new Client();
                                           objClient.ClientId = Guid.NewGuid();
                                           string strClientValue = string.Empty;

                                           strClientValue = (client.Length > 49) ? client.Substring(0, 49) : client;

                                           //objClnt.Name = policy.ClientName;
                                           objClient.Name = strClientValue;
                                           objClient.LicenseeId = LicID;
                                           objClient.IsDeleted = false;
                                           Client.AddUpdateClient(client, LicID, objClient.ClientId);
                                           ActionLogger.Logger.WriteImportLogDetail("Import Policy: client saved as new : " + policyIDKey, true);
                                       }
                                       objPolicy.ClientReference.Value = (from p in DataModel.Clients where p.ClientId == objClient.ClientId select p).FirstOrDefault();
                                   }
                                   //following requires client reference, so added at last
                                   if (isNewPolicy || string.IsNullOrEmpty(objPolicy.Insured))
                                   {
                                       if (dt.Columns.Contains("insured?"))
                                       {
                                           objPolicy.Insured = Convert.ToString(dt.Rows[i]["insured"]);
                                       }
                                       else
                                       {
                                           objPolicy.Insured = objPolicy.Client.Name;
                                       }
                                   }

                               }
                               catch (Exception ex)
                               {
                                   errorCount++;
                                   errorList.Add(importedPolicyID);
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: client : " + ex.Message, true);
                                   continue;
                               }
                               #endregion
                               if (isNewPolicy)
                               {
                                   DataModel.AddToPolicies(objPolicy);
                                   DataModel.SaveChanges();
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy : policy saved successfully", true);
                                   PolicyToLearnPost.AddLearnedAfterImport(objPolicy.PolicyId, "", covNickName, strProductType, importedPolicyID);
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy : learned fields saved successfully", true);
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy: Policy saved as new : " + policyIDKey, true);

                               }
                               else
                               {
                                   DataModel.SaveChanges();
                                   AddUpdatePolicyHistory(objPolicy.PolicyId);
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy : policy updated successfully", true);
                                   //Learned fields to update , once policy fields are updated 
                                   PolicyToLearnPost.AddLearnedAfterImport(objPolicy.PolicyId, "", covNickName, strProductType, importedPolicyID);
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy : learned fields saved successfully", true);
                                   PolicyLearnedField.AddUpdateHistoryLearned(objPolicy.PolicyId);
                                   ActionLogger.Logger.WriteImportLogDetail("Import Policy: Policy updated : " + policyIDKey, true);
                               }


                               //Save Incoming 
                               if (inSchedule != null)
                               {
                                   inSchedule.AddUpdate();
                                   ActionLogger.Logger.WriteImportLogDetail("Incoming schedule added for the policy", true);
                               }

                               //Save Outgoing
                               //Delete old schdule- outgoing  payments witj old schedule will remian intact, new will take effect on new entries only.
                               //Check only if new outgoing schedule exists, then overwrite the old
                               //This is to exclude the case when policy is existing and excel has blank entries. Without this, existing schedule will be overwritten
                               // by 100% to house for existing policy 
                               if (isNewPolicy ||  OutGoingField.Count > 0)
                               {
                                   ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule adding to the policy", true);
                                   OutGoingField = CompleteOutgoingSchedule(OutGoingField, objPolicy.PolicyId, LicID);
                                   OutGoingPayment.DeletePolicyOutGoingSchedulebyPolicyId(objPolicy.PolicyId);
                                   ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule removed from the policy", true);
                                   OutGoingPayment.AddUpdate(OutGoingField);
                                   ActionLogger.Logger.WriteImportLogDetail("Outgoing schedule added for the policy", true);
                               }

                               if (isNewPolicy) addCount++;
                               else updateCount++;
                           }
                       }
                       catch (Exception ex)
                       {
                           errorCount++;
                           errorList.Add(importedPolicyID);
                           ActionLogger.Logger.WriteImportLogDetail("Import Policy exception: saving data fields  : " + ex.Message, true);
                           continue;
                       }
                       #endregion
                   
                   }
                   catch (Exception ex)
                   {
                       ActionLogger.Logger.WriteImportLogDetail("Exception adding policy: " + ex.Message, true);
                       errorCount++;
                       errorList.Add(importedPolicyID);
                   }
               }
               
            }
            objStatus.ImportCount = addCount;
            objStatus.UpdateCount = updateCount;
            objStatus.ErrorCount = errorCount;
            objStatus.ErrorList = errorList;
            return objStatus;
        }

        static List<OutGoingPayment>  CompleteOutgoingSchedule(List<OutGoingPayment> OutGoingPayments, Guid PolicyID, Guid LicID)
        {
            List<OutGoingPayment> OutGoingField = OutGoingPayments;
            Guid houseAcct = PostUtill.GetPolicyHouseOwner((Guid)LicID);
            try
            {
                if (OutGoingField != null && OutGoingField.Count > 0)
                {
                    //Check total payment fy and ry 
                    double fyTotal = OutGoingField.Where(x => x.FirstYearPercentage != null).Sum(x => Convert.ToDouble(x.FirstYearPercentage));
                    double ryTotal = OutGoingField.Where(x => x.RenewalPercentage != null).Sum(x => Convert.ToDouble(x.RenewalPercentage));
                    if (fyTotal < 100 || ryTotal < 100)
                    {
                        OutGoingPayment OutgoingRecord = new OutGoingPayment();
                        OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                        OutgoingRecord.PolicyId = PolicyID;
                    //    OutgoingRecord.IsPrimaryAgent = false;
                        OutgoingRecord.FirstYearPercentage = Convert.ToDouble(100 - fyTotal);
                        OutgoingRecord.RenewalPercentage = Convert.ToDouble(100 - ryTotal);
                        OutgoingRecord.PayeeUserCredentialId = houseAcct;
                        OutgoingRecord.ScheduleTypeId = 2;
                        OutGoingField.Add(OutgoingRecord);
                    }
                }
                else
                {
                    //No previous payment, add new with house account 
                    OutGoingPayment OutgoingRecord = new OutGoingPayment();
                    OutgoingRecord.OutgoingScheduleId = Guid.NewGuid();
                    OutgoingRecord.PolicyId = PolicyID;
                   // OutgoingRecord.IsPrimaryAgent = true;
                    OutgoingRecord.FirstYearPercentage = 100;// string.IsNullOrEmpty(fy) ? 0 : Convert.ToDouble(fy);
                    OutgoingRecord.RenewalPercentage = 100;// string.IsNullOrEmpty(ry) ? 0 : Convert.ToDouble(ry);
                    OutgoingRecord.PayeeUserCredentialId = houseAcct;
                    OutgoingRecord.ScheduleTypeId = 2;
                    OutGoingField.Add(OutgoingRecord);
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail("Exception CompleteOutgoingSchedule :  " + ex.Message,true);
            }
            return OutGoingField;
        }

        static int PolicyModeID(string strModeType)
        {
            int intStatus = 0;

            switch (strModeType.ToLower())
            {
                case "monthly":
                    intStatus = 0;
                    break;

                case "quarterly":
                    intStatus = 1;
                    break;

                case "semi-annually":
                    intStatus = 2;
                    break;

                case "annually":
                    intStatus = 3;
                    break;

                case "one time":
                    intStatus = 4;
                    break;

                case "random":
                    intStatus = 5;
                    break;

                default:
                    intStatus = 0;
                    break;
            }
            return intStatus;
        }


        static int PolicTermisionID(string strTermReason)
        {
            int intStatus = 0;

            switch (strTermReason.ToLower())
            {
                case "replaced by new policy":
                    intStatus = 0;
                    break;

                case "lost to competitor":
                    intStatus = 1;
                    break;

                case "voluntary":
                    intStatus = 2;
                    break;

                case "out of business":
                    intStatus = 3;
                    break;

                case "non-payment":
                    intStatus = 4;
                    break;

                case "per carrier":
                    intStatus = 5;
                    break;

                default:
                    intStatus = 0;
                    break;
            }
            return intStatus;
        }

        //static DataTable GetDefaultTemplateTable()
        //{
        //    DataTable dt = new DataTable();
        //    string path = System.Configuration.ConfigurationSettings.AppSettings["ExcelPath"];
        //    try
        //    {
        //        using (var pck = new OfficeOpenXml.ExcelPackage())
        //        {
        //            using (var stream = File.OpenRead(path))
        //            {
        //                pck.Load(stream);
        //            }
        //            var ws = pck.Workbook.Worksheets.First();
        //            DataTable tbl = new DataTable("Temp");
        //            foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
        //            {
        //                tbl.Columns.Add(firstRowCell.Text);
        //               // tbl.Columns.Add(firstRowCell.Start.Column);
        //            }
        //            var startRow =  1;
        //            for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
        //            {
        //                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
        //                DataRow row = tbl.Rows.Add();
        //                foreach (var cell in wsRow)
        //                {
        //                    try
        //                    {
        //                        row[cell.Start.Column - 1] = cell.Text;
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        ActionLogger.Logger.WriteImportLogDetail("Exception GetDataTableFromExcel: " + ex.Message,true);
        //                        throw ex;
        //                    }
        //                }
        //            }
        //            return tbl;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ActionLogger.Logger.WriteImportLogDetail("Exception GetDataTableFromExcel: " + ex.Message,true);
        //        throw ex;
        //    }
        //}

        public static string CheckExcelFormat(DataTable dt )
        {
            string result = "";
            try
            {
                ActionLogger.Logger.WriteImportLogDetail("Starting CheckExcelFormat: ", true);
               // DataTable tbRef = GetDefaultTemplateTable();

                /*for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string col = dt.Columns[j].ColumnName;
                    if (!tbRef.Columns.Contains(col))
                    {
                        ActionLogger.Logger.WriteImportLogDetail("CheckExcelFormat mismatch in  tbRef.Columns.Contain at: " + col, true);
                        result = "Your excel file contains additional column '" + col + "'" ;
                        return result;
                    }
                }*/

                /* Not comparing full template right now, only mandatiry fields as per discussion
                 * for (int j = 0; j < tbRef.Columns.Count; j++)
                 {
                     string col = tbRef.Columns[j].ColumnName;
                     if (!dt.Columns.Contains(col))
                     {
                         ActionLogger.Logger.WriteImportLogDetail("CheckExcelFormat mismatch in dt.Columns.Contain at: " + col, true);
                         result = "Your excel file doesn't have required column '" + col + "'";
                         return result;
                     }
                 }*/
                string missCol = "";

                if (!dt.Columns.Contains("Vision Plan ID") && !dt.Columns.Contains("Policy Plan ID"))
                {
                    ActionLogger.Logger.WriteImportLogDetail("CheckExcelFormat mismatch in  planid", true);
                    missCol += "\nVision Plan ID";
                    //result = "Your excel file contains additional column '" + col + "'";
                    //return result;
                }

                if (!dt.Columns.Contains("Account Name"))
                {
                    ActionLogger.Logger.WriteImportLogDetail("CheckExcelFormat mismatch in  planid", true);
                    missCol += "\nAccount Name";
                }

                if (!dt.Columns.Contains("Original Plan Start Date"))
                {
                    ActionLogger.Logger.WriteImportLogDetail("CheckExcelFormat mismatch in  planid", true);
                    missCol += "\nOriginal Plan Start Date";
                }
                if (!string.IsNullOrEmpty(missCol))
                {
                    result = "Your excel file is missing following columns:\n" + missCol;
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail("Exception CheckExcelFormat: " + ex.Message, true);
                return result;
            }
            ActionLogger.Logger.WriteImportLogDetail("CheckExcelFormat success ", true);
            return result;
        }

        #endregion


        #region Save and Delete Methods

        public static void AddUpdatePolicy(PolicyDetailsData _PolicyRecord)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _PolicyRecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null)
                {
                    _policy = new DLinq.Policy
                    {
                        PolicyId = _PolicyRecord.PolicyId,
                        PolicyNumber = _PolicyRecord.PolicyNumber,
                        PolicyType = _PolicyRecord.PolicyType,
                        Insured = _PolicyRecord.Insured,
                        OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate,
                        TrackFromDate = _PolicyRecord.TrackFromDate,
                        MonthlyPremium = _PolicyRecord.ModeAvgPremium,
                        SubmittedThrough = _PolicyRecord.SubmittedThrough,
                        Enrolled = _PolicyRecord.Enrolled,
                        Eligible = _PolicyRecord.Eligible,
                        PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate,
                        IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth,
                        IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage,
                        IsTrackPayment = _PolicyRecord.IsTrackPayment,
                        IsDeleted = false,
                        ReplacedBy = _PolicyRecord.ReplacedBy,
                        DuplicateFrom = _PolicyRecord.DuplicateFrom,
                        CreatedOn = DateTime.Now,
                        IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule,
                        IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule,
                        SplitPercentage = _PolicyRecord.SplitPercentage,
                        Advance=_PolicyRecord.Advance,
                        ProductType = _PolicyRecord.ProductType,
                        UserCredentialId = _PolicyRecord.UserCredentialId,
                        AccoutExec = _PolicyRecord.AccoutExec,
                        //new fields
                        PrimaryAgent = _PolicyRecord.PrimaryAgent,
                        IsCustomBasicSchedule = _PolicyRecord.IsCustomBasicSchedule,
                        CustomScheduleDateType = _PolicyRecord.CustomDateType,
                    };
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();

                    //_policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.PolicyModeId = _PolicyRecord.PolicyModeId;

                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();

                    if (_policy.PolicyStatusId == ((int)_PolicyStatus.Active))
                        _policy.ActivatedOn = _policy.CreatedOn;
                    else
                        _policy.ActivatedOn = null;

                    DataModel.AddToPolicies(_policy);

                }
                else
                {
                    _policy.PolicyId = _PolicyRecord.PolicyId;
                    _policy.PolicyNumber = _PolicyRecord.PolicyNumber;
                    _policy.PolicyType = _PolicyRecord.PolicyType;
                    _policy.Insured = _PolicyRecord.Insured;
                    _policy.OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate;
                    _policy.TrackFromDate = _PolicyRecord.TrackFromDate;
                    _policy.MonthlyPremium = _PolicyRecord.ModeAvgPremium;
                    _policy.SubmittedThrough = _PolicyRecord.SubmittedThrough;
                    _policy.Enrolled = _PolicyRecord.Enrolled;
                    _policy.Eligible = _PolicyRecord.Eligible;
                    _policy.PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate;
                    _policy.IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth;
                    _policy.IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage;
                    _policy.IsTrackPayment = _PolicyRecord.IsTrackPayment;
                    _policy.IsDeleted = _PolicyRecord.IsDeleted;
                    _policy.ReplacedBy = _PolicyRecord.ReplacedBy;
                    _policy.DuplicateFrom = _PolicyRecord.DuplicateFrom;
                    _policy.IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule;
                    _policy.IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule;
                    _policy.SplitPercentage = _PolicyRecord.SplitPercentage;
                    //recently added
                    _policy.Advance = _PolicyRecord.Advance;
                    _policy.ProductType = _PolicyRecord.ProductType;

                    //added 15012016
                    _policy.UserCredentialId = _PolicyRecord.UserCredentialId;
                    _policy.AccoutExec = _PolicyRecord.AccoutExec;


                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();

                    if ((_policy.PolicyStatusId == ((int)_PolicyStatus.Pending)) && (_PolicyRecord.PolicyStatusId == ((int)_PolicyStatus.Active)))
                    {
                        if (_policy.ActivatedOn == null)
                        {
                            _policy.ActivatedOn = DateTime.Now;
                        }
                    }
                   
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                   
                    if (_PolicyRecord.PolicyModeId != null)
                        _policy.PolicyModeId = _PolicyRecord.PolicyModeId;
                    else
                        _policy.PolicyModeId = new System.Nullable<int>();

                    if (_PolicyRecord.CoverageId == Guid.Empty)
                    {
                        _policy.CoverageId = null;
                    }
                    else
                    {
                        _policy.CoverageId = _PolicyRecord.CoverageId;
                    }

                    //Add /Update termination date
                    if ((_PolicyRecord.PolicyStatusId == ((int)_PolicyStatus.Pending)) || (_PolicyRecord.PolicyStatusId == ((int)_PolicyStatus.Active)))
                    {
                        _policy.TerminationReasonId = null;
                    }
                    else
                    {
                        if (_PolicyRecord.TerminationReasonId != null)
                        {
                            _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                        }
                        else
                        {
                            _policy.TerminationReasonId = null;
                        }
                    }
                   // _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();

                    if (_PolicyRecord.PayorId == Guid.Empty)
                    {
                        _policy.PayorId = null;
                    }
                    else
                    {
                        _policy.PayorId = _PolicyRecord.PayorId;
                    }
                    // _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();

                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    if (_PolicyRecord.CarrierID == Guid.Empty)
                    {
                        _policy.CarrierId = null;
                    }
                    else
                    {
                        _policy.CarrierId = _PolicyRecord.CarrierID;
                    }
                    //New fields
                    _policy.PrimaryAgent = _PolicyRecord.PrimaryAgent;
                    _policy.IsCustomBasicSchedule = _PolicyRecord.IsCustomBasicSchedule;
                    _policy.CustomScheduleDateType = _PolicyRecord.CustomDateType;
                    // _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();

                }
                //DataModel.Policies.Where(p => p.PolicyId == _PolicyRecord.PolicyId).FirstOrDefault().CoverageId = null;
                DataModel.SaveChanges();
            }
        }

        public static void UpdatePendingPolicy(DEU _DEU)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _DEU.PolicyId) select p).FirstOrDefault();
                if (_policy != null)
                {
                    // in case of pending policy
                    //Update the following details
                    if (_policy.PolicyStatusId==2)
                    {
                        if (_DEU.SplitPer != null)
                            _policy.SplitPercentage = _DEU.SplitPer;

                        if (_DEU.PolicyMode != null)
                            _policy.PolicyModeId = _DEU.PolicyMode;

                        if (_DEU.OriginalEffectiveDate != null)
                            _policy.OriginalEffectiveDate = _DEU.OriginalEffectiveDate;

                        //if (_DEU.TrackFromDate != null)
                        //    _policy.TrackFromDate = _DEU.TrackFromDate;

                        if (_DEU.Eligible != null)                       
                            _policy.Eligible = _DEU.Eligible;                        

                        if (_DEU.Enrolled != null)                      
                            _policy.Enrolled = _DEU.Enrolled;

                        if (_DEU.CompTypeID != null)
                            _policy.IncomingPaymentTypeId = _DEU.CompTypeID;                        
                        
                      
                        DataModel.SaveChanges();
                    }
                    
                }
            }
        }

        public static void UpdatePolicyClient(Guid policyId,Guid clientID)
        {
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var _policy = (from p in DataModel.Policies where (p.PolicyId == policyId) select p).FirstOrDefault();
                    if (_policy != null)
                    {
                        _policy.PolicyClientId = clientID;
                        DataModel.SaveChanges();


                    }
                }
            }
            catch
            {
            }
        }

        public static void UpdatePolicyClientLernedFields(Guid policyId, Guid clientID)
        {
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var _policy = (from p in DataModel.PolicyLearnedFields where (p.PolicyId == policyId) select p).FirstOrDefault();
                    if (_policy != null)
                    {
                        _policy.ClientID = clientID;
                         DataModel.SaveChanges();


                    }
                }
            }
            catch
            {
            }
        }

        public static void UpdateMode(Guid _PolicyID, int PolicyMode)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _PolicyID) select p).FirstOrDefault();
                if (_policy != null)
                {
                    // in case of pending policy
                    if (_policy.PolicyStatusId == 2)
                    {
                        _policy.PolicyModeId = PolicyMode;
                        DataModel.SaveChanges();
                    }

                }
            }
        }

        public static PolicySavedStatus SavePolicyData(PolicyDetailsData OriginalPolicy, PolicyDetailsData ReplacedPolicy)
        {
            PolicySavedStatus status = new PolicySavedStatus();

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(15)
            };

            if (OriginalPolicy != null || ReplacedPolicy != null)
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, options))
                    {
                        if (ReplacedPolicy != null)
                        {
                            Policy.AddUpdatePolicy(ReplacedPolicy);
                            Policy.AddUpdatePolicyHistory(ReplacedPolicy.PolicyId);
                            PolicyLearnedField.AddUpdateHistoryLearned(ReplacedPolicy.PolicyId);
                        }
                        if (OriginalPolicy != null)
                        {
                            Policy.AddUpdatePolicy(OriginalPolicy);
                            PolicyToLearnPost.AddUpdatPolicyToLearn(OriginalPolicy.PolicyId);
                            Policy.AddUpdatePolicyHistory(OriginalPolicy.PolicyId);
                            PolicyLearnedField.AddUpdateHistoryLearned(OriginalPolicy.PolicyId);
                        }
                        transaction.Complete();
                    }
                }
                catch
                {
                    status.IsError = true;
                    status.ErrorMessage = "Policy " + OriginalPolicy.PolicyNumber + " is not saved succesfully.";
                }
            }

            return status;
        }

        public static PolicySavedStatus SavePolicy(PolicyDetailsData OriginalPolicy, PolicyDetailsData ReplacedPolicy, string strRenewal,string strCoverageNickName)
        {
            PolicySavedStatus status = new PolicySavedStatus();

            TransactionOptions options = new TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromMinutes(60)
            };

            if (OriginalPolicy != null || ReplacedPolicy != null)
            {
                try
                {
                    using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, options))
                    {
                        if (ReplacedPolicy != null)
                        {
                            Policy.AddUpdatePolicy(ReplacedPolicy);
                            Policy.AddUpdatePolicyHistory(ReplacedPolicy.PolicyId);
                            PolicyLearnedField.AddUpdateHistoryLearned(ReplacedPolicy.PolicyId);
                        }
                        if (OriginalPolicy != null)
                        {
                            //ActionLogger.Logger.WriteImportLogDetail(" " + " Call AddUpdatePolicy", true);
                            Policy.AddUpdatePolicy(OriginalPolicy);
                            //ActionLogger.Logger.WriteImportLogDetail(" " + "Call AddPolicyToLearn", true);                           
                            PolicyToLearnPost.AddPolicyToLearn(OriginalPolicy.PolicyId, strRenewal, strCoverageNickName, OriginalPolicy.ProductType);
                            //ActionLogger.Logger.WriteImportLogDetail(" " + "Call AddUpdatePolicyHistory", true);

                            //Thread thPolicyHistory = new Thread(() =>
                            //{
                            //    Policy.AddUpdatePolicyHistory(OriginalPolicy.PolicyId);
                            //});
                            //thPolicyHistory.IsBackground = true;
                            //thPolicyHistory.Start();

                            //Thread thPolicyLearnedHistory = new Thread(() =>
                            //{
                            //    PolicyLearnedField.AddUpdateHistoryLearned(OriginalPolicy.PolicyId);
                            //});
                            //thPolicyLearnedHistory.IsBackground = true;
                            //thPolicyLearnedHistory.Start();
                        }
                        transaction.Complete();
                    }
                }
                catch (Exception ex)
                {
                    status.IsError = true;
                    status.ErrorMessage = "Policy " + OriginalPolicy.PolicyNumber + " is not saved succesfully.";
                    ActionLogger.Logger.WriteImportLogDetail("Exception saving policy : " + ex.Message, true);
                    if(ex.InnerException!=null)
                    {
                        ActionLogger.Logger.WriteImportLogDetail("Inner Exception saving policy : " + ex.InnerException.Message, true);
                        ActionLogger.Logger.WriteImportLogDetail("Inner Exception saving policy : " + ex.InnerException.StackTrace, true);
                    }
                }
            }

            return status;
        }

        //public static PolicySavedStatus SavePolicy(PolicyDetailsData OriginalPolicy, PolicyDetailsData ReplacedPolicy, string strRenewal)
        //{
        //    PolicySavedStatus status = new PolicySavedStatus();

        //    TransactionOptions options = new TransactionOptions
        //    {
        //        IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
        //        Timeout = TimeSpan.FromMinutes(15)
        //    };

        //    if (OriginalPolicy != null || ReplacedPolicy != null)
        //    {
        //        try
        //        {
        //            using (TransactionScope transaction = new TransactionScope(TransactionScopeOption.Required, options))
        //            {
        //                if (ReplacedPolicy != null)
        //                {
        //                    Policy.AddUpdatePolicy(ReplacedPolicy);
        //                    Policy.AddUpdatePolicyHistory(ReplacedPolicy.PolicyId);
        //                    PolicyLearnedField.AddUpdateHistoryLearned(ReplacedPolicy.PolicyId);
        //                }
        //                if (OriginalPolicy != null)
        //                {
        //                    //ActionLogger.Logger.WriteImportLogDetail(" " + " Call AddUpdatePolicy", true);
        //                    Policy.AddUpdatePolicy(OriginalPolicy);
        //                    //ActionLogger.Logger.WriteImportLogDetail(" " + "Call AddPolicyToLearn", true);                           
        //                    PolicyToLearnPost.AddPolicyToLearn(OriginalPolicy.PolicyId, strRenewal);
        //                    //ActionLogger.Logger.WriteImportLogDetail(" " + "Call AddUpdatePolicyHistory", true);
        //                    Thread thPolicyHistory = new Thread(() =>
        //                        {
        //                            Policy.AddUpdatePolicyHistory(OriginalPolicy.PolicyId);
        //                        });
        //                    thPolicyHistory.IsBackground = true;
        //                    thPolicyHistory.Start();

        //                    Thread thPolicyLearnedHistory = new Thread(() =>
        //                        {
        //                            PolicyLearnedField.AddUpdateHistoryLearned(OriginalPolicy.PolicyId);
        //                        });
        //                    thPolicyLearnedHistory.IsBackground = true;
        //                    thPolicyLearnedHistory.Start();
        //                }
        //                transaction.Complete();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            status.IsError = true;
        //            status.ErrorMessage = "Policy " + OriginalPolicy.PolicyNumber + " is not saved succesfully.";
        //        }
        //    }

        //    return status;
        //}

        public static void UpdateRPolicyStatus(PolicyDetailsData _policyr)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _policyr.PolicyId) select p).FirstOrDefault();
                if (_policy != null)
                {
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == (int)_PolicyStatus.Terminated select m).FirstOrDefault();
                    DataModel.SaveChanges();
                }
            }
        }

        public void UpdatePolicyTermDate(Guid policyID, DateTime? dtTermReson)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == policyID) select p).FirstOrDefault();
                if (_policy != null)
                {
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == (int)_PolicyStatus.Terminated select m).FirstOrDefault();
                    // _policy.PolicyStatusId=1;
                    //Term reson "Per Carrier
                    _policy.TerminationReasonId = 5;
                    _policy.PolicyTerminationDate = dtTermReson;
                    DataModel.SaveChanges();
                }
            }
        }

        public static void UpdatePolicySetting(PolicyDetailsData policy)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DataModel.Policies.Where(p => p.PolicyId == policy.PolicyId).FirstOrDefault().IsTrackMissingMonth = policy.IsTrackMissingMonth;
                DataModel.Policies.Where(p => p.PolicyId == policy.PolicyId).FirstOrDefault().IsTrackIncomingPercentage = policy.IsTrackIncomingPercentage;
                DataModel.SaveChanges();
            }
        }

        public static string GetPolicyProductType(Guid policyID, Guid PayorID, Guid CarrierID, Guid CoverageID)
        {
            string strNickName = string.Empty;

            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    var strproductType = from p in DataModel.Policies
                                              where (p.PolicyId == policyID && p.PayorId == PayorID && p.CarrierId==CarrierID && p.CoverageId==CoverageID)
                                              select p.ProductType;

                    foreach (var item in strproductType)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            strNickName = Convert.ToString(item);
                        }

                    }

                }

            }
            catch
            {
            }
            return strNickName;
        }

        public static void UpdateLastFollowupRunsWithTodayDate(Guid PolicyId)
        {
            try
            {
                if (PolicyId == Guid.Empty) return;
                if (PolicyId == null) return;
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DataModel.UpdatelastFollowupRuns(PolicyId);
                }
            }
            catch
            {
            }

        }

        public static void MarkPolicyDeleted(PolicyDetailsData _policyrecord)
        {
            ActionLogger.Logger.WriteImportLogDetail("MarkPolicyDeleted : PolicyID - " + _policyrecord.PolicyId + ", UserID: " + _policyrecord.UserCredentialId, true); 
          using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                try
                {
                    var _policy = (from p in DataModel.Policies where (p.PolicyId == _policyrecord.PolicyId) select p).FirstOrDefault();
                    if (_policy != null)
                    {
                        _policy.IsDeleted = true;
                        DataModel.SaveChanges();
                        ActionLogger.Logger.WriteImportLogDetail("MarkPolicyDeleted : PolicyID - " + _policyrecord.PolicyId + " deleted successfully by UserID: " + _policyrecord.UserCredentialId, true);
                    }
                    else
                    {
                        ActionLogger.Logger.WriteImportLogDetail("MarkPolicyDeleted : PolicyID - " + _policyrecord.PolicyId + " not found for deletion", true);
                    }
                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteImportLogDetail("MarkPolicyDeleted exception: PolicyID - " + _policyrecord.PolicyId + ", exception: " + ex.Message, true);
                }
            }
        }

        public static void MarkPolicyDeletedById(Guid PolicyId)
        {
            ActionLogger.Logger.WriteImportLogDetail("MarkPolicyDeletedById : PolicyID - " + PolicyId, true); 
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                try
                {
                    var _policy = (from p in DataModel.Policies where (p.PolicyId == PolicyId) select p).FirstOrDefault();
                    if (_policy != null)
                    {
                        _policy.IsDeleted = true;
                        DataModel.SaveChanges();
                    }
                    ActionLogger.Logger.WriteImportLogDetail("MarkPolicyDeletedById completed : PolicyID - " + PolicyId, true); 
                }
                catch(Exception ex)
                {
                    ActionLogger.Logger.WriteImportLogDetail("MarkPolicyDeletedById  exception: PolicyID - " + PolicyId + ", ex: " +ex.Message, true); 
                }
            }
        }

        public static void DeletePolicyFromDB(PolicyDetailsData _Policy)
        {
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() +  " DeletePolicyFromDB started: " + _Policy.PolicyId,true);
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == _Policy.PolicyId) select p).FirstOrDefault();
                //Check null before going to delete on the basis of Policy ID
                if (_policy == null) return;
                DataModel.DeleteObject(_policy);
                DataModel.SaveChanges();

            }
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeletePolicyFromDB ended: " + _Policy.PolicyId, true);
        }

        public static void DeletePolicyFromDBById(Guid PolicyId)
        {
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeletePolicyFromDBById started: " + PolicyId, true);
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.Policies where (p.PolicyId == PolicyId) select p).FirstOrDefault();
                DataModel.DeleteObject(_policy);
                DataModel.SaveChanges();
            }
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeletePolicyFromDBById ended: " + PolicyId, true);
        }

        public static void DeletePolicyCascadeFromDBById(Guid PolicyId)
        {
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeletePolicyCascadeFromDBById started: " + PolicyId, true);
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    //PolicyOutgoingSchedule
                    List<DLinq.PolicyOutgoingSchedule> _PolicyOutgoingSchedulelst = DataModel.PolicyOutgoingSchedules.Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyOutgoingSchedulelst != null)
                    {
                        foreach (DLinq.PolicyOutgoingSchedule pos in _PolicyOutgoingSchedulelst)
                            DataModel.DeleteObject(pos);
                    }

                    //PolicyOutgoingAdvancedSchedule 
                    List<DLinq.PolicyOutgoingAdvancedSchedule> _PolicyOutgoingAdvancedScheduleLst = DataModel.PolicyOutgoingAdvancedSchedules
                                                                                                    .Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyOutgoingAdvancedScheduleLst != null)
                    {
                        foreach (DLinq.PolicyOutgoingAdvancedSchedule poas in _PolicyOutgoingAdvancedScheduleLst)
                        {
                            DataModel.DeleteObject(poas);
                        }

                    }
                    //PolicyNote
                    List<DLinq.PolicyNote> _PolicyNoteLst = DataModel.PolicyNotes.Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyNoteLst != null)
                    {
                        foreach (DLinq.PolicyNote pn in _PolicyNoteLst)
                        {
                            DataModel.DeleteObject(pn);
                        }
                    }

                    //PolicyLevelBillingDetail
                    List<DLinq.PolicyLevelBillingDetail> _PolicyLevelBillingDetailLst = DataModel.PolicyLevelBillingDetails.Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyLevelBillingDetailLst != null)
                    {
                        foreach (DLinq.PolicyLevelBillingDetail plbd in _PolicyLevelBillingDetailLst)
                        {
                            DataModel.DeleteObject(plbd);
                        }
                    }

                    //PolicyLearnedFieldsHistory
                    List<DLinq.PolicyLearnedFieldsHistory> _PolicyLearnedFieldsHistoryLst = DataModel.PolicyLearnedFieldsHistories.Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyLearnedFieldsHistoryLst != null)
                    {
                        foreach (DLinq.PolicyLearnedFieldsHistory plfh in _PolicyLearnedFieldsHistoryLst)
                        {
                            DataModel.DeleteObject(plfh);
                        }
                    }

                    //PolicyLearnedField
                    List<DLinq.PolicyLearnedField> _PolicyLearnedFieldLst = DataModel.PolicyLearnedFields.Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyLearnedFieldLst != null)
                    {
                        foreach (DLinq.PolicyLearnedField plf in _PolicyLearnedFieldLst)
                        {
                            DataModel.DeleteObject(plf);
                        }
                    }

                    //PolicyIncomingAdvancedSchedule
                    List<DLinq.PolicyIncomingAdvancedSchedule> _PolicyIncomingAdvancedScheduleLst = DataModel.PolicyIncomingAdvancedSchedules.Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyIncomingAdvancedScheduleLst != null)
                    {
                        foreach (DLinq.PolicyIncomingAdvancedSchedule pias in _PolicyIncomingAdvancedScheduleLst)
                        {
                            DataModel.DeleteObject(pias);
                        }
                    }

                    //PolicyIncomingSchedule
                    List<DLinq.PolicyIncomingSchedule> _PolicyIncomingScheduleLst = DataModel.PolicyIncomingSchedules
                                                                                        .Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyIncomingScheduleLst != null)
                    {
                        foreach (DLinq.PolicyIncomingSchedule pis in _PolicyIncomingScheduleLst)
                        {
                            DataModel.DeleteObject(pis);
                        }
                    }

                    //LastPolicyViewed
                    List<DLinq.LastPolicyViewed> _LastPolicyViewedLst = DataModel.LastPolicyVieweds
                                                                        .Where(p => p.PolicyId == PolicyId).ToList();

                    if (_LastPolicyViewedLst != null)
                    {
                        foreach (DLinq.LastPolicyViewed lpv in _LastPolicyViewedLst)
                        {

                            DataModel.DeleteObject(lpv);
                        }
                    }

                    //FollowupIssue
                    List<DLinq.FollowupIssue> _FollowupIssueLst = DataModel.FollowupIssues
                                                                .Where(p => p.PolicyId == PolicyId).ToList();
                    if (_FollowupIssueLst != null)
                    {
                        foreach (DLinq.FollowupIssue fil in _FollowupIssueLst)
                        {
                            DataModel.DeleteObject(fil);
                        }
                    }

                    //PolicyPaymentEntry
                    List<DLinq.PolicyPaymentEntry> _PolicyPaymentEntryLst = DataModel.PolicyPaymentEntries.Where(p => p.PolicyId == PolicyId).ToList();
                    if (_PolicyPaymentEntryLst != null)
                    {
                        foreach (DLinq.PolicyPaymentEntry ppel in _PolicyPaymentEntryLst)
                        {
                            List<DLinq.PolicyOutgoingPayment> _PolicyOutgoingPaymentLst = DataModel.PolicyOutgoingPayments
                                                            .Where(p => p.PaymentEntryId == ppel.PaymentEntryId).ToList();
                            foreach (DLinq.PolicyOutgoingPayment popl in _PolicyOutgoingPaymentLst)
                            {
                                DataModel.DeleteObject(popl);
                            }

                        }
                        foreach (DLinq.PolicyPaymentEntry ppel in _PolicyPaymentEntryLst)
                        {
                            var payment_followup = DataModel.FollowupIssues.Where(pf => pf.IssueId == ppel.FollowUpVarIssueId).AsEnumerable();
                            foreach (var pay_follow in payment_followup)
                            {
                                DataModel.DeleteObject(pay_follow);
                            }
                            DataModel.DeleteObject(ppel);
                        }
                    }

                    List<DLinq.EntriesByDEU> _entryByDEU = DataModel.EntriesByDEUs.Where(p => p.PolicyID == PolicyId).ToList();
                    if (_entryByDEU != null)
                    {
                        foreach (DLinq.EntriesByDEU ebd in _entryByDEU)
                        {
                            DataModel.DeleteObject(ebd);
                        }
                    }

                    var _policyHis = (from p in DataModel.PoliciesHistories where (p.PolicyId == PolicyId) select p).FirstOrDefault();
                    if (_policyHis != null)
                    {
                        DataModel.DeleteObject(_policyHis);
                    }

                    var _policy = (from p in DataModel.Policies where (p.PolicyId == PolicyId) select p).FirstOrDefault();
                    DataModel.DeleteObject(_policy);
                    DataModel.SaveChanges();
                }
                ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeletePolicyCascadeFromDBById ended: " + PolicyId, true);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeletePolicyCascadeFromDBById exception: " + PolicyId + "      " + ex.Message, true);
            }
            
        }

        public static bool CheckForPolicyPaymentExists(Guid Policyid)
        {
            bool flag = false;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyId", Policyid);
            PolicyDetailsData _policylst = Policy.GetPolicyData(parameters).FirstOrDefault();           
            if (_policylst != null)
            {
                if (_policylst.policyPaymentEntries == null)
                {
                    _policylst.policyPaymentEntries = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(Policyid);
                }
                List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = _policylst.policyPaymentEntries;
                /*Acme - commented following after Kevin's email to check for payment entries only*/
                /*foreach (PolicyPaymentEntriesPost popy in _PolicyPaymentEntriesPost)
                {
                    List<PolicyOutgoingDistribution> _PolicyOutgoingDistributionLst = PolicyOutgoingDistribution.GetOutgoingPaymentByPoicyPaymentEntryId(popy.PaymentEntryID);
                    if (_PolicyOutgoingDistributionLst != null && _PolicyOutgoingDistributionLst.Count != 0)
                    {
                        flag = true;
                        return flag;
                    }
                }*/
                if (_PolicyPaymentEntriesPost != null && _PolicyPaymentEntriesPost.Count > 0)
                {
                    flag = true;
                    return flag;
                }
            }
           
            return flag;
        }

        #endregion

        #region Get Methods

        public static int? GetPolicyStatusID(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var policyStatusId = from pl in DataModel.Policies
                                     where (pl.IsDeleted == false) && (pl.PolicyId == PolicyId)
                                     select new { pl.PolicyStatusId }.PolicyStatusId;
                return (int?)policyStatusId.SingleOrDefault();
            }
        }

        public static bool FollowUpRunsRequired(Guid PolicyId)
        {
            bool flag = true;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DateTime? LastFollowUpRuns = DataModel.Policies.Where(p => p.PolicyId == PolicyId).FirstOrDefault().LastFollowUpRuns;
                int DaysCnt = Convert.ToInt32(SystemConstant.GetKeyValue(MasterSystemConst.NextFollowUpRunDaysCount.ToString()));
                if (LastFollowUpRuns.HasValue)
                {
                    DateTime finaldatetorun = LastFollowUpRuns.Value.AddDays(DaysCnt);
                    if (DateTime.Today > finaldatetorun)
                        flag = true;
                    else
                        flag = false;
                }
            }
            return flag;
        }

        public static bool IsTrackPaymentChecked(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var isTrackPaymentchecked = (from p in DataModel.Policies
                                             where p.PolicyId == PolicyId
                                             select new { p.IsTrackPayment }.IsTrackPayment).FirstOrDefault();
                return isTrackPaymentchecked;
            }

        }

        public DateTime ? GetFollowUpDate(Guid PolicyID)
        {
            DateTime? dtFollowupDate = null;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                dtFollowupDate = (from pl in DataModel.Policies.Where(p => p.PolicyId == PolicyID)
                                  select pl.LastFollowUpRuns).FirstOrDefault();

                return dtFollowupDate;

            }

        }

        public static List<PolicyDetailsData> GetPolicyDataForWindowService(Dictionary<string, object> Parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        {
            Expression<Func<DLinq.Policy, bool>> parametersFromHelperClass = HelperClass.getWhereClause<DLinq.Policy>(Parameters);
            if (ParameterExpression != null)
            {
                parametersFromHelperClass = parametersFromHelperClass.And(ParameterExpression);
            }
            return FillinAllDataForWindowService(parametersFromHelperClass, ParameterExpression);
        }

        private static List<PolicyDetailsData> FillinAllDataForWindowService(Expression<Func<DLinq.Policy, bool>> parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        {
            List<DLinq.Policy> policies;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                policies = (from pl in DataModel.Policies
                            .Where(parameters)
                            select pl).ToList();
                List<PolicyDetailsData> endList = new List<PolicyDetailsData>();

                if (policies == null)
                {
                    return endList;
                }
                try
                {
                    endList = (from pl in policies
                               select new PolicyDetailsData
                               {
                                   //PolicyId = pl.PolicyId == null ? Guid.Empty : pl.PolicyId,
                                   PolicyId = pl.PolicyId,
                                   PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber,
                                   PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
                                   PolicyStatusName = pl.MasterPolicyStatu.Name,
                                   PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType,
                                   PolicyLicenseeId = pl.Licensee.LicenseeId,
                                   Insured = pl.Insured == null ? string.Empty : pl.Insured,
                                   OriginalEffectiveDate = pl.OriginalEffectiveDate,
                                   TrackFromDate = pl.TrackFromDate,
                                   PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
                                   ModeAvgPremium = pl.MonthlyPremium,
                                   SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough,
                                   Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled,
                                   Eligible = pl.Eligible == null ? string.Empty : pl.Eligible,
                                   PolicyTerminationDate = pl.PolicyTerminationDate,
                                   TerminationReasonId = pl.TerminationReasonId,
                                   IsTrackMissingMonth = pl.IsTrackMissingMonth,
                                   IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                                   IsTrackPayment = pl.IsTrackPayment,
                                   IsDeleted = pl.IsDeleted,
                                   CarrierID = pl.Carrier == null ? Guid.Empty : pl.Carrier.CarrierId,
                                   CarrierName = pl.Carrier == null ? string.Empty : pl.Carrier.CarrierName,
                                   CoverageId = pl.Coverage == null ? Guid.Empty : pl.Coverage.CoverageId,
                                   CoverageName = pl.Coverage == null ? string.Empty : pl.Coverage.ProductName,
                                   ClientId = pl.Client == null ? Guid.Empty : pl.Client.ClientId,
                                   ClientName = pl.Client == null ? string.Empty : pl.Client.Name,
                                   ReplacedBy = pl.ReplacedBy,
                                   DuplicateFrom = pl.DuplicateFrom,
                                   IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
                                   IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
                                   PayorId = pl.Payor == null ? Guid.Empty : pl.Payor.PayorId,
                                   PayorName = pl.Payor == null ? string.Empty : pl.Payor.PayorName,
                                   PayorNickName = pl.Payor == null ? string.Empty : pl.Payor.NickName,
                                   SplitPercentage = pl.SplitPercentage,
                                   IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
                                   PolicyIncomingPayType = pl.MasterIncomingPaymentType.Name,
                                   CreatedOn = pl.CreatedOn,
                                   RowVersion = pl.RowVersion,
                                   CreatedBy = pl.CreatedBy.Value,//--always check it will never null                                   
                                   IsSavedPolicy = true,
                                   CompType = pl.PolicyLearnedField == null ? null : pl.PolicyLearnedField.CompTypeID == null ? null : pl.PolicyLearnedField.CompTypeID,
                                   CompSchuduleType = pl.PolicyLearnedField == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType,
                                   LastFollowUpRuns = pl.LastFollowUpRuns,

                               }).ToList();
                }
                catch
                {
                }

                endList = new List<PolicyDetailsData>(endList.Where(p => p.PolicyNumber != string.Empty)).ToList();
                endList = new List<PolicyDetailsData>(endList.Where(p => p.ClientId != Guid.Empty)).ToList();

               // endList.ForEach(p => p.PolicyPreviousData = FillPolicyDetailPreviousData(p));       
         
                return endList;
            }

        }

        //CReated by Acme - to return trackdate - no modification done in previous method "GetPolicyTrackDate"
        public static DateTime? GetPolicyTrackFromDate(Guid PolicyID)
        {
            try
            {
                ActionLogger.Logger.WriteImportLogDetail("GetPolicyTrackFromDate policyID" + PolicyID, true);
                using (SqlConnection con = new SqlConnection(DBConnection.GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("Select TrackFromDate from policies where policyID ='" + PolicyID + "'", con))
                    {
                        cmd.Parameters.AddWithValue("@PolicyId", PolicyID);
                        con.Open();

                        object obj = cmd.ExecuteScalar();
                        if (obj !=null )
                        {
                            DateTime dtTime = Convert.ToDateTime(obj);
                            ActionLogger.Logger.WriteImportLogDetail("GetPolicyTrackFromDate date found: " + dtTime, true);
                            return dtTime;
                        }
                        else
                        {
                            ActionLogger.Logger.WriteImportLogDetail("GetPolicyTrackFromDate date not found: ", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail("GetPolicyTrackFromDate exception: " + ex.Message, true);
            }
            return null;
        }

        public static DateTime? GetPolicyTrackDate(Guid PolicyID)
        {

            DateTime? dtTime = null;
            DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
            EntityConnection ec = (EntityConnection)ctx.Connection;
            SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
            string adoConnStr = sc.ConnectionString;

            using (SqlConnection con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("usp_GetPolicyTrackFromDate", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PolicyId", PolicyID);
                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    // Call Read before accessing data. 
                    while (reader.Read())
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["TrackFromDate"])))
                            {
                                dtTime = Convert.ToDateTime(reader["TrackFromDate"]);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return dtTime;
            
        }
       
        public static List<PolicyDetailsData> GetPolicyData(Dictionary<string, object> Parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        {
            try
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    ActionLogger.Logger.WriteImportLogDetail("GetPolicyData parameters :  " + Parameters.ElementAt(i).Key + ", value: " + Parameters.ElementAt(i).Value, true);
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail("GetPolicyData ex :  " +ex.Message, true);
            }

            Expression<Func<DLinq.Policy, bool>> parametersFromHelperClass = HelperClass.getWhereClause<DLinq.Policy>(Parameters);
            if (ParameterExpression != null)
            {
                parametersFromHelperClass = parametersFromHelperClass.And(ParameterExpression);
            }
            return FillinAllData(parametersFromHelperClass, ParameterExpression);
        }

        private static List<PolicyDetailsData> FillinAllData(Expression<Func<DLinq.Policy, bool>> parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        {           
            List<DLinq.Policy> policies;
            List<PolicyDetailsData> endList = new List<PolicyDetailsData>();
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                try
                {
                    DataModel.CommandTimeout = 600000000;

                    policies = (from pl in DataModel.Policies.Where(parameters) select pl).ToList();

                    if (policies == null)
                    {
                        return endList;
                    }
                   //// ActionLogger.Logger.WriteImportLogDetail("FillinAllData list policies  parameters :  " + parameters., true);
                   

                    endList = (from pl in policies

                               select new PolicyDetailsData
                               {
                                   PolicyId = pl.PolicyId,
                                   PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber,
                                   PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
                                   PolicyStatusName = pl.MasterPolicyStatu.Name,
                                   PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType,
                                   PolicyLicenseeId = pl.Licensee.LicenseeId,
                                   Insured = pl.Insured == null ? string.Empty : pl.Insured,
                                   OriginalEffectiveDate = pl.OriginalEffectiveDate,
                                   TrackFromDate = pl.TrackFromDate,
                                   PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
                                   ModeAvgPremium = pl.MonthlyPremium,
                                   SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough,
                                   Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled,
                                   Eligible = pl.Eligible == null ? string.Empty : pl.Eligible,
                                   PolicyTerminationDate = pl.PolicyTerminationDate,
                                   TerminationReasonId = pl.TerminationReasonId,
                                   IsTrackMissingMonth = pl.IsTrackMissingMonth,
                                   IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                                   IsTrackPayment = pl.IsTrackPayment,
                                   IsDeleted = pl.IsDeleted,
                                   CarrierID = pl.Carrier == null ? Guid.Empty : pl.Carrier.CarrierId,
                                   CarrierName = pl.Carrier == null ? string.Empty : pl.Carrier.CarrierName,
                                   CoverageId = pl.Coverage == null ? Guid.Empty : pl.Coverage.CoverageId,
                                   CoverageName = pl.Coverage == null ? string.Empty : pl.Coverage.ProductName,

                                   ClientId = pl.Client == null ? Guid.Empty : pl.Client.ClientId,
                                   ClientName = pl.Client == null ? string.Empty : pl.Client.Name,

                                   ReplacedBy = pl.ReplacedBy,
                                   DuplicateFrom = pl.DuplicateFrom,
                                   IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
                                   IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
                                   PayorId = pl.Payor == null ? Guid.Empty : pl.Payor.PayorId,
                                   PayorName = pl.Payor == null ? string.Empty : pl.Payor.PayorName,
                                   PayorNickName = pl.Payor == null ? string.Empty : pl.Payor.NickName,
                                   SplitPercentage = pl.SplitPercentage == null ? 0.0 : pl.SplitPercentage,
                                   IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
                                   PolicyIncomingPayType = pl.MasterIncomingPaymentType == null ? string.Empty : pl.MasterIncomingPaymentType.Name,
                                   CreatedOn = pl.CreatedOn,
                                   RowVersion = pl.RowVersion,
                                   CreatedBy = pl.CreatedBy.Value,
                                   IsSavedPolicy = true,
                                   CompType = pl.PolicyLearnedField == null ? null : pl.PolicyLearnedField.CompTypeID == null ? null : pl.PolicyLearnedField.CompTypeID,
                                   CompSchuduleType = pl.PolicyLearnedField == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType,
                                   LastFollowUpRuns = pl.LastFollowUpRuns,
                                   Advance = pl.Advance == null ? null : pl.Advance,
                                   ProductType = pl.ProductType,
                                   //added
                                   AccoutExec = pl.AccoutExec,
                                   IsCustomBasicSchedule = pl.IsCustomBasicSchedule,
                                   CustomDateType = pl.CustomScheduleDateType

                               }).ToList();


                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteImportLogDetail("FillinAllData " + ex.StackTrace.ToString(), true);
                    ActionLogger.Logger.WriteImportLogDetail("FillinAllData " + ex.InnerException.ToString(), true);
                }
                //endList.ForEach(p => p.PolicyPreviousData = FillPolicyDetailPreviousData(p));

                endList = new List<PolicyDetailsData>(endList.Where(p => p.ClientId != Guid.Empty)).ToList();
              //  endList = new List<PolicyDetailsData>(endList.Where(p => p.PolicyNumber != string.Empty)).ToList(); Acme commented as data should be returned for blank policy numbers too.
               /* if (endList != null)
                {
                    try
                    {
                       // ActionLogger.Logger.WriteImportLogDetail("FillinAllData list count:  " + endList.Count, true);
                        foreach (PolicyDetailsData p in endList)
                        {
                          //  ActionLogger.Logger.WriteImportLogDetail("FillinAllData list count:  policy: " + p.PolicyId + ", client: " + p.ClientId + ", Name: " + p.ClientName, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        ActionLogger.Logger.WriteImportLogDetail("FillinAllData logging exception:  " + ex.Message, true);
                    }
                }*/
                return endList.OrderBy(p => p.PolicyNumber).ToList();
            }
        }

        public static PolicyDetailsData GetPolicyDetailsOnPolicyID(Guid policyID)
        {
            PolicyDetailsData policies = null;
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                try
                {
                    DataModel.CommandTimeout = 600000000;

                    policies = (from pl in DataModel.Policies
                                                  where pl.PolicyId == policyID
                                                  select new PolicyDetailsData
                                                  {
                                                      PolicyId = pl.PolicyId,
                                                      PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber,
                                                      PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
                                                      PolicyStatusName = pl.MasterPolicyStatu.Name,
                                                      PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType,
                                                      PolicyLicenseeId = pl.Licensee.LicenseeId,
                                                      Insured = pl.Insured == null ? string.Empty : pl.Insured,
                                                      OriginalEffectiveDate = pl.OriginalEffectiveDate,
                                                      TrackFromDate = pl.TrackFromDate,
                                                      PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
                                                      ModeAvgPremium = pl.MonthlyPremium,
                                                      SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough,
                                                      Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled,
                                                      Eligible = pl.Eligible == null ? string.Empty : pl.Eligible,
                                                      PolicyTerminationDate = pl.PolicyTerminationDate,
                                                      TerminationReasonId = pl.TerminationReasonId,
                                                      IsTrackMissingMonth = pl.IsTrackMissingMonth,
                                                      IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                                                      IsTrackPayment = pl.IsTrackPayment,
                                                      IsDeleted = pl.IsDeleted,
                                                      CarrierID = pl.Carrier == null ? Guid.Empty : pl.Carrier.CarrierId,
                                                      CarrierName = pl.Carrier == null ? string.Empty : pl.Carrier.CarrierName,
                                                      CoverageId = pl.Coverage == null ? Guid.Empty : pl.Coverage.CoverageId,
                                                      CoverageName = pl.Coverage == null ? string.Empty : pl.Coverage.ProductName,

                                                      ClientId = pl.Client == null ? Guid.Empty : pl.Client.ClientId,
                                                      ClientName = pl.Client == null ? string.Empty : pl.Client.Name,

                                                      ReplacedBy = pl.ReplacedBy,
                                                      DuplicateFrom = pl.DuplicateFrom,
                                                      IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
                                                      IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
                                                      PayorId = pl.Payor == null ? Guid.Empty : pl.Payor.PayorId,
                                                      PayorName = pl.Payor == null ? string.Empty : pl.Payor.PayorName,
                                                      PayorNickName = pl.Payor == null ? string.Empty : pl.Payor.NickName,
                                                      SplitPercentage = pl.SplitPercentage == null ? 0.0 : pl.SplitPercentage,
                                                      IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
                                                      PolicyIncomingPayType = pl.MasterIncomingPaymentType == null ? string.Empty : pl.MasterIncomingPaymentType.Name,
                                                      CreatedOn = pl.CreatedOn,
                                                      RowVersion = pl.RowVersion,
                                                      CreatedBy = pl.CreatedBy.Value,
                                                      IsSavedPolicy = true,
                                                      CompType = pl.PolicyLearnedField == null ? null : pl.PolicyLearnedField.CompTypeID == null ? null : pl.PolicyLearnedField.CompTypeID,
                                                      CompSchuduleType = pl.PolicyLearnedField == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType,
                                                      LastFollowUpRuns = pl.LastFollowUpRuns,
                                                      Advance = pl.Advance == null ? null : pl.Advance,
                                                      ProductType = pl.ProductType,
                                                      //added
                                                      AccoutExec = pl.AccoutExec

                                                  }).FirstOrDefault();

                }
                catch (Exception ex)
                {
                    ActionLogger.Logger.WriteImportLogDetail("GetPolicyDetailsOnPolicyID " + ex.StackTrace.ToString(), true);
                    ActionLogger.Logger.WriteImportLogDetail("GetPolicyDetailsOnPolicyID " + ex.InnerException.ToString(), true);
                }
                return policies;
            }
        }

        public static List<PolicyDetailsData> GetPolicyClientWise(Guid LicenseeId, Guid ClientId)
        {
            List<PolicyDetailsData> lstPolicyDetailsData = new List<PolicyDetailsData>();

            DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
            EntityConnection ec = (EntityConnection)ctx.Connection;
            SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
            string adoConnStr = sc.ConnectionString;
            
            DateTime? nullDateTime = null;
            int? nullint = null;
            bool? nullBool = null;
            Guid? nullGuid = null;           
            decimal? Nulldecimal = null;

            using (SqlConnection con = new SqlConnection(adoConnStr))
            {
                using (SqlCommand cmd = new SqlCommand("Usp_GetPolicyClientWise", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LicenseeId", LicenseeId);
                    cmd.Parameters.AddWithValue("@PolicyClientId", ClientId);
                    con.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    // Call Read before accessing data. 
                    while (reader.Read())
                    {
                        try
                        {
                            PolicyDetailsData objPolicyDetailsData = new PolicyDetailsData();

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyId"])))
                                {
                                    objPolicyDetailsData.PolicyId = reader["PolicyId"] == null ? Guid.Empty : (Guid)reader["PolicyId"];
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyNumber"])))
                                {
                                    objPolicyDetailsData.PolicyNumber = reader["PolicyNumber"] == null ? string.Empty : Convert.ToString(reader["PolicyNumber"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyStatusId"])))
                                {
                                    objPolicyDetailsData.PolicyStatusId = reader["PolicyStatusId"] == null ? nullint : Convert.ToInt32(reader["PolicyStatusId"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyStatusName"])))
                                {
                                    objPolicyDetailsData.PolicyStatusName = reader["PolicyStatusName"] == null ? string.Empty : Convert.ToString(reader["PolicyStatusName"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyType"])))
                                {
                                    objPolicyDetailsData.PolicyType = reader["PolicyType"] == null ? string.Empty : Convert.ToString(reader["PolicyType"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["LicenseeId"])))
                                {
                                    objPolicyDetailsData.PolicyLicenseeId = reader["LicenseeId"] == null ? nullGuid : (Guid)reader["LicenseeId"];
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["Insured"])))
                                {
                                    objPolicyDetailsData.Insured = reader["Insured"] == null ? string.Empty : Convert.ToString(reader["Insured"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["OriginalEffectiveDate"])))
                                {
                                    objPolicyDetailsData.OriginalEffectiveDate = reader["OriginalEffectiveDate"] == null ? nullDateTime : Convert.ToDateTime(reader["OriginalEffectiveDate"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["TrackFromDate"])))
                                {
                                    objPolicyDetailsData.TrackFromDate = reader["TrackFromDate"] == null ? nullDateTime : Convert.ToDateTime(reader["TrackFromDate"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyModeId"])))
                                {
                                    objPolicyDetailsData.PolicyModeId = reader["PolicyModeId"] == null ? nullint : Convert.ToInt32(reader["PolicyModeId"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["MonthlyPremium"])))
                                {
                                    objPolicyDetailsData.ModeAvgPremium = reader["MonthlyPremium"] == null ? Nulldecimal : Convert.ToDecimal(reader["MonthlyPremium"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["SubmittedThrough"])))
                                {
                                    objPolicyDetailsData.SubmittedThrough = reader["SubmittedThrough"] == null ? string.Empty : Convert.ToString(reader["SubmittedThrough"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["Enrolled"])))
                                {
                                    objPolicyDetailsData.Enrolled = reader["Enrolled"] == null ? string.Empty : Convert.ToString(reader["Enrolled"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["Eligible"])))
                                {
                                    objPolicyDetailsData.Eligible = reader["Eligible"] == null ? string.Empty : Convert.ToString(reader["Eligible"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PolicyTerminationDate"])))
                                {
                                    objPolicyDetailsData.PolicyTerminationDate = reader["PolicyTerminationDate"] == null ? nullDateTime : Convert.ToDateTime(reader["PolicyTerminationDate"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["TerminationReasonId"])))
                                {
                                    objPolicyDetailsData.TerminationReasonId = reader["TerminationReasonId"] == null ? nullint : Convert.ToInt32(reader["TerminationReasonId"]);
                                }

                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackMissingMonth"])))
                                {
                                    if (reader["IsTrackMissingMonth"] != null)
                                    {
                                        objPolicyDetailsData.IsTrackMissingMonth = Convert.ToBoolean(reader["IsTrackMissingMonth"]);
                                    }
                                }

                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackIncomingPercentage"])))
                                {
                                    if (reader["IsTrackIncomingPercentage"] != null)
                                    {
                                        objPolicyDetailsData.IsTrackIncomingPercentage = Convert.ToBoolean(reader["IsTrackIncomingPercentage"]);
                                    }
                                }

                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["IsTrackPayment"])))
                                {
                                    if (reader["IsTrackPayment"] != null)
                                    {
                                        objPolicyDetailsData.IsTrackPayment = Convert.ToBoolean(reader["IsTrackPayment"]);
                                    }
                                }

                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["IsDeleted"])))
                                {
                                    if (reader["IsDeleted"] != null)
                                    {
                                        objPolicyDetailsData.IsDeleted = Convert.ToBoolean(reader["IsDeleted"]);
                                    }
                                }

                            }
                            catch
                            {
                            }
                            try
                            {

                                if (!string.IsNullOrEmpty(Convert.ToString(reader["CarrierID"])))
                                {
                                    objPolicyDetailsData.CarrierID = reader["CarrierID"] == null ? nullGuid : (Guid)(reader["CarrierID"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {

                                if (!string.IsNullOrEmpty(Convert.ToString(reader["CarrierName"])))
                                {
                                    objPolicyDetailsData.CarrierName = reader["CarrierName"] == null ? string.Empty : Convert.ToString(reader["CarrierName"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["CoverageId"])))
                                {
                                    objPolicyDetailsData.CoverageId = reader["CoverageId"] == null ? nullGuid : (Guid)(reader["CoverageId"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["ProductName"])))
                                {
                                    objPolicyDetailsData.CoverageName = reader["ProductName"] == null ? string.Empty : Convert.ToString(reader["ProductName"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["ClientId"])))
                                {
                                    objPolicyDetailsData.ClientId = reader["ClientId"] == null ? nullGuid : (Guid)(reader["ClientId"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["ClientsName"])))
                                {
                                    objPolicyDetailsData.ClientName = reader["ClientsName"] == null ? string.Empty : Convert.ToString(reader["ClientsName"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["ReplacedBy"])))
                                {
                                    objPolicyDetailsData.ReplacedBy = reader["ReplacedBy"] == null ? nullGuid : (Guid)(reader["ReplacedBy"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["DuplicateFrom"])))
                                {
                                    objPolicyDetailsData.DuplicateFrom = reader["DuplicateFrom"] == null ? nullGuid : (Guid)(reader["DuplicateFrom"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["IsIncomingBasicSchedule"])))
                                {
                                    objPolicyDetailsData.IsIncomingBasicSchedule = reader["IsIncomingBasicSchedule"] == null ? nullBool : Convert.ToBoolean(reader["IsIncomingBasicSchedule"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["IsOutGoingBasicSchedule"])))
                                {
                                    objPolicyDetailsData.IsOutGoingBasicSchedule = reader["IsOutGoingBasicSchedule"] == null ? nullBool : Convert.ToBoolean(reader["IsOutGoingBasicSchedule"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["CarrierID"])))
                                {
                                    objPolicyDetailsData.CarrierID = reader["CarrierID"] == null ? nullGuid : (Guid)(reader["CarrierID"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PayorID"])))
                                {
                                    objPolicyDetailsData.PayorId = reader["PayorID"] == null ? nullGuid : (Guid)(reader["PayorID"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["PayorName"])))
                                {
                                    objPolicyDetailsData.PayorName = reader["PayorName"] == null ? string.Empty : Convert.ToString(reader["PayorName"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["SplitPercentage"])))
                                {
                                    objPolicyDetailsData.PayorNickName = reader["PayorNickName"] == null ? string.Empty : Convert.ToString(reader["PayorNickName"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["SplitPercentage"])))
                                {
                                    objPolicyDetailsData.SplitPercentage = reader["SplitPercentage"] == null ? 0 : Convert.ToDouble(reader["SplitPercentage"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["IncomingPaymentTypeId"])))
                                {
                                    objPolicyDetailsData.IncomingPaymentTypeId = reader["IncomingPaymentTypeId"] == null ? nullint : Convert.ToInt32(reader["IncomingPaymentTypeId"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["Name"])))
                                {
                                    objPolicyDetailsData.PolicyIncomingPayType = reader["Name"] == null ? string.Empty : Convert.ToString(reader["Name"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["CreatedOn"])))
                                {
                                    objPolicyDetailsData.CreatedOn = reader["CreatedOn"] == null ? nullDateTime : Convert.ToDateTime(reader["CreatedOn"]);
                                }

                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["RowVersion"])))
                                {
                                    objPolicyDetailsData.RowVersion = (Byte[])(reader["RowVersion"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["CreatedBy"])))
                                {
                                    objPolicyDetailsData.CreatedBy = (Guid)(reader["CreatedBy"]);
                                }
                            }
                            catch
                            {
                            }

                            objPolicyDetailsData.IsSavedPolicy = true;

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["CompTypeID"])))
                                {
                                    objPolicyDetailsData.CompSchuduleType = reader["CompTypeID"] == null ? string.Empty : Convert.ToString(reader["CompTypeID"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["CompScheduleType"])))
                                {
                                    objPolicyDetailsData.CompSchuduleType = reader["CompScheduleType"] == null ? string.Empty : Convert.ToString(reader["CompScheduleType"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["LastFollowUpRuns"])))
                                {
                                    objPolicyDetailsData.LastFollowUpRuns = reader["LastFollowUpRuns"] == null ? nullDateTime : Convert.ToDateTime(reader["LastFollowUpRuns"]);
                                }
                            }
                            catch
                            {
                            }
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["Advance"])))
                                {
                                    objPolicyDetailsData.Advance = reader["Advance"] == null ? nullint : Convert.ToInt32(reader["Advance"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["AccoutExec"])))
                                {
                                    objPolicyDetailsData.AccoutExec = reader["AccoutExec"] == null ? null : Convert.ToString(reader["AccoutExec"]);
                                }
                            }
                            catch
                            {
                            }

                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(reader["UserCredentialId"])))
                                {
                                    objPolicyDetailsData.UserCredentialId = reader["UserCredentialId"] == null ? Guid.Empty : (Guid)(reader["UserCredentialId"]);
                                }
                            }
                            catch
                            {
                            }

                            //Acme - added with new split on dates feature
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["PrimaryAgent"])))
                            {
                                objPolicyDetailsData.PrimaryAgent = reader["PrimaryAgent"] == null ? Guid.Empty : (Guid)reader["PrimaryAgent"];
                            }
                            objPolicyDetailsData.IsCustomBasicSchedule = false;
                            if (!string.IsNullOrEmpty(Convert.ToString(reader["IsCustomBasicSchedule"])))
                            {
                                string strIsCustom = Convert.ToString(reader["IsCustomBasicSchedule"]);
                                bool isCustom = false;
                                bool.TryParse(strIsCustom, out isCustom);
                                objPolicyDetailsData.IsCustomBasicSchedule = isCustom;

                                if (isCustom)
                                    objPolicyDetailsData.CustomDateType = Convert.ToString(reader["CustomScheduleDateType"]);

                            }


                            lstPolicyDetailsData.Add(objPolicyDetailsData);
                        }
                        catch(Exception ex)
                        {
                            ActionLogger.Logger.WriteImportLogDetail("Exception getting policy: " + ex.Message, true);
                        }

                    }

                    // Call Close when done reading.
                    reader.Close();
                }
            }

            return lstPolicyDetailsData;
        }

        //private static List<PolicyDetailsData> FillinAllData(Expression<Func<DLinq.Policy, bool>> parameters, Expression<Func<DLinq.Policy, bool>> ParameterExpression = null)
        //{
        //    //ActionLogger.Logger.WriteImportLogDetail("Get policy started", true);
        //    List<DLinq.Policy> policies;
        //    List<PolicyDetailsData> endList = new List<PolicyDetailsData>();
        //    using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
        //    {
        //        try
        //        {
        //            if (ParameterExpression == null)
        //            {
        //                string temp = null;
        //                string par = parameters.ToString();
        //                par = par.Replace("AndAlso", "And");
        //                string[] Splitstring = Regex.Split(par, "And");


        //                for (int i = 1; i < Splitstring.Count(); i++)
        //                {
        //                    temp = temp + "And" + Splitstring[i];
        //                }
        //                string re = parameters.ToString();

        //                var result = DataModel.FillPolicy(temp);

        //                //ActionLogger.Logger.WriteImportLogDetail( "sp executed", true);

        //                if (result == null)
        //                {
        //                  //  ActionLogger.Logger.WriteImportLogDetail("data is null", true);
        //                    return endList;
        //                }

        //                endList = (from pl in result
        //                           select new PolicyDetailsData
        //                           {
        //                               PolicyId = Guid.Parse(pl.PolicyId.ToString()),
        //                               PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber,
        //                               PolicyStatusId = pl.PolicyStatusId,
        //                               PolicyStatusName = pl.PolicyStatusName,
        //                               PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType,
        //                               PolicyLicenseeId = pl.LicenseeId,
        //                               Insured = pl.Insured == null ? string.Empty : pl.Insured,
        //                               OriginalEffectiveDate = pl.OriginalEffectiveDate,
        //                               TrackFromDate = pl.TrackFromDate,
        //                               PolicyModeId = pl.PolicyModeId,
        //                               ModeAvgPremium = pl.MonthlyPremium,
        //                               SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough,
        //                               Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled,
        //                               Eligible = pl.Eligible == null ? string.Empty : pl.Eligible,
        //                               PolicyTerminationDate = pl.PolicyTerminationDate,
        //                               TerminationReasonId = pl.TerminationReasonId,
        //                               IsTrackMissingMonth = bool.Parse(pl.IsTrackMissingMonth.ToString()),
        //                               IsTrackIncomingPercentage = bool.Parse(pl.IsTrackIncomingPercentage.ToString()),
        //                               IsTrackPayment = bool.Parse(pl.IsTrackPayment.ToString()),
        //                               IsDeleted = bool.Parse(pl.IsDeleted.ToString()),
        //                               CarrierID = pl.CarrierID == null ? Guid.Empty : pl.CarrierID,
        //                               CarrierName = pl.CarrierName == null ? string.Empty : pl.CarrierName,
        //                               CoverageId = pl.CoverageId == null ? Guid.Empty : pl.CoverageId,
        //                               CoverageName = pl.ProductName == null ? string.Empty : pl.ProductName,
        //                               ClientId = pl.ClientId,
        //                               ClientName = pl.ClientsName,
        //                               ReplacedBy = pl.ReplacedBy,
        //                               DuplicateFrom = pl.DuplicateFrom,
        //                               IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
        //                               IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
        //                               PayorId = pl.PayorID == null ? Guid.Empty : pl.PayorID,
        //                               PayorName = pl.PayorName == null ? string.Empty : pl.PayorName,
        //                               PayorNickName = pl.PayorNickName == null ? string.Empty : pl.PayorNickName,
        //                               SplitPercentage = pl.SplitPercentage,
        //                               IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
        //                               PolicyIncomingPayType = pl.Name,
        //                               CreatedOn = pl.CreatedOn,
        //                               RowVersion = pl.RowVersion,
        //                               CreatedBy = pl.CreatedBy.Value,//--always check it will never null                                   
        //                               IsSavedPolicy = true,
        //                               //CompType = pl.CompTypeID == null ? null : pl.CompTypeID,
        //                               CompSchuduleType = pl.CompScheduleType == null ? string.Empty : pl.CompScheduleType,  
        //                               LastFollowUpRuns=pl.LastFollowUpRuns,
        //                               Advance = pl.Advance == null ? null : pl.Advance,  
                                       

        //                           }).ToList();

        //            }

        //            else
        //            {
        //                policies = (from pl in DataModel.Policies
        //                            .Where(parameters)
        //                            select pl).ToList();

        //                if (policies == null)
        //                {
        //                    return endList;
        //                }
        //                endList = (from pl in policies
        //                           select new PolicyDetailsData
        //                           {
        //                               PolicyId = pl.PolicyId == null ? Guid.Empty : pl.PolicyId,
        //                               PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber,
        //                               PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
        //                               PolicyStatusName = pl.MasterPolicyStatu.Name,
        //                               PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType,
        //                               PolicyLicenseeId = pl.Licensee.LicenseeId,
        //                               Insured = pl.Insured == null ? string.Empty : pl.Insured,
        //                               OriginalEffectiveDate = pl.OriginalEffectiveDate,
        //                               TrackFromDate = pl.TrackFromDate,
        //                               PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
        //                               ModeAvgPremium = pl.MonthlyPremium,
        //                               SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough,
        //                               Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled,
        //                               Eligible = pl.Eligible == null ? string.Empty : pl.Eligible,
        //                               PolicyTerminationDate = pl.PolicyTerminationDate,
        //                               TerminationReasonId = pl.TerminationReasonId,
        //                               IsTrackMissingMonth = pl.IsTrackMissingMonth,
        //                               IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
        //                               IsTrackPayment = pl.IsTrackPayment,
        //                               IsDeleted = pl.IsDeleted,
        //                               CarrierID = pl.Carrier == null ? Guid.Empty : pl.Carrier.CarrierId,
        //                               CarrierName = pl.Carrier == null ? string.Empty : pl.Carrier.CarrierName,
        //                               CoverageId = pl.Coverage == null ? Guid.Empty : pl.Coverage.CoverageId,
        //                               CoverageName = pl.Coverage == null ? string.Empty : pl.Coverage.ProductName,
        //                               ClientId = pl.Client.ClientId,
        //                               ClientName = pl.Client.Name,
        //                               ReplacedBy = pl.ReplacedBy,
        //                               DuplicateFrom = pl.DuplicateFrom,
        //                               IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
        //                               IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
        //                               PayorId = pl.Payor == null ? Guid.Empty : pl.Payor.PayorId,
        //                               PayorName = pl.Payor == null ? string.Empty : pl.Payor.PayorName,
        //                               PayorNickName = pl.Payor == null ? string.Empty : pl.Payor.NickName,
        //                               SplitPercentage = pl.SplitPercentage,
        //                               IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
        //                               PolicyIncomingPayType = pl.MasterIncomingPaymentType.Name,
        //                               CreatedOn = pl.CreatedOn,
        //                               RowVersion = pl.RowVersion,
        //                               CreatedBy = pl.CreatedBy.Value,//--always check it will never null                                   
        //                               IsSavedPolicy = true,
        //                               CompType = pl.PolicyLearnedField == null ? null : pl.PolicyLearnedField.CompTypeID == null ? null : pl.PolicyLearnedField.CompTypeID,
        //                               CompSchuduleType = pl.PolicyLearnedField == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType,
        //                               LastFollowUpRuns=pl.LastFollowUpRuns,
        //                               Advance = pl.Advance == null ? null : pl.Advance,                                      


        //                           }).ToList();
        //            }

        //        }
        //        catch(Exception ex)
        //        {
        //            //ActionLogger.Logger.WriteImportLogDetail(ex.ToString(), true);
        //        }
        //        //if (policies.FirstOrDefault().PolicyLearnedField != null)
        //        //{
        //        //    //MapLearnedFields(endList, policies);
        //        //}
        //        endList.ForEach(p => p.PolicyPreviousData = FillPolicyDetailPreviousData(p));
              
        //        //if (policies.FirstOrDefault().PolicyPaymentEntries != null)
        //        //{
        //        //    //endList.ForEach(policy => MapPolicyIncomingPayment((policies.Where(p => p.PolicyId == policy.PolicyId).FirstOrDefault().PolicyPaymentEntries).ToList(), policy));
        //        //}

        //       // ActionLogger.Logger.WriteImportLogDetail("end policy function", true);
        //       // ActionLogger.Logger.WriteImportLogDetail(endList.Count.ToString(), true);

        //        return endList.OrderBy(p=>p.PolicyNumber).ToList();
                
        //        #region for Comment sp

        //        //policies = (from pl in DataModel.Policies
        //        //            .Where(parameters)
        //        //            select pl).ToList();
        //        //List<PolicyDetailsData> endList = new List<PolicyDetailsData>();

        //        //if (policies == null)
        //        //{
        //        //    return endList;
        //        //}
        //        //try
        //        //{
        //        //    endList = (from pl in policies
        //        //               select new PolicyDetailsData
        //        //               {
        //        //                   PolicyId = pl.PolicyId == null ? Guid.Empty : pl.PolicyId,
        //        //                   PolicyNumber = pl.PolicyNumber == null ? string.Empty : pl.PolicyNumber,
        //        //                   PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
        //        //                   PolicyStatusName = pl.MasterPolicyStatu.Name,
        //        //                   PolicyType = pl.PolicyType == null ? string.Empty : pl.PolicyType,
        //        //                   PolicyLicenseeId = pl.Licensee.LicenseeId,
        //        //                   Insured = pl.Insured == null ? string.Empty : pl.Insured,
        //        //                   OriginalEffectiveDate = pl.OriginalEffectiveDate,
        //        //                   TrackFromDate = pl.TrackFromDate,
        //        //                   PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
        //        //                   ModeAvgPremium = pl.MonthlyPremium,
        //        //                   SubmittedThrough = pl.SubmittedThrough == null ? string.Empty : pl.SubmittedThrough,
        //        //                   Enrolled = pl.Enrolled == null ? string.Empty : pl.Enrolled,
        //        //                   Eligible = pl.Eligible == null ? string.Empty : pl.Eligible,
        //        //                   PolicyTerminationDate = pl.PolicyTerminationDate,
        //        //                   TerminationReasonId = pl.TerminationReasonId,
        //        //                   IsTrackMissingMonth = pl.IsTrackMissingMonth,
        //        //                   IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
        //        //                   IsTrackPayment = pl.IsTrackPayment,
        //        //                   IsDeleted = pl.IsDeleted,
        //        //                   CarrierID = pl.Carrier == null ? Guid.Empty : pl.Carrier.CarrierId,
        //        //                   CarrierName = pl.Carrier == null ? string.Empty : pl.Carrier.CarrierName,
        //        //                   CoverageId = pl.Coverage == null ? Guid.Empty : pl.Coverage.CoverageId,
        //        //                   CoverageName = pl.Coverage == null ? string.Empty : pl.Coverage.ProductName,
        //        //                   ClientId = pl.Client.ClientId,
        //        //                   ClientName = pl.Client.Name,
        //        //                   ReplacedBy = pl.ReplacedBy,
        //        //                   DuplicateFrom = pl.DuplicateFrom,
        //        //                   IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
        //        //                   IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
        //        //                   PayorId = pl.Payor == null ? Guid.Empty : pl.Payor.PayorId,
        //        //                   PayorName = pl.Payor == null ? string.Empty : pl.Payor.PayorName,
        //        //                   PayorNickName = pl.Payor == null ? string.Empty : pl.Payor.NickName,
        //        //                   SplitPercentage = pl.SplitPercentage,
        //        //                   IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
        //        //                   PolicyIncomingPayType=pl.MasterIncomingPaymentType.Name,
        //        //                   CreatedOn = pl.CreatedOn,
        //        //                   RowVersion = pl.RowVersion,
        //        //                   CreatedBy = pl.CreatedBy.Value,//--always check it will never null                                   
        //        //                   IsSavedPolicy = true,
        //        //                   CompType = pl.PolicyLearnedField == null ? null : pl.PolicyLearnedField.CompTypeID == null ? null : pl.PolicyLearnedField.CompTypeID,
        //        //                   CompSchuduleType = pl.PolicyLearnedField == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType == null ? string.Empty : pl.PolicyLearnedField.CompScheduleType,


        //        //               }).ToList();
        //        //}
        //        //catch
        //        //{
        //        //}
        //        ////if (policies.FirstOrDefault().PolicyLearnedField != null)
        //        ////{
        //        ////    //MapLearnedFields(endList, policies);
        //        ////}
        //        //endList.ForEach(p => p.PolicyPreviousData = FillPolicyDetailPreviousData(p));
        //        ////if (policies.FirstOrDefault().PolicyPaymentEntries != null)
        //        ////{
        //        ////    //endList.ForEach(policy => MapPolicyIncomingPayment((policies.Where(p => p.PolicyId == policy.PolicyId).FirstOrDefault().PolicyPaymentEntries).ToList(), policy));
        //        ////}
        //        //return endList;

        //        #endregion
        //    }

        //}
        
        #endregion

        public PolicyDetailsData GetPolicyStting(Guid policyID)
        {

            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                PolicyDetailsData _Policy = (from pl in DataModel.Policies
                                             where (pl.IsDeleted == false) && (pl.PolicyId == policyID)
                                             select new PolicyDetailsData
                                          {
                                              PolicyId = pl.PolicyId,                                              
                                              IsTrackMissingMonth = pl.IsTrackMissingMonth,
                                              IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                                              PolicyNumber = pl.PolicyNumber,
                                          }).FirstOrDefault();

                return _Policy;
            }
        }
    
        #region Loading Releations for Policy
        private static void MapPolicyIncomingPayment(List<DLinq.PolicyPaymentEntry> Source, PolicyDetailsData Target)
        {
            List<PolicyPaymentEntriesPost> payments = (from u in Source
                                                       where u.PolicyId == Target.PolicyId
                                                       select new PolicyPaymentEntriesPost
                                                                    {
                                                                        PaymentEntryID = u.PaymentEntryId,
                                                                        StmtID = u.StatementId.Value,
                                                                        PolicyID = u.PolicyId.Value,
                                                                        // IssueID = u.IssueID.Value,
                                                                        InvoiceDate = u.InvoiceDate,
                                                                        PaymentRecived = u.PaymentRecived ?? 0,
                                                                        CommissionPercentage = u.CommissionPercentage ?? 0,
                                                                        NumberOfUnits = u.NumberOfUnits ?? 0,
                                                                        DollerPerUnit = u.DollerPerUnit ?? 0,
                                                                        Fee = u.Fee.Value,
                                                                        SplitPer = u.SplitPercentage ?? 0,
                                                                        TotalPayment = u.TotalPayment ?? 0,
                                                                        CreatedOn = u.CreatedOn.Value,
                                                                        CreatedBy = u.CreatedBy ?? Guid.Empty,
                                                                        PostStatusID = u.PostStatusID,
                                                                        DEUEntryId = u.DEUEntryId ?? Guid.Empty,
                                                                        StmtNumber = u.Statement.StatementNumber,
                                                                        FollowUpVarIssueId = u.FollowUpVarIssueId,
                                                                    }).ToList();

            Target.policyPaymentEntries = payments;
        }

        private static void MapPolicyOutGoingPayments(List<DLinq.PolicyOutgoingPayment> Source, PolicyPaymentEntriesPost Target)
        {
            List<PolicyOutgoingDistribution> policyIncomingSchedule = (from u in Source
                                                                       where u.PaymentEntryId == Target.PaymentEntryID
                                                                       select new PolicyOutgoingDistribution
                                                                       {
                                                                           PaymentEntryId = u.PaymentEntryId,
                                                                           CreatedOn = u.CreatedOn,
                                                                           IsPaid = u.IsPaid,
                                                                           OutgoingPaymentId = u.OutgoingPaymentId,
                                                                           OutGoingPerUnit = u.OutgoingPerUnit,
                                                                           RecipientUserCredentialId = u.RecipientUserCredentialId,
                                                                           PaidAmount = u.PaidAmount,
                                                                           Payment = u.Payment,
                                                                           Premium = u.Premium
                                                                       }).ToList();

        }

        private static void MapLearnedFields(List<PolicyDetailsData> policies, List<DLinq.Policy> Source)
        {
            var policyLearnedFields = (from plearned in Source
                                       select new PolicyLearnedFieldData
                                       {
                                           PolicyId = plearned.PolicyLearnedField.PolicyId,
                                           Insured = plearned.PolicyLearnedField.Insured,
                                           PolicyNumber = plearned.PolicyLearnedField.PolicyNumber,
                                           Effective = plearned.PolicyLearnedField.Effective,
                                           TrackFrom = plearned.PolicyLearnedField.TrackFrom,
                                           Renewal = plearned.PolicyLearnedField.Renewal,
                                           CarrierId = plearned.PolicyLearnedField.CarrierId,
                                           CoverageId = plearned.PolicyLearnedField.CoverageId,
                                           PAC =plearned.PolicyLearnedField.PAC,
                                           PMC =plearned.PolicyLearnedField.PMC,
                                           ModalAvgPremium = plearned.PolicyLearnedField.ModalAvgPremium,
                                           PolicyModeId = plearned.PolicyLearnedField.PolicyModeId,
                                           Enrolled = plearned.PolicyLearnedField.Enrolled,
                                           Eligible = plearned.PolicyLearnedField.Eligible,
                                           AutoTerminationDate = plearned.PolicyLearnedField.AutoTerminationDate,
                                           Link1 = plearned.PolicyLearnedField.Link1,
                                           PayorSysId = plearned.PolicyLearnedField.PayorSysID,
                                           LastModifiedOn = plearned.PolicyLearnedField.LastModifiedOn,
                                           LastModifiedUserCredentialId = plearned.PolicyLearnedField.LastModifiedUserCredentialid,
                                           CompTypeId = plearned.PolicyLearnedField.CompTypeID,
                                           CompScheduleType = plearned.PolicyLearnedField.CompScheduleType,
                                           PayorId = plearned.PolicyLearnedField.PayorId,
                                           PreviousEffectiveDate = plearned.PolicyLearnedField.PreviousEffectiveDate,
                                           PreviousPolicyModeid = plearned.PolicyLearnedField.PreviousPolicyModeId, 
                                       }).ToList();
            policyLearnedFields.ForEach(plf => plf.CoverageNickName = Coverage.GetCoverageNickName(plf.PayorId ?? Guid.Empty, plf.CarrierId ?? Guid.Empty, plf.CoverageId ?? Guid.Empty));
            policyLearnedFields.ForEach(plf => plf.CarrierNickName = Carrier.GetCarrierNickName(plf.PayorId ?? Guid.Empty, plf.CarrierId ?? Guid.Empty));
            policies.ForEach(p => p.LearnedFields = policyLearnedFields.Where(pl => pl.PolicyId == p.PolicyId).FirstOrDefault());
        }
        #endregion

        public static decimal GetPMC(Guid policyID)
        {
            return PostUtill.CalculatePMC(policyID);
        }

        public static decimal GetPAC(Guid policyID)
        {
            return PostUtill.CalculatePAC(policyID);
        }
        
        #region Batch and Others
        public static Batch GenerateBatch(PolicyDetailsData _policy)
        {
            bool IsBatchPaid = false;
            Batch _batch = null;
            Batch objBatch = new Batch();
            //List<Batch> tempBatchlst = Batch.GetBatchList(UploadStatus.Automatic);
            List<Batch> tempBatchlst = objBatch.GetBatchList(UploadStatus.Automatic);

            tempBatchlst = tempBatchlst == null ? null : tempBatchlst
                   .Where(p => p.PayorId == _policy.PayorId).Where(p => p.LicenseeId == _policy.PolicyLicenseeId).ToList();

            foreach (Batch bth in tempBatchlst)
            {               
                IsBatchPaid = objBatch.GetBatchPaidStatus(bth.BatchId);
                if (!IsBatchPaid)
                {
                    _batch = bth;
                    break;
                }
            }

            if (_batch == null)
            {
                _batch = new Batch();
                _batch.BatchId = Guid.NewGuid();
                _batch.LicenseeId = _policy.PolicyLicenseeId.Value;
                _batch.PayorId = _policy.PayorId;
                _batch.CreatedDate = DateTime.Today;
                _batch.FileType = null;
                _batch.UploadStatus = UploadStatus.Automatic;
                _batch.EntryStatus = EntryStatus.BatchCompleted;
                _batch.AssignedDeuUserName = null;
                _batch.CreatedFromUpload = null;
                _batch.FileName = null;
                _batch.LastModifiedDate = DateTime.Today;
                _batch.IsManuallyUploaded = null;
                _batch.SiteId = null;

                _batch.AddUpdate();
            }
            else
            {
                _batch.LastModifiedDate = DateTime.Today;
                _batch.AddUpdate();
            }
            return _batch;
        }

        public static Statement GenerateStatment(Guid BatchId, Guid PayorId, decimal PaymentRecived, Guid CreatedBy)
        {
            Statement _Statement = null;
            //List<Statement> StmtLst = Statement.GetStatementList(BatchId);
            //Added statement object
            Statement objStatement = new Statement();
            List<Statement> StmtLst = objStatement.GetStatementList(BatchId);
            
            if (StmtLst != null)
                _Statement = StmtLst.Where(p => p.PayorId == PayorId).FirstOrDefault();

            if (_Statement == null)
            {
                _Statement = new Statement();
                _Statement.StatementID = Guid.NewGuid();
                _Statement.BatchId = BatchId;
                _Statement.PayorId = PayorId;
                _Statement.CheckAmount = PaymentRecived;
                _Statement.StatusId = 2;
                _Statement.Entries = 1;
                _Statement.CreatedDate = DateTime.Now;
                _Statement.LastModified = DateTime.Now;
                _Statement.CreatedBy = CreatedBy;
                _Statement.BalanceForOrAdjustment = null;
                _Statement.EnteredAmount = PaymentRecived;
                _Statement.StatementDate = null;
                _Statement.AddUpdate();
            }
            else
            {
                _Statement.StatusId = 1;
                _Statement.CheckAmount += PaymentRecived;
                _Statement.EnteredAmount += PaymentRecived;
                _Statement.Entries += 1;
                _Statement.LastModified = DateTime.Now;
                _Statement.AddUpdate();
            }
            return _Statement;
        }

        public static void AddUpdatePolicyHistory(Guid PolicyId)
        {
            List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(PolicyId);
            if (_PolicyPaymentEntriesPost.Count != 0) return;
          //  Dictionary<string, object> parameters = new Dictionary<string, object>();
           // parameters.Add("PolicyId", PolicyId);
            PolicyDetailsData _PolicyRecord = GetPolicyDetailsOnPolicyID(PolicyId);  //GetPolicyData(parameters).FirstOrDefault();
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _PolicyRecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null)
                {
                    _policy = new DLinq.PoliciesHistory
                    {
                        PolicyId = _PolicyRecord.PolicyId,
                        PolicyNumber = _PolicyRecord.PolicyNumber,
                        PolicyType = _PolicyRecord.PolicyType,
                        Insured = _PolicyRecord.Insured,
                        OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate,
                        TrackFromDate = _PolicyRecord.TrackFromDate,
                        MonthlyPremium = _PolicyRecord.ModeAvgPremium,
                        SubmittedThrough = _PolicyRecord.SubmittedThrough,
                        Enrolled = _PolicyRecord.Enrolled,
                        Eligible = _PolicyRecord.Eligible,
                        PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate,
                        IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth,
                        IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage,
                        IsTrackPayment = _PolicyRecord.IsTrackPayment,
                        IsDeleted = false,
                        ReplacedBy = _PolicyRecord.ReplacedBy,
                        DuplicateFrom = _PolicyRecord.DuplicateFrom,
                        CreatedOn = DateTime.Now,
                        IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule,
                        IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule,
                        SplitPercentage = _PolicyRecord.SplitPercentage,
                        Advance = _PolicyRecord.Advance,
                        ProductType = _PolicyRecord.ProductType,
                    };
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();
                    DataModel.AddToPoliciesHistories(_policy);

                }
                else
                {
                    _policy.PolicyId = _PolicyRecord.PolicyId;
                    _policy.PolicyNumber = _PolicyRecord.PolicyNumber;
                    _policy.PolicyType = _PolicyRecord.PolicyType;
                    _policy.Insured = _PolicyRecord.Insured;
                    _policy.OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate;
                    _policy.TrackFromDate = _PolicyRecord.TrackFromDate;
                    _policy.MonthlyPremium = _PolicyRecord.ModeAvgPremium;
                    _policy.SubmittedThrough = _PolicyRecord.SubmittedThrough;
                    _policy.Enrolled = _PolicyRecord.Enrolled;
                    _policy.Eligible = _PolicyRecord.Eligible;
                    _policy.PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate;
                    _policy.IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth;
                    _policy.IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage;
                    _policy.IsTrackPayment = _PolicyRecord.IsTrackPayment;
                    _policy.IsDeleted = _PolicyRecord.IsDeleted;
                    _policy.ReplacedBy = _PolicyRecord.ReplacedBy;
                    _policy.DuplicateFrom = _PolicyRecord.DuplicateFrom;
                    _policy.CreatedOn = DateTime.Now;
                    _policy.IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule;
                    _policy.IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule;
                    _policy.SplitPercentage = _PolicyRecord.SplitPercentage;

                    _policy.ProductType = _PolicyRecord.ProductType;

                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();

                }
                DataModel.SaveChanges();
            }
        }

        public static void AddUpdatePolicyHistory(DLinq.Policy _PolicyRecord, DLinq.CommissionDepartmentEntities DataModel)
        {
            try
            {
                List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesPost = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(_PolicyRecord.PolicyId);
                if (_PolicyPaymentEntriesPost.Count != 0) return;

                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _PolicyRecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null)
                {
                    _policy = new DLinq.PoliciesHistory
                    {
                        PolicyId = _PolicyRecord.PolicyId,
                        PolicyNumber = _PolicyRecord.PolicyNumber,
                        PolicyType = _PolicyRecord.PolicyType,
                        Insured = _PolicyRecord.Insured,
                        OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate,
                        TrackFromDate = _PolicyRecord.TrackFromDate,
                        MonthlyPremium = _PolicyRecord.MonthlyPremium,
                        SubmittedThrough = _PolicyRecord.SubmittedThrough,
                        Enrolled = _PolicyRecord.Enrolled,
                        Eligible = _PolicyRecord.Eligible,
                        PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate,
                        IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth,
                        IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage,
                        IsTrackPayment = _PolicyRecord.IsTrackPayment,
                        IsDeleted = false,
                        ReplacedBy = _PolicyRecord.ReplacedBy,
                        DuplicateFrom = _PolicyRecord.DuplicateFrom,
                        CreatedOn = DateTime.Now,
                        IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule,
                        IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule,
                        SplitPercentage = _PolicyRecord.SplitPercentage,
                        ProductType = _PolicyRecord.ProductType,
                    };
                    _policy.MasterPolicyStatuReference.Value = _PolicyRecord.MasterPolicyStatu;//(from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = _PolicyRecord.Client;// (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = _PolicyRecord.Licensee;// (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = _PolicyRecord.MasterPolicyMode;// (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = _PolicyRecord.Coverage;// (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = _PolicyRecord.MasterPolicyTerminationReason;// (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = _PolicyRecord.MasterIncomingPaymentType;// (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = _PolicyRecord.Payor;// (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = _PolicyRecord.UserCredential;// (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = _PolicyRecord.Carrier;// (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();
                    DataModel.AddToPoliciesHistories(_policy);

                }
                else
                {
                    _policy.PolicyId = _PolicyRecord.PolicyId;
                    _policy.PolicyNumber = _PolicyRecord.PolicyNumber;
                    _policy.PolicyType = _PolicyRecord.PolicyType;
                    _policy.Insured = _PolicyRecord.Insured;
                    _policy.OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate;
                    _policy.TrackFromDate = _PolicyRecord.TrackFromDate;
                    _policy.MonthlyPremium = _PolicyRecord.MonthlyPremium;
                    _policy.SubmittedThrough = _PolicyRecord.SubmittedThrough;
                    _policy.Enrolled = _PolicyRecord.Enrolled;
                    _policy.Eligible = _PolicyRecord.Eligible;
                    _policy.PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate;
                    _policy.IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth;
                    _policy.IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage;
                    _policy.IsTrackPayment = _PolicyRecord.IsTrackPayment;
                    _policy.IsDeleted = _PolicyRecord.IsDeleted;
                    _policy.ReplacedBy = _PolicyRecord.ReplacedBy;
                    _policy.DuplicateFrom = _PolicyRecord.DuplicateFrom;
                    _policy.CreatedOn = DateTime.Now;
                    _policy.IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule;
                    _policy.IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule;
                    _policy.SplitPercentage = _PolicyRecord.SplitPercentage;

                    _policy.ProductType = _PolicyRecord.ProductType;

                    _policy.MasterPolicyStatuReference.Value = _PolicyRecord.MasterPolicyStatu;//(from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = _PolicyRecord.Client;// (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = _PolicyRecord.Licensee;// (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = _PolicyRecord.MasterPolicyMode;// (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = _PolicyRecord.Coverage;// (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = _PolicyRecord.MasterPolicyTerminationReason;// (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = _PolicyRecord.MasterIncomingPaymentType;// (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = _PolicyRecord.Payor;// (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = _PolicyRecord.UserCredential;// (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = _PolicyRecord.Carrier;// (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();
                }
                DataModel.SaveChanges();

            }
            catch(Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail("AddUpdatePolicyHistory :" + ex.InnerException.ToString(), true);
            }
        }
        
        public static void AddUpdatePolicyHistoryNotCheckPayment(Guid PolicyId)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("PolicyId", PolicyId);
            PolicyDetailsData _PolicyRecord = GetPolicyData(parameters).FirstOrDefault();
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _PolicyRecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null)
                {
                    _policy = new DLinq.PoliciesHistory
                    {
                        PolicyId = _PolicyRecord.PolicyId,
                        PolicyNumber = _PolicyRecord.PolicyNumber,
                        PolicyType = _PolicyRecord.PolicyType,
                        Insured = _PolicyRecord.Insured,
                        OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate,
                        TrackFromDate = _PolicyRecord.TrackFromDate,
                        MonthlyPremium = _PolicyRecord.ModeAvgPremium,
                        SubmittedThrough = _PolicyRecord.SubmittedThrough,
                        Enrolled = _PolicyRecord.Enrolled,
                        Eligible = _PolicyRecord.Eligible,
                        PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate,
                        IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth,
                        IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage,
                        IsTrackPayment = _PolicyRecord.IsTrackPayment,
                        IsDeleted = false,
                        ReplacedBy = _PolicyRecord.ReplacedBy,
                        DuplicateFrom = _PolicyRecord.DuplicateFrom,
                        CreatedOn = DateTime.Now,
                        IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule,
                        IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule,
                        SplitPercentage = _PolicyRecord.SplitPercentage,
                    };
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();
                    DataModel.AddToPoliciesHistories(_policy);

                }
                else
                {
                    _policy.PolicyId = _PolicyRecord.PolicyId;
                    _policy.PolicyNumber = _PolicyRecord.PolicyNumber;
                    _policy.PolicyType = _PolicyRecord.PolicyType;
                    _policy.Insured = _PolicyRecord.Insured;
                    _policy.OriginalEffectiveDate = _PolicyRecord.OriginalEffectiveDate;
                    _policy.TrackFromDate = _PolicyRecord.TrackFromDate;
                    _policy.MonthlyPremium = _PolicyRecord.ModeAvgPremium;
                    _policy.SubmittedThrough = _PolicyRecord.SubmittedThrough;
                    _policy.Enrolled = _PolicyRecord.Enrolled;
                    _policy.Eligible = _PolicyRecord.Eligible;
                    _policy.PolicyTerminationDate = _PolicyRecord.PolicyTerminationDate;
                    _policy.IsTrackMissingMonth = _PolicyRecord.IsTrackMissingMonth;
                    _policy.IsTrackIncomingPercentage = _PolicyRecord.IsTrackIncomingPercentage;
                    _policy.IsTrackPayment = _PolicyRecord.IsTrackPayment;
                    _policy.IsDeleted = _PolicyRecord.IsDeleted;
                    _policy.ReplacedBy = _PolicyRecord.ReplacedBy;
                    _policy.DuplicateFrom = _PolicyRecord.DuplicateFrom;
                    _policy.CreatedOn = DateTime.Now;
                    _policy.IsIncomingBasicSchedule = _PolicyRecord.IsIncomingBasicSchedule;
                    _policy.IsOutGoingBasicSchedule = _PolicyRecord.IsOutGoingBasicSchedule;
                    _policy.SplitPercentage = _PolicyRecord.SplitPercentage;
                    _policy.MasterPolicyStatuReference.Value = (from m in DataModel.MasterPolicyStatus where m.PolicyStatusId == _PolicyRecord.PolicyStatusId select m).FirstOrDefault();
                    _policy.ClientReference.Value = (from s in DataModel.Clients where s.ClientId == _PolicyRecord.ClientId select s).FirstOrDefault();
                    _policy.LicenseeReference.Value = (from l in DataModel.Licensees where l.LicenseeId == _PolicyRecord.PolicyLicenseeId select l).FirstOrDefault();
                    _policy.MasterPolicyModeReference.Value = (from m in DataModel.MasterPolicyModes where m.PolicyModeId == _PolicyRecord.PolicyModeId select m).FirstOrDefault();
                    _policy.CoverageReference.Value = (from s in DataModel.Coverages where s.CoverageId == _PolicyRecord.CoverageId select s).FirstOrDefault();
                    _policy.MasterPolicyTerminationReasonReference.Value = (from s in DataModel.MasterPolicyTerminationReasons where s.PTReasonId == _PolicyRecord.TerminationReasonId select s).FirstOrDefault();
                    _policy.MasterIncomingPaymentTypeReference.Value = (from m in DataModel.MasterIncomingPaymentTypes where m.IncomingPaymentTypeId == _PolicyRecord.IncomingPaymentTypeId select m).FirstOrDefault();
                    _policy.PayorReference.Value = (from m in DataModel.Payors where m.PayorId == _PolicyRecord.PayorId select m).FirstOrDefault();
                    _policy.UserCredentialReference.Value = (from s in DataModel.UserCredentials where s.UserCredentialId == _PolicyRecord.CreatedBy select s).FirstOrDefault();
                    _policy.CarrierReference.Value = (from m in DataModel.Carriers where m.CarrierId == _PolicyRecord.CarrierID select m).FirstOrDefault();

                }
                DataModel.SaveChanges();
            }
        }

        public static PolicyDetailsData GetPolicyHistoryIdWise(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                PolicyDetailsData _Policy = (from pl in DataModel.PoliciesHistories
                                             where (pl.IsDeleted == false) && (pl.PolicyId == PolicyId)
                                             select new PolicyDetailsData
                                          {
                                              PolicyId = pl.PolicyId,
                                              PolicyNumber = pl.PolicyNumber,
                                              PolicyStatusId = pl.MasterPolicyStatu.PolicyStatusId,
                                              PolicyStatusName = pl.MasterPolicyStatu.Name,
                                              PolicyType = pl.PolicyType,
                                              PolicyLicenseeId = pl.Licensee.LicenseeId,
                                              Insured = pl.Insured,
                                              OriginalEffectiveDate = pl.OriginalEffectiveDate,
                                              TrackFromDate = pl.TrackFromDate,
                                              PolicyModeId = pl.MasterPolicyMode.PolicyModeId,
                                              ModeAvgPremium = pl.MonthlyPremium,
                                              SubmittedThrough = pl.SubmittedThrough,
                                              Enrolled = pl.Enrolled,
                                              Eligible = pl.Eligible,
                                              PolicyTerminationDate = pl.PolicyTerminationDate,
                                              TerminationReasonId = pl.TerminationReasonId,
                                              IsTrackMissingMonth = pl.IsTrackMissingMonth,
                                              IsTrackIncomingPercentage = pl.IsTrackIncomingPercentage,
                                              IsTrackPayment = pl.IsTrackPayment,
                                              IsDeleted = pl.IsDeleted,
                                              CarrierID = pl.Carrier.CarrierId == null ? Guid.Empty : pl.Carrier.CarrierId,
                                              CarrierName = pl.Carrier.CarrierName,
                                              CoverageId = pl.Coverage.CoverageId == null ? Guid.Empty : pl.Coverage.CoverageId,
                                              CoverageName = pl.Coverage.ProductName,
                                              ClientId = pl.Client.ClientId,
                                              ClientName = pl.Client.Name,
                                              ReplacedBy = pl.ReplacedBy,
                                              DuplicateFrom = pl.DuplicateFrom,
                                              IsIncomingBasicSchedule = pl.IsIncomingBasicSchedule,
                                              IsOutGoingBasicSchedule = pl.IsOutGoingBasicSchedule,
                                              PayorId = pl.Payor.PayorId == null ? Guid.Empty : pl.Payor.PayorId,
                                              PayorName = pl.Payor.PayorName,
                                              SplitPercentage = pl.SplitPercentage,
                                              IncomingPaymentTypeId = pl.IncomingPaymentTypeId,
                                              CreatedOn = pl.CreatedOn,
                                              CreatedBy = pl.CreatedBy.Value,//--always check it will never null
                                              IsSavedPolicy = true,

                                          }).FirstOrDefault();
                if (_Policy != null)
                {
                    _Policy.PolicyPreviousData = FillPolicyDetailPreviousData(_Policy);
                }
                
                return _Policy;
            }
        }

        public static void DeletePolicyHistory(PolicyDetailsData _policyrecord)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _policyrecord.PolicyId) select p).FirstOrDefault();
                if (_policy == null) return;
                _policy.IsDeleted = true;
                DataModel.SaveChanges();
            }
        }

        public static void DeletePolicyHistoryPermanentById(PolicyDetailsData _Policy)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                var _policy = (from p in DataModel.PoliciesHistories where (p.PolicyId == _Policy.PolicyId) select p).FirstOrDefault();
                if (_policy == null) return;
                DataModel.DeleteObject(_policy);
                DataModel.SaveChanges();
            }
        }

        public static PolicyDetailPreviousData FillPolicyDetailPreviousData(PolicyDetailsData _Policy)
        {
            PolicyDetailPreviousData _PolicyDetailPreviousData = new PolicyDetailPreviousData();
            try
            {
                if (_Policy != null)
                {

                    _PolicyDetailPreviousData.OriginalEffectiveDate = _Policy.OriginalEffectiveDate;
                    _PolicyDetailPreviousData.PolicyModeId = _Policy.PolicyModeId;
                    _PolicyDetailPreviousData.TrackFromDate = _Policy.TrackFromDate;
                    _PolicyDetailPreviousData.PolicyTermdateDate = _Policy.PolicyTerminationDate;
                }
            }
            catch (Exception)
            {
            }
           
          
            return _PolicyDetailPreviousData;
        }

        public static PolicyDetailMasterData GetPolicyDetailMasterData()
        {
            PolicyDetailMasterData pdMasterData = new PolicyDetailMasterData();
            pdMasterData.Statuses = PolicyStatus.GetPolicyStatusList();
            pdMasterData.Modes = PolicyMode.GetPolicyModeListWithBlankAdded();
            pdMasterData.TerminationReasons = PolicyTerminationReason.GetTerminationReasonListWithBlankAdded();
            pdMasterData.IncomingPaymentTypes = PolicyIncomingPaymentType.GetIncomingPaymentTypeList();
            pdMasterData.IssueCategories = IssueCategory.GetAllCategory();
            pdMasterData.IssueReasons = IssueReasons.GetAllReason();
            pdMasterData.IssueResults = IssueResults.GetAllResults();
            pdMasterData.IssueStatuses = IssueStatus.GetAllStatus();
            pdMasterData.LearnedMasterIncomingPaymentTypes = PolicyIncomingPaymentType.GetIncomingPaymentTypeList(); ;
            pdMasterData.LearnedMasterPaymentsModes = PolicyMode.GetPolicyModeListWithBlankAdded();
            pdMasterData.IncomingAdvanceScheduleTypes = PolicyIncomingScheduleType.GetIncomingScheduleTypeList();
            pdMasterData.OutgoingAdvanceScheduleTypes = PolicyOutgoingScheduleType.GetOutgoingScheduleTypeList();

            return pdMasterData;
        }

        #endregion
    }


}
