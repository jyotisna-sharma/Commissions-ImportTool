using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary;
using System.ServiceModel;

namespace MyAgencyVault.WcfService
{
    [ServiceContract]
    interface IOutgoingPayment
    {
        [OperationContract]
        void AddUpdateOutgoingPayment(List<OutGoingPayment> GisgoingSchedule, bool IsCustomSchedule = false);
        
        [OperationContract]
        void DeleteOutgoingPayment(OutGoingPayment OutGoingPymnt);
        
        [OperationContract]
        List<OutGoingPayment> GetOutgoingPayments();

        [OperationContract]
        List<OutGoingPayment> GetOutgoingSheduleForPolicy(Guid PolicyId);

        [OperationContract]
        bool CheckIsPaymentFromDEUForOutgoingPaymentID(Guid OutgoingPaymentid);

        [OperationContract]
        PolicyOutgoingDistribution GetOutgoingPaymentByID(Guid OutgoingPaymentid);
    }

    public partial class MavService : IOutgoingPayment
    {
        public void AddUpdateOutgoingPayment(List<OutGoingPayment> GisgoingSchedule, bool IsCustomSchedule=false)
        {
            try
            {
                if (GisgoingSchedule != null)
                {
                    ActionLogger.Logger.WriteImportLog("AddUpdateOutgoingPayment, new schedule: " + GisgoingSchedule.ToStringDump(), true);
                }
            }
            catch(Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("AddUpdateOutgoingPayment request log not written: " + ex.Message, true);
            }
           
            OutGoingPayment.AddUpdate(GisgoingSchedule, IsCustomSchedule);
        }

        public void DeleteOutgoingPayment(OutGoingPayment OutGoingPymnt)
        {
            try
            {
                if (OutGoingPymnt != null)
                {
                    ActionLogger.Logger.WriteImportLog("DeleteOutgoingPayment schedule: " + OutGoingPymnt.ToStringDump(), true);
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteImportLog("DeleteOutgoingPayment request log not written: " + ex.Message, true);
            }
            OutGoingPymnt.Delete();
        }

        public List<OutGoingPayment> GetOutgoingPayments()
        {
            return OutGoingPayment.GetOutgoingShedule();
        }

        public List<OutGoingPayment> GetOutgoingSheduleForPolicy(Guid PolicyId)
        {
            List <OutGoingPayment> lstPayments = OutGoingPayment.GetOutgoingSheduleForPolicy(PolicyId);
            if(lstPayments != null)
            {
                try
                {
                    ActionLogger.Logger.WriteImportLog("GetOutgoingSheduleForPolicy PolicyID: " + PolicyId + ", Schedule: " + lstPayments.ToStringDump(), true);
                }
                catch(Exception ex)
                {
                    ActionLogger.Logger.WriteImportLog("GetOutgoingSheduleForPolicynot error for  PolicyID: " + PolicyId + ", Exception: " + ex.Message, true);
                }
            }
            return lstPayments;
        }

        public bool CheckIsPaymentFromDEUForOutgoingPaymentID(Guid OutgoingPaymentid)
        {
            return PolicyOutgoingDistribution.CheckIsPaymentFromDEU(OutgoingPaymentid);
        }

        public PolicyOutgoingDistribution GetOutgoingPaymentByID(Guid OutgoingPaymentid)
        {
            return PolicyOutgoingDistribution.GetOutgoingPaymentById(OutgoingPaymentid);
        }
    }
}
