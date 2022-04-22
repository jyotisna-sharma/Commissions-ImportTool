using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using MyAgencyVault.BusinessLibrary.Base;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.Data.SqlClient;

namespace MyAgencyVault.BusinessLibrary
{
    [DataContract]
    public class PolicyToolIncommingShedule : PayorIncomingSchedule, IEditable<PolicyToolIncommingShedule>
    {
        public PolicyToolIncommingShedule()
        {
            this.Mode = Mode.Standard;
            this.CustomType = CustomMode.Graded;
        }

        #region "data members aka - public properties"
        [DataMember]
        public Guid IncomingScheduleId { get; set; }
        [DataMember]
        public Guid PolicyId { get; set; }
        // [DataMember]
        //public double? FirstYearPercentage { get; set; }
        //[DataMember]
        //public double? RenewalPercentage { get; set; }
        //[DataMember]
        //public double? SplitPercentage { get; set; }
        //[DataMember]
        //public int ScheduleTypeId { get; set; }
        #endregion
        #region IEditable<IncomingSchedule> Members

        public void AddUpdate()
        {
            try
            {
                if (this != null)
                {
                    ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " AddUpdate Incoming schedule request: " + this.ToStringDump(), true);
                }
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {

                    DLinq.PolicyIncomingSchedule PolicyIncomingScheduleDetails = (from e in DataModel.PolicyIncomingSchedules
                                                                                  where e.PolicyId == this.PolicyId
                                                                                  select e).FirstOrDefault();

                    if (PolicyIncomingScheduleDetails == null)
                    {
                        PolicyIncomingScheduleDetails = new DLinq.PolicyIncomingSchedule
                        {
                            FirstYearPercentage = this.FirstYearPercentage,
                            RenewalPercentage = this.RenewalPercentage,
                            IncomingScheduleId = this.IncomingScheduleId
                        };
                        PolicyIncomingScheduleDetails.PolicyReference.Value = (from inc in DataModel.Policies where inc.PolicyId == this.PolicyId select inc).FirstOrDefault();
                        PolicyIncomingScheduleDetails.MasterBasicIncomingScheduleReference.Value = (from inc in DataModel.MasterBasicIncomingSchedules where inc.ScheduleId == this.ScheduleTypeId select inc).FirstOrDefault();
                        DataModel.AddToPolicyIncomingSchedules(PolicyIncomingScheduleDetails);


                    }
                    else
                    {
                        PolicyIncomingScheduleDetails.FirstYearPercentage = this.FirstYearPercentage;
                        PolicyIncomingScheduleDetails.RenewalPercentage = this.RenewalPercentage;
                        PolicyIncomingScheduleDetails.MasterBasicIncomingScheduleReference.Value = (from inc in DataModel.MasterBasicIncomingSchedules where inc.ScheduleId == this.ScheduleTypeId select inc).FirstOrDefault();
                    }

                    DataModel.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " AddUpdate Incoming schedule error: " + ex.Message, true);
            }
        }

