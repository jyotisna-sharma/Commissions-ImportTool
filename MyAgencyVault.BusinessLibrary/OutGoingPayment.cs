using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary.Base;
using System.Runtime.Serialization;
using DLinq = DataAccessLayer.LinqtoEntity;


namespace MyAgencyVault.BusinessLibrary
{
    public class OutGoingPayment
    {
        #region "Data members aka - public properties"
        [DataMember]
        public Guid OutgoingScheduleId { get; set; }
        [DataMember]
        public Guid PolicyId { get; set; }
        [DataMember]
        public string Payor { get; set; }
        [DataMember]
        public Guid PayeeUserCredentialId { get; set; }
        [DataMember]
        public double? FirstYearPercentage { get; set; }
        [DataMember]
        public double? RenewalPercentage { get; set; }
        [DataMember]
        public bool IsPrimaryAgent { get; set; }

        [DataMember]
        public bool IsEditDisable { get; set; }

        
        [DataMember]
        public int ScheduleTypeId { get; set; }
        [DataMember]
        public DateTime? CreatedOn { get; set; }

        //Acme- new parameters for  custom split feature 
        [DataMember]
        public DateTime? CustomStartDate { get; set; }
        [DataMember]
        public DateTime? CustomEndDate { get; set; }
        //[DataMember]
        //public string CustomDateType { get; set; }
        [DataMember]
        public double? SplitPercent { get; set; }

        [DataMember]
        public int? TierNumber { get; set; }

        #endregion

        #region IEditable<OutgoingPayment> Members
        public void AddUpdate()
        {
            throw new NotImplementedException();
        }

        public static void AddUpdate(List<OutGoingPayment> GlobalOutPayment, bool IsCustomSchedule = false)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyOutgoingSchedule gcs = null;