        public void Delete()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyIncomingSchedule _PInc = (from n in DataModel.PolicyIncomingSchedules
                                                      where (n.IncomingScheduleId == this.IncomingScheduleId)
                                                      select n).FirstOrDefault();
                DataModel.DeleteObject(_PInc);
                DataModel.SaveChanges();
            }

        }
        public static void DeleteSchedule(Guid PolicyId)
        {
            try
            {
                PolicyToolIncommingShedule _PolicyIncomingSchedule = GetPolicyToolIncommingSheduleListPolicyWise(PolicyId);
                if (_PolicyIncomingSchedule == null) return;
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    DLinq.PolicyIncomingSchedule _PInc = (from n in DataModel.PolicyIncomingSchedules
                                                          where (n.IncomingScheduleId == _PolicyIncomingSchedule.IncomingScheduleId)
                                                          select n).FirstOrDefault();
                    if (_PInc == null) return;
                    DataModel.DeleteObject(_PInc);
                    DataModel.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail("Exception in delete incomng schedule : " + ex.Message, true);
            }
        }

        public PolicyToolIncommingShedule GetOfID()
        {
            throw new NotImplementedException();
        }

        public static void SavePolicyIncomingSchedule(PolicyToolIncommingShedule policyIncomingSchedule)
        {
            PolicyToolIncommingShedule policyData = new PolicyToolIncommingShedule();
            try
            {
                ActionLogger.Logger.WriteImportLog("SavePolicyIncomingSchedule:Processing begins with IncomingScheduleId" + policyIncomingSchedule.ToStringDump(), true);

                using (SqlConnection con = new SqlConnection(DBConnection.GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("usp_SavePolicyIncomingScheule", con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IncomingScheduleID", policyIncomingSchedule.IncomingScheduleID);
                        cmd.Parameters.AddWithValue("@PolicyId", policyIncomingSchedule.PolicyId);
                        cmd.Parameters.AddWithValue("@FirstYearPercentage", policyIncomingSchedule.FirstYearPercentage);
                        cmd.Parameters.AddWithValue("@RenewalPercentage", policyIncomingSchedule.RenewalPercentage);
                        cmd.Parameters.AddWithValue("@ScheduleTypeId", policyIncomingSchedule.ScheduleTypeId);
                        cmd.Parameters.AddWithValue("@Mode", policyIncomingSchedule.Mode);
                        cmd.Parameters.AddWithValue("@CustomType", policyIncomingSchedule.CustomType);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
             /*   Won't be used from imort tool 
              *   if (policyIncomingSchedule.Mode == Mode.Custom)
                {
                    if (policyIncomingSchedule.CustomType == CustomMode.Graded)
                    {
                        SaveGradedSchedule(policyIncomingSchedule, policyIncomingSchedule.PolicyId);
                    }
                    else
                    {
                        SaveNonGradedSchedule(policyIncomingSchedule, policyIncomingSchedule.PolicyId);
                    }
                }*/
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("SavePolicyIncomingSchedule:Exception occurs while processing with IncomingScheduleId" + policyIncomingSchedule.IncomingScheduleID + " " + ex.Message, true);
                throw ex;
            }
        }
        public static List<PolicyToolIncommingShedule> GetPolicyToolIncommingSheduleList(Guid? PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<PolicyToolIncommingShedule> _PolicyToolIncommingShedule;
                _PolicyToolIncommingShedule = (from hd in DataModel.PolicyIncomingSchedules
                                               where hd.PolicyId == PolicyId
                                               select new PolicyToolIncommingShedule
                                               {
                                                   FirstYearPercentage = hd.FirstYearPercentage,
                                                   RenewalPercentage = hd.RenewalPercentage,
                                                   PolicyId = hd.Policy.PolicyId,
                                                   ScheduleTypeId = hd.MasterBasicIncomingSchedule.ScheduleId,
                                                   IncomingScheduleId = hd.IncomingScheduleId,
                                               }).ToList();
                return _PolicyToolIncommingShedule;
            }
        }
        public static List<PolicyToolIncommingShedule> GetPolicyToolIncommingSheduleList()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                List<PolicyToolIncommingShedule> _PolicyToolIncommingShedule;
                _PolicyToolIncommingShedule = (from hd in DataModel.PolicyIncomingSchedules
                                               select new PolicyToolIncommingShedule
                                               {
                                                   FirstYearPercentage = hd.FirstYearPercentage,
                                                   RenewalPercentage = hd.RenewalPercentage,
                                                   PolicyId = hd.Policy.PolicyId,
                                                   ScheduleTypeId = hd.MasterBasicIncomingSchedule.ScheduleId,
                                                   IncomingScheduleId = hd.IncomingScheduleId,
                                               }).ToList();
                return _PolicyToolIncommingShedule;
            }
        }

        public static PolicyToolIncommingShedule GetPolicyToolIncommingSheduleListPolicyWise(Guid PolicyId)
        {
            PolicyToolIncommingShedule PolicyToolIncommingSheduleLst = GetPolicyToolIncommingSheduleList(PolicyId).FirstOrDefault();
            return PolicyToolIncommingSheduleLst;
        }
        public bool IsValid()
        {
            throw new NotImplementedException();
        }
        #endregion

    }

}