                foreach (OutGoingPayment _globalCoveragesShedule in GlobalOutPayment)
                {
                    gcs = (from e in DataModel.PolicyOutgoingSchedules
                           where e.OutgoingScheduleId == _globalCoveragesShedule.OutgoingScheduleId
                           select e).FirstOrDefault();
                    if (gcs == null)
                    {
                        gcs = new DLinq.PolicyOutgoingSchedule
                        {
                            OutgoingScheduleId = _globalCoveragesShedule.OutgoingScheduleId,
                            FirstYearPercentage = (IsCustomSchedule) ? 0 :  _globalCoveragesShedule.FirstYearPercentage,
                            RenewalPercentage = (IsCustomSchedule) ? 0 : _globalCoveragesShedule.RenewalPercentage,
                      //      IsPrimaryAgent = _globalCoveragesShedule.IsPrimaryAgent,
                            ScheduleTypeId = _globalCoveragesShedule.ScheduleTypeId,
                            CreatedOn=DateTime.Today,
                            CustomStartDate = (IsCustomSchedule) ? _globalCoveragesShedule.CustomStartDate : null,
                            CustomEndDate = (IsCustomSchedule) ? _globalCoveragesShedule.CustomEndDate : null,
                            SplitPercent = (IsCustomSchedule) ? _globalCoveragesShedule.SplitPercent : 0,

                        };

                        gcs.PolicyReference.Value = (from f in DataModel.Policies where f.PolicyId == _globalCoveragesShedule.PolicyId select f).FirstOrDefault();
                        //gcs.UserDetailReference.Value = _objPayee;
                        gcs.UserCredentialReference.Value = (from f in DataModel.UserCredentials where f.UserCredentialId == _globalCoveragesShedule.PayeeUserCredentialId select f).FirstOrDefault();
                        DataModel.AddToPolicyOutgoingSchedules(gcs);

                    }
                    else
                    {
                        gcs.FirstYearPercentage = (IsCustomSchedule) ? 0 : _globalCoveragesShedule.FirstYearPercentage;
                        gcs.RenewalPercentage = (IsCustomSchedule) ? 0 : _globalCoveragesShedule.RenewalPercentage;
                       // gcs.IsPrimaryAgent = _globalCoveragesShedule.IsPrimaryAgent;
                        gcs.ScheduleTypeId = _globalCoveragesShedule.ScheduleTypeId;
                        gcs.CustomStartDate = (IsCustomSchedule) ? _globalCoveragesShedule.CustomStartDate : null;
                          gcs.CustomEndDate = (IsCustomSchedule) ? _globalCoveragesShedule.CustomEndDate : null;
                          gcs.SplitPercent = (IsCustomSchedule) ? _globalCoveragesShedule.SplitPercent : 0;
                    }
                    DataModel.SaveChanges();
                }
              

            }

        }

        public void Delete()
        {
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Delete started in outgoingPayment.cs - OutgoingScheduleId: " + this.OutgoingScheduleId, true);
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyOutgoingSchedule _policyOutgoingSchedule = (from n in DataModel.PolicyOutgoingSchedules
                                                                        where (n.OutgoingScheduleId == this.OutgoingScheduleId)
                                                                        select n).FirstOrDefault();

                if (_policyOutgoingSchedule != null)
                {
                    DataModel.DeleteObject(_policyOutgoingSchedule);
                    DataModel.SaveChanges();
                }
            }
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Delete ended in outgoingPayment.cs - OutgoingScheduleId: " + this.OutgoingScheduleId, true);
        }

        #endregion

        public static int getIncomingScheduleTypeId(Guid PolicyId)
        {
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " getIncomingScheduleTypeId start", true);

            var incomingScheduleTypeId = 1;
            try
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    incomingScheduleTypeId = (int)DataModel.PolicyIncomingSchedules.Where(a => a.PolicyId == PolicyId).FirstOrDefault().ScheduleTypeId;
                }
                ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " getIncomingScheduleTypeId incomingScheduleTypeId " + incomingScheduleTypeId, true);
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " Exception getIncomingScheduleTypeId " + ex.Message, true);
            }
            return incomingScheduleTypeId;
        }

        public static List<OutGoingPayment> GetOutgoingShedule()
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                return (from gc in DataModel.PolicyOutgoingSchedules
                        select new OutGoingPayment
                        {
                            RenewalPercentage = gc.RenewalPercentage,
                            FirstYearPercentage = gc.FirstYearPercentage,
                         //   IsPrimaryAgent = gc.IsPrimaryAgent,
                            OutgoingScheduleId = gc.OutgoingScheduleId,
                            PolicyId = gc.Policy.PolicyId,
                            PayeeUserCredentialId = gc.UserCredential.UserCredentialId,
                            ScheduleTypeId = gc.ScheduleTypeId.Value,
                            CreatedOn = gc.CreatedOn,
                            Payor = gc.UserCredential.UserDetail.NickName,                           
                        }).ToList();
            }
        }

        /// <summary>
        /// Gets schedule based on invoice/entered date
        ///Fetch only those schedules where invoice/entered date falls in one of the date ranges 
        /// </summary>
        /// <param name="PolicyId"></param>
        /// <param name="DateType"></param>
        /// <param name="Invoice"></param>
        /// <param name="Entered"></param>
        /// <returns></returns>
        public static List<OutGoingPayment> GetCustomScheduleForPolicy(Guid PolicyId, string DateType, DateTime? Invoice, DateTime? Entered)
        {
            ActionLogger.Logger.WriteImportLogDetail("GetCustomScheduleForPolicy:Data Come from import tool service  processing  starts with PolicyId: " + PolicyId, true);
            if (!string.IsNullOrEmpty(DateType))
            {
                using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
                {
                    if (DateType.ToLower() == "invoice")
                    {
                        ActionLogger.Logger.WriteImportLogDetail("GetCustomScheduleForPolicy:Data Come from import tool service  processing  inprogress with DateType: " + DateType, true);
                        return (from gc in DataModel.PolicyOutgoingSchedules
                                where (gc.PolicyId == PolicyId && 
                                           ( (Invoice>= gc.CustomStartDate && Invoice<= gc.CustomEndDate) ||
                                             (Invoice >= gc.CustomStartDate && gc.CustomEndDate  == null)
                                            )
                                  )
                                select new OutGoingPayment
                                {
                                    RenewalPercentage = gc.RenewalPercentage,
                                    FirstYearPercentage = gc.FirstYearPercentage,
                                //    IsPrimaryAgent = gc.IsPrimaryAgent,
                                    OutgoingScheduleId = gc.OutgoingScheduleId,
                                    PolicyId = gc.Policy.PolicyId,
                                    PayeeUserCredentialId = gc.UserCredential.UserCredentialId,
                                    ScheduleTypeId = gc.ScheduleTypeId.Value,
                                    CreatedOn = gc.CreatedOn,
                                    Payor = gc.UserCredential.UserDetail.NickName,
                                    CustomStartDate = gc.CustomStartDate,
                                    CustomEndDate = gc.CustomEndDate,
                                    SplitPercent = (gc.SplitPercent == null) ? 0 : (double)(gc.SplitPercent),
                                    TierNumber = gc.TierNumber
                                }).ToList();
                    }
                    else
                    {
                        ActionLogger.Logger.WriteImportLogDetail("GetCustomScheduleForPolicy:Data Come from import tool service  processing  inprogress without DateType-invoice: " + DateType, true);
                        return (from gc in DataModel.PolicyOutgoingSchedules
                                where (gc.PolicyId == PolicyId &&
                                           ((Entered >= gc.CustomStartDate && Entered <= gc.CustomEndDate) ||
                                             (Entered >= gc.CustomStartDate && gc.CustomEndDate == null)
                                            ))
                                select new OutGoingPayment
                                {
                                    RenewalPercentage = gc.RenewalPercentage,
                                    FirstYearPercentage = gc.FirstYearPercentage,
                                   // IsPrimaryAgent = gc.IsPrimaryAgent,
                                    OutgoingScheduleId = gc.OutgoingScheduleId,
                                    PolicyId = gc.Policy.PolicyId,
                                    PayeeUserCredentialId = gc.UserCredential.UserCredentialId,
                                    ScheduleTypeId = gc.ScheduleTypeId.Value,
                                    CreatedOn = gc.CreatedOn,
                                    Payor = gc.UserCredential.UserDetail.NickName,
                                    CustomStartDate = gc.CustomStartDate,
                                    CustomEndDate = gc.CustomEndDate,
                                    SplitPercent = (gc.SplitPercent == null) ? 0 : Convert.ToDouble(gc.SplitPercent),
                                    TierNumber = gc.TierNumber
                                }).ToList();
                    }
                }
            }
            else
            {
                ActionLogger.Logger.WriteImportLogDetail("GetCustomScheduleForPolicy:Data Come from import tool service  processing  inprogress with PolicyId: " + PolicyId+ "DateType:"+ DateType , true);
                return null;
            }

        }

        /// <summary>
        /// Acme - changes to include custom split 
        /// </summary>
        /// <param name="PolicyId"></param>
        /// <returns></returns>
        public static List<OutGoingPayment> GetOutgoingSheduleForPolicy(Guid PolicyId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                return (from gc in DataModel.PolicyOutgoingSchedules
                        where (gc.PolicyId == PolicyId)
                        select new OutGoingPayment
                        {
                            RenewalPercentage = gc.RenewalPercentage,
                            FirstYearPercentage = gc.FirstYearPercentage,
                        //    IsPrimaryAgent = gc.IsPrimaryAgent,
                            OutgoingScheduleId = gc.OutgoingScheduleId,
                            PolicyId = gc.Policy.PolicyId,
                            PayeeUserCredentialId = gc.UserCredential.UserCredentialId,
                            ScheduleTypeId = gc.ScheduleTypeId.Value,
                            CreatedOn = gc.CreatedOn,
                            Payor = gc.UserCredential.UserDetail.NickName, 
                            CustomStartDate = gc.CustomStartDate,
                            CustomEndDate = gc.CustomEndDate,
                            SplitPercent = (gc.SplitPercent == null )? 0 :(double)(gc.SplitPercent),
                            TierNumber = gc.TierNumber
                        }).ToList();
            }
        }

        public void DeleteSchedule(List<OutGoingPayment> DeleteOutPayment)
        {
           
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                DLinq.PolicyOutgoingSchedule gcs = null;
                foreach (OutGoingPayment _out in DeleteOutPayment)
                {
                    ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeleteSchedule started in outgoingPayment.cs - OutgoingScheduleId: " + _out.OutgoingScheduleId, true);
                    gcs = (from e in DataModel.PolicyOutgoingSchedules
                           where e.OutgoingScheduleId == _out.OutgoingScheduleId
                           select e).FirstOrDefault();
                    if (gcs != null)
                    {
                        DataModel.DeleteObject(gcs);
                        DataModel.SaveChanges();
                    }
                    ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeleteSchedule ended in outgoingPayment.cs - OutgoingScheduleId: " + _out.OutgoingScheduleId, true);
                }

            }
        }

        public static void DeletePolicyOutGoingSchedulebyPolicyId(Guid PolicyId)
        {
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeletePolicyOutGoingSchedulebyPolicyId started in outgoingPayment.cs - PolicyId: " + PolicyId, true);
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
               List< DLinq.PolicyOutgoingSchedule> gcs = DataModel.PolicyOutgoingSchedules.Where(p => p.PolicyId == PolicyId).ToList <DLinq.PolicyOutgoingSchedule>();
                foreach (DLinq.PolicyOutgoingSchedule _out in gcs)
                {
                  
                    DataModel.DeleteObject(_out);
                        DataModel.SaveChanges();
                }
            }
            ActionLogger.Logger.WriteImportLogDetail(DateTime.Now.ToString() + " DeletePolicyOutGoingSchedulebyPolicyId ended in outgoingPayment.cs - PolicyId: " + PolicyId, true);
        }

        public static bool IsUserPresentAsPayee(Guid userId)
        {
            using (DLinq.CommissionDepartmentEntities DataModel = Entity.DataModel)
            {
                int count = DataModel.PolicyOutgoingSchedules.Where(s => s.PayeeUserCredentialId == userId).Count();
                if (count != 0)
                    return true;
                else
                    return false;
            }
        }
    }
}
